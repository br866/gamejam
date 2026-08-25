using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class FormalCooperativeRailMover : MonoBehaviour, IFormalLevelTemporaryState, IFormalPushMover
{
    [System.Serializable]
    public class DirectionPointGroup
    {
        public Transform humanPoint;
    }

    [SerializeField] private DirectionPointGroup[] directionGroups = new DirectionPointGroup[4];
    [SerializeField] private float movementSpeed = 2f;
    [SerializeField] private float interactionRange = 2.5f;
    [SerializeField] private float sideTolerance = 0.55f;

    [Header("Wwise Audio")]
    [Tooltip("Play_Crate_Push: start transient followed by the continuous pushing loop.")]
    [SerializeField] private AK.Wwise.Event playPushEvent = new AK.Wwise.Event();
    [Tooltip("Stop_Crate_Push: stops Play_Crate_Push and plays the release tail.")]
    [SerializeField] private AK.Wwise.Event stopPushEvent = new AK.Wwise.Event();
    [Tooltip("Keeps very short input gaps from repeatedly retriggering the start and stop tails.")]
    [SerializeField, Min(0f)] private float pushAudioStopDelay = 0.1f;

    private Rigidbody body;
    private Vector3 initialPosition;
    private Vector3 movementOrigin;
    private Quaternion initialRotation;
    private float travel;
    private FormalPlayerActor human;
    private int humanGroup = -1;
    private int movementAxis = -1;
    private bool pushAudioPlaying;
    private float pushAudioStopRemaining;
    private uint pushAudioPlayingId = AkUnitySoundEngine.AK_INVALID_PLAYING_ID;
    private bool warnedMissingPlayPushEvent;
    private bool warnedMissingStopPushEvent;

    public bool IsEngaged => human != null;
    public bool IsAttached(FormalPlayerActor actor) => actor != null && actor == human;

    void Awake()
    {
        body = GetComponent<Rigidbody>();
        initialPosition = transform.position;
        movementOrigin = initialPosition;
        initialRotation = transform.rotation;
        body.isKinematic = false;
        body.velocity = Vector3.zero;
        body.angularVelocity = Vector3.zero;
        body.isKinematic = true;
        body.interpolation = RigidbodyInterpolation.None;

        FormalLevelController level = FormalLevelActors.FindLevelController(gameObject.scene);
        if (level != null)
            level.RegisterTemporaryState(this);
    }

    public bool TryEngage(FormalPlayerActor actor)
    {
        if (!enabled)
            return false;

        if (actor == null || IsAttached(actor))
        {
            Debug.LogWarning($"[FormalCooperativeRailMover] F engage rejected: actor={(actor != null ? actor.name : "null")}, alreadyAttached={IsAttached(actor)}");
            return false;
        }

        int group = FindMatchingGroup(actor);
        if (group < 0)
        {
            Debug.LogWarning($"[FormalCooperativeRailMover] F engage rejected: actor={actor.name}, actorPosition={actor.transform.position}, cratePosition={transform.position}, no matching interaction point within range={interactionRange:F2}");
            return false;
        }

        if (!CanUseGroup(actor, group))
        {
            Debug.LogWarning($"[FormalCooperativeRailMover] F engage rejected: actor={actor.name}, group={group}, role={actor.Role}, group unavailable");
            return false;
        }

        if (actor.Role != FormalPlayerActor.ActorRole.Human)
        {
            Debug.LogWarning($"[FormalCooperativeRailMover] F engage rejected: actor={actor.name}, role={actor.Role}; only Human can attach");
            return false;
        }

        human = actor;
        humanGroup = group;
        movementAxis = humanGroup < 2 ? 0 : 1;
        movementOrigin = transform.position;
        travel = 0f;

        IgnoreMoverCollision(actor, true);
        actor.LockMoverInteraction(transform.position);
        KeepAttachedActorsAtNodes();
        Debug.Log($"[FormalCooperativeRailMover] F engage succeeded: actor={actor.name}, group={humanGroup}, movementAxis={(movementAxis == 0 ? "X" : "Z")}, origin={movementOrigin}");
        return true;
    }

    public void Move(Vector3 worldDirection)
    {
        Move(worldDirection, Vector3.Dot(worldDirection, GetMovementAxis()) >= 0f);
    }

    public void Move(Vector3 worldDirection, bool pushingAnimation)
    {
        if (!IsEngaged || worldDirection.sqrMagnitude < 0.01f)
        {
            UpdatePushAudio(false);
            return;
        }

        Vector3 axis = GetMovementAxis();
        if (axis.sqrMagnitude < 0.01f)
        {
            UpdatePushAudio(false);
            return;
        }
        float direction = Vector3.Dot(worldDirection, axis);
        if (Mathf.Abs(direction) < 0.01f)
        {
            // Keep the active actor's WASD input useful even when the camera is
            // looking across the rail instead of along it.
            Vector3 fallback = worldDirection;
            fallback.y = 0f;
            direction = Mathf.Abs(fallback.x) >= Mathf.Abs(fallback.z)
                ? fallback.x
                : fallback.z;
        }

        if (Mathf.Abs(direction) < 0.01f)
        {
            UpdatePushAudio(false);
            return;
        }

        direction = Mathf.Sign(direction);

        float prospectiveTravel = travel + direction * movementSpeed * Time.fixedDeltaTime;
        Vector3 targetPosition = movementOrigin + axis * prospectiveTravel;
        Vector3 step = targetPosition - transform.position;

        // 墙体阻挡：Transform 位移不经过物理解算，必须先探测目标位置是否穿墙。
        if (step.sqrMagnitude > 0.000001f && ProbeBlocked(step.normalized, step.magnitude + 0.05f))
        {
            UpdateAttachedAnimations(pushingAnimation);
            UpdatePushAudio(false);
            return;
        }

        travel = prospectiveTravel;
        transform.position = targetPosition;
        Physics.SyncTransforms();
        KeepAttachedActorsAtNodes();
        UpdateAttachedAnimations(pushingAnimation);
        UpdatePushAudio(true);
    }

    void Update()
    {
        // Death freezes the physics loop before it can reach the normal stop path.
        if (pushAudioPlaying && FormalDeathScreen.IsShowing)
            StopPushAudioImmediate();
    }

    bool ProbeBlocked(Vector3 direction, float distance)
    {
        BoxCollider box = GetComponent<BoxCollider>();
        if (box == null)
            return false;

        Vector3 half = Vector3.Scale(box.size * 0.5f, transform.lossyScale);
        Vector3 origin = transform.position + Vector3.up * 0.1f;
        RaycastHit hit;
        if (Physics.BoxCast(origin, Vector3.Scale(half, new Vector3(0.9f, 0.9f, 0.9f)), direction, out hit, transform.rotation, distance))
        {
            // IgnoreCollision 只影响接触解算，不影响 BoxCast；挂点角色随箱移动，需排除。
            if (hit.collider != null && !hit.collider.isTrigger
                && !hit.collider.transform.IsChildOf(transform)
                && !IsAttachedRider(hit.collider))
                return true;
        }
        return false;
    }

    bool IsAttachedRider(Collider collider)
    {
        return human != null && collider.transform.IsChildOf(human.transform);
    }

    public void SetAttachedPushAnimation()
    {
        if (human != null)
            human.SetMoverInteraction(true, transform.position);
    }

    public void SetAttachedPullAnimation()
    {
        if (human != null)
            human.SetMoverInteraction(false, transform.position);
    }

    public void SetAttachedIdleAnimation()
    {
        if (human != null)
            human.SetMoverIdle();
    }

    public void Cancel()
    {
        StopPushAudioImmediate();
        Detach(human);
        human = null;
        humanGroup = -1;
        movementAxis = -1;
        if (!body.isKinematic)
        {
            body.velocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
        }
    }

    void OnDisable()
    {
        StopPushAudioImmediate();
    }

    void UpdatePushAudio(bool moving)
    {
        if (moving)
        {
            pushAudioStopRemaining = pushAudioStopDelay;
            if (pushAudioPlaying)
                return;

            if (playPushEvent != null && playPushEvent.IsValid())
            {
                pushAudioPlayingId = playPushEvent.Post(gameObject);
                pushAudioPlaying = pushAudioPlayingId != AkUnitySoundEngine.AK_INVALID_PLAYING_ID;
            }
            else if (!warnedMissingPlayPushEvent)
            {
                Debug.LogWarning("[FormalCooperativeRailMover] Play_Crate_Push is not assigned.", this);
                warnedMissingPlayPushEvent = true;
            }
            return;
        }

        if (!pushAudioPlaying)
            return;

        pushAudioStopRemaining -= Time.fixedDeltaTime;
        if (pushAudioStopRemaining <= 0f)
            StopPushAudioImmediate();
    }

    void StopPushAudioImmediate()
    {
        pushAudioStopRemaining = 0f;
        if (!pushAudioPlaying)
            return;

        if (stopPushEvent != null && stopPushEvent.IsValid())
        {
            stopPushEvent.Post(gameObject);
        }
        else
        {
            if (pushAudioPlayingId != AkUnitySoundEngine.AK_INVALID_PLAYING_ID)
            {
                AkUnitySoundEngine.ExecuteActionOnPlayingID(
                    AkActionOnEventType.AkActionOnEventType_Stop,
                    pushAudioPlayingId,
                    0,
                    AkCurveInterpolation.AkCurveInterpolation_Linear);
            }

            if (!warnedMissingStopPushEvent)
            {
                Debug.LogWarning("[FormalCooperativeRailMover] Stop_Crate_Push is not assigned; stopped the loop without its release tail.", this);
                warnedMissingStopPushEvent = true;
            }
        }

        pushAudioPlaying = false;
        pushAudioPlayingId = AkUnitySoundEngine.AK_INVALID_PLAYING_ID;
    }

    public void ResetTemporaryState()
    {
        Cancel();
        travel = 0f;
        movementOrigin = initialPosition;
        body.position = initialPosition;
        body.rotation = initialRotation;
        transform.SetPositionAndRotation(initialPosition, initialRotation);
        Physics.SyncTransforms();
    }

    void KeepAttachedActorsAtNodes()
    {
        if (human != null && humanGroup >= 0)
            human.SnapToMoverPoint(GetGroupPoint(humanGroup, human.Role));
    }

    void Detach(FormalPlayerActor actor)
    {
        if (actor == null)
            return;

        IgnoreMoverCollision(actor, false);
        actor.ReleaseMoverInteraction();
        actor.Stop();
    }

    void IgnoreMoverCollision(FormalPlayerActor actor, bool ignore)
    {
        Collider actorCollider = actor.GetComponentInChildren<Collider>();
        if (actorCollider == null)
            return;

        foreach (Collider moverCollider in GetComponentsInChildren<Collider>())
            if (moverCollider != null && !moverCollider.isTrigger)
                Physics.IgnoreCollision(actorCollider, moverCollider, ignore);
    }

    Vector3 GetGroupPoint(int group, FormalPlayerActor.ActorRole role)
    {
        DirectionPointGroup points = GetGroup(group);
        Transform point = points?.humanPoint;
        return point != null ? point.position : transform.position;
    }

    int FindMatchingGroup(FormalPlayerActor actor)
    {
        Vector3 actorOffset = actor.transform.position - transform.position;
        actorOffset.y = 0f;

        Debug.Log($"[FormalCooperativeRailMover] F matching start: actorOffset={actorOffset}, actorFlatDistance={actorOffset.magnitude:F3}, sideTolerance={sideTolerance:F3}, interactionRange={interactionRange:F3}");

        int bestGroup = -1;
        float bestDot = -1f;
        for (int group = 0; group < 4; group++)
        {
            DirectionPointGroup points = GetGroup(group);
            if (points == null)
            {
                Debug.Log($"[FormalCooperativeRailMover] F matching group={group}: group=null");
                continue;
            }

            Vector3 groupOffset = GetGroupCenter(points) - transform.position;
            groupOffset.y = 0f;
            if (groupOffset.sqrMagnitude < 0.01f)
            {
                Debug.Log($"[FormalCooperativeRailMover] F matching group={group}: centerOffset={groupOffset}, invalid center direction");
                continue;
            }

            float dot = actorOffset.sqrMagnitude < 0.01f
                ? 1f
                : Vector3.Dot(actorOffset.normalized, groupOffset.normalized);
            Vector3 pointPosition = GetGroupPoint(group, actor.Role);
            float pointDistance = FlatDistance(actor.transform.position, pointPosition);
            Debug.Log($"[FormalCooperativeRailMover] F matching group={group}: point={pointPosition}, centerOffset={groupOffset}, pointDistance={pointDistance:F3}, dot={dot:F3}, sidePass={dot >= sideTolerance}, rangePass={pointDistance <= interactionRange}");
            if (dot > bestDot)
            {
                bestDot = dot;
                bestGroup = group;
            }
        }

        if (bestGroup < 0 || bestDot < sideTolerance)
        {
            Debug.Log($"[FormalCooperativeRailMover] F matching rejected by side: bestGroup={bestGroup}, bestDot={bestDot:F3}, sideTolerance={sideTolerance:F3}");
            return -1;
        }

        Vector3 bestPoint = GetGroupPoint(bestGroup, actor.Role);
        float bestDistance = FlatDistance(actor.transform.position, bestPoint);
        bool inRange = bestDistance <= interactionRange;
        Debug.Log($"[FormalCooperativeRailMover] F matching selected: group={bestGroup}, point={bestPoint}, distance={bestDistance:F3}, range={interactionRange:F3}, inRange={inRange}");
        return inRange ? bestGroup : -1;
    }

    bool CanUseGroup(FormalPlayerActor actor, int group)
    {
        if (actor.Role == FormalPlayerActor.ActorRole.Human && humanGroup >= 0)
            return false;
        return actor.Role == FormalPlayerActor.ActorRole.Human;
    }

    void UpdateAttachedAnimations(bool pushing)
    {
        if (human != null)
        {
            if (pushing)
                human.SetMoverInteraction(true, transform.position);
            else
                human.SetMoverIdle();
        }
    }

    Vector3 GetMovementAxis()
    {
        Vector3 axis = GetGroupDirection(movementAxis == 0 ? 0 : 2);
        axis.y = 0f;
        return axis.sqrMagnitude > 0.01f ? axis.normalized : Vector3.zero;
    }

    DirectionPointGroup GetGroup(int group)
    {
        return directionGroups != null && group >= 0 && group < directionGroups.Length
            ? directionGroups[group]
            : null;
    }

    Vector3 GetGroupCenter(DirectionPointGroup points)
    {
        if (points == null)
            return transform.position;

        Vector3 center = Vector3.zero;
        int count = 0;
        if (points.humanPoint != null) { center += points.humanPoint.position; count++; }
        return count > 0 ? center / count : transform.position;
    }

    Vector3 GetGroupDirection(int group)
    {
        Vector3 direction = GetGroupCenter(GetGroup(group)) - transform.position;
        direction.y = 0f;
        return direction.sqrMagnitude > 0.01f ? direction.normalized : Vector3.zero;
    }

    void GetHorizontalAxes(out Vector3 firstAxis, out Vector3 secondAxis)
    {
        Vector3[] candidates = { transform.right, transform.forward, transform.up };
        firstAxis = GetFlattestAxis(candidates, Vector3.zero);
        secondAxis = GetFlattestAxis(candidates, firstAxis);
    }

    static Vector3 GetFlattestAxis(Vector3[] candidates, Vector3 excludedAxis)
    {
        Vector3 best = Vector3.zero;
        float bestMagnitude = -1f;
        foreach (Vector3 candidate in candidates)
        {
            Vector3 flattened = candidate;
            flattened.y = 0f;
            float magnitude = flattened.sqrMagnitude;
            if (magnitude <= bestMagnitude || magnitude < 0.01f ||
                (excludedAxis.sqrMagnitude > 0.01f && Mathf.Abs(Vector3.Dot(flattened.normalized, excludedAxis)) > 0.9f))
                continue;

            best = flattened.normalized;
            bestMagnitude = magnitude;
        }

        return best;
    }

    static float FlatDistance(Vector3 first, Vector3 second)
    {
        first.y = 0f;
        second.y = 0f;
        return Vector3.Distance(first, second);
    }
}
