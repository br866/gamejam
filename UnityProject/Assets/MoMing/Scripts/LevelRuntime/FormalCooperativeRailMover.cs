using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class FormalCooperativeRailMover : MonoBehaviour, IFormalLevelTemporaryState
{
    [System.Serializable]
    public class DirectionPointGroup
    {
        public Transform humanPoint;
    }

    [SerializeField] private DirectionPointGroup[] directionGroups = new DirectionPointGroup[4];
    [SerializeField] private float minimumTravel = -2.5f;
    [SerializeField] private float maximumTravel = 4f;
    [SerializeField] private float movementSpeed = 2f;
    [SerializeField] private float interactionRange = 2.5f;
    [SerializeField] private float sideTolerance = 0.55f;

    private Rigidbody body;
    private Vector3 initialPosition;
    private Quaternion initialRotation;
    private float travel;
    private FormalPlayerActor human;
    private int humanGroup = -1;
    private int movementAxis = -1;

    public bool IsEngaged => human != null;
    public bool IsAttached(FormalPlayerActor actor) => actor != null && actor == human;

    void Awake()
    {
        body = GetComponent<Rigidbody>();
        initialPosition = transform.position;
        initialRotation = transform.rotation;
        body.isKinematic = false;
        body.velocity = Vector3.zero;
        body.angularVelocity = Vector3.zero;
        body.isKinematic = true;

        FormalLevelController level = FormalLevelActors.FindLevelController(gameObject.scene);
        if (level != null)
            level.RegisterTemporaryState(this);
    }

    void FixedUpdate()
    {
        KeepAttachedActorsAtNodes();
    }

    void LateUpdate()
    {
        // Animation is evaluated after physics; restore the authored point before rendering.
        KeepAttachedActorsAtNodes();
    }

    public bool TryEngage(FormalPlayerActor actor)
    {
        if (actor == null || IsAttached(actor))
            return false;

        int group = FindMatchingGroup(actor);
        if (group < 0 || !CanUseGroup(actor, group))
            return false;

        if (actor.Role != FormalPlayerActor.ActorRole.Human)
            return false;

        human = actor;
        humanGroup = group;
        movementAxis = humanGroup < 2 ? 0 : 1;

        IgnoreMoverCollision(actor, true);
        actor.LockMoverInteraction(transform.position);
        KeepAttachedActorsAtNodes();
        return true;
    }

    public void Move(Vector3 worldDirection)
    {
        Move(worldDirection, Vector3.Dot(worldDirection, GetMovementAxis()) >= 0f);
    }

    public void Move(Vector3 worldDirection, bool pushingAnimation)
    {
        if (!IsEngaged || worldDirection.sqrMagnitude < 0.01f)
            return;

        Vector3 axis = GetMovementAxis();
        if (axis.sqrMagnitude < 0.01f)
            return;
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
            return;

        direction = Mathf.Sign(direction);

        travel = Mathf.Clamp(travel + direction * movementSpeed * Time.fixedDeltaTime, minimumTravel, maximumTravel);
        Vector3 targetPosition = initialPosition + axis * travel;
        body.position = targetPosition;
        transform.position = targetPosition;
        Physics.SyncTransforms();
        KeepAttachedActorsAtNodes();
        UpdateAttachedAnimations(pushingAnimation);
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

    public void ResetTemporaryState()
    {
        Cancel();
        travel = 0f;
        body.position = initialPosition;
        body.rotation = initialRotation;
        transform.SetPositionAndRotation(initialPosition, initialRotation);
        Physics.SyncTransforms();
    }

    void KeepAttachedActorsAtNodes()
    {
        if (human != null && humanGroup >= 0)
            human.SetPosition(GetGroupPoint(humanGroup, human.Role));
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
        Collider actorCollider = actor.GetComponent<Collider>();
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
        if (actorOffset.sqrMagnitude < 0.01f)
            return -1;

        int bestGroup = -1;
        float bestDot = -1f;
        for (int group = 0; group < 4; group++)
        {
            DirectionPointGroup points = GetGroup(group);
            if (points == null)
                continue;

            Vector3 groupOffset = GetGroupCenter(points) - transform.position;
            groupOffset.y = 0f;
            if (groupOffset.sqrMagnitude < 0.01f)
                continue;

            float dot = Vector3.Dot(actorOffset.normalized, groupOffset.normalized);
            if (dot > bestDot)
            {
                bestDot = dot;
                bestGroup = group;
            }
        }

        if (bestGroup < 0 || bestDot < sideTolerance)
            return -1;

        return FlatDistance(actor.transform.position, GetGroupPoint(bestGroup, actor.Role)) <= interactionRange
            ? bestGroup
            : -1;
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
            human.SetMoverInteraction(pushing, transform.position);
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
