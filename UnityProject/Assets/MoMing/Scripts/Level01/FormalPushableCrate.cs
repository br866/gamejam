using UnityEngine;

/// <summary>
/// 物理推箱：未挂点时 kinematic 完全不可推动；挂点后切换为动态刚体，
/// 速度驱动 + 墙体自然阻挡（不会穿墙）。前方 BoxCast 检测到障碍时报告 Blocked。
/// requiredPushers=2 时需要人类和狗同时挂点才能推动（协作箱/柜子）。
/// axisMode=Auto 时移动方向由挂点位置推导，支持推（W）和拉（S）；固定轴模式用于柜子等单向机关。
/// axisMode=Free 时全向移动：人与挂点相对位置恒定随箱平移，箱子沿相机相对输入自由移动。
/// </summary>
[RequireComponent(typeof(Rigidbody), typeof(BoxCollider))]
public class FormalPushableCrate : MonoBehaviour, IFormalLevelTemporaryState, IFormalPushMover
{
    public enum PushAxisMode { Auto, PlusX, MinusX, PlusZ, MinusZ, Free }

    [SerializeField] private float movementSpeed = 2f;
    [SerializeField] private float interactionRange = 2.5f;
    [SerializeField] private int requiredPushers = 1;
    [SerializeField] private Transform[] interactionPoints = new Transform[4];
    [SerializeField] private float blockProbeSkin = 0.12f;
    [SerializeField] private PushAxisMode axisMode = PushAxisMode.Auto;

    [Header("Wwise Audio")]
    [Tooltip("Play_Crate_Push: start transient followed by the continuous pushing loop.")]
    [SerializeField] private AK.Wwise.Event playPushEvent = new AK.Wwise.Event();
    [Tooltip("Stop_Crate_Push: stops Play_Crate_Push and plays the release tail.")]
    [SerializeField] private AK.Wwise.Event stopPushEvent = new AK.Wwise.Event();
    [Tooltip("Keeps very short input gaps from repeatedly retriggering the start and stop tails.")]
    [SerializeField, Min(0f)] private float pushAudioStopDelay = 0.1f;

    private Rigidbody body;
    private BoxCollider box;
    private FormalPlayerActor human;
    private FormalPlayerActor dog;
    private int humanPoint = -1;
    private int dogPoint = -1;
    private Vector3 initialPosition;
    private Quaternion initialRotation;
    private bool pushAudioPlaying;
    private float pushAudioStopRemaining;
    private uint pushAudioPlayingId = AkUnitySoundEngine.AK_INVALID_PLAYING_ID;
    private bool warnedMissingPlayPushEvent;
    private bool warnedMissingStopPushEvent;

    public bool IsEngaged { get { return human != null; } }
    public bool IsBlocked { get; private set; }
    public bool IsAttached(FormalPlayerActor actor) { return actor != null && (actor == human || actor == dog); }

    void Awake()
    {
        body = GetComponent<Rigidbody>();
        box = GetComponent<BoxCollider>();
        initialPosition = transform.position;
        initialRotation = transform.rotation;
        body.isKinematic = true;
        body.useGravity = true;
        body.constraints = RigidbodyConstraints.FreezeRotation;
        body.interpolation = RigidbodyInterpolation.Interpolate;
        body.drag = 8f;

        FormalLevelController level = FormalLevelActors.FindLevelController(gameObject.scene);
        if (level != null)
            level.RegisterTemporaryState(this);
    }

    void Start()
    {
        // 开局就把箱子落到地面：手摆高度经常差一点，物理接管后会和地板穿透互搏抖动。
        SettleOnGround();
    }

    /// <summary>
    /// 垂直落地校正：X/Z 不动，把箱体底面贴到正下方支撑面(地面)上。
    /// 垂直位置从此由接触自然保持，不再依赖手摆的初始 Y 是否精确。
    /// </summary>
    void SettleOnGround()
    {
        if (box == null)
            return;

        Bounds bounds = box.bounds;
        Vector3 origin = new Vector3(bounds.center.x, bounds.max.y + 1f, bounds.center.z);
        float distance = bounds.size.y + 20f;
        RaycastHit[] hits = Physics.RaycastAll(origin, Vector3.down, distance, ~0, QueryTriggerInteraction.Ignore);

        // 取箱体下方最高的支撑面（跳过自身；叠在别的箱子/平台上也能正确落上去）。
        float supportY = float.NegativeInfinity;
        foreach (RaycastHit hit in hits)
        {
            if (hit.transform.IsChildOf(transform))
                continue;
            if (hit.point.y > bounds.max.y + 0.01f)
                continue;
            if (hit.point.y > supportY)
                supportY = hit.point.y;
        }
        if (supportY == float.NegativeInfinity)
            return;

        float delta = supportY - bounds.min.y + 0.01f;
        if (Mathf.Abs(delta) < 0.0001f)
            return;

        Vector3 settled = transform.position + Vector3.up * delta;
        body.position = settled;
        transform.SetPositionAndRotation(settled, transform.rotation);
        Physics.SyncTransforms();
    }

    void FixedUpdate()
    {
        if (!IsEngaged)
        {
            UpdatePushAudio(false);
            return;
        }

        KeepActorsAtPoints();

        Vector3 moveAxis;
        if (axisMode == PushAxisMode.Free)
        {
            // Free 模式 WASD 四个方向都能推，动画跟着实际输入走，不再只认 W。
            moveAxis = ResolveWorldInput();
            if (moveAxis.sqrMagnitude < 0.01f)
            {
                IsBlocked = false;
                ApplyAttachedAnimation(Vector3.zero);
                UpdatePushAudio(false);
                return;
            }
        }
        else
        {
            Vector3 axis = ResolvePushAxis();
            if (axis.sqrMagnitude < 0.01f)
            {
                ApplyAttachedAnimation(Vector3.zero);
                UpdatePushAudio(false);
                return;
            }

            float inputSign = ResolveInputDirection(axis);
            if (inputSign == 0f)
            {
                IsBlocked = false;
                ApplyAttachedAnimation(Vector3.zero);
                UpdatePushAudio(false);
                return;
            }
            moveAxis = axis * inputSign;
        }

        if (CountAttached() < Mathf.Max(1, requiredPushers))
        {
            IsBlocked = false;
            ApplyAttachedAnimation(Vector3.zero);
            UpdatePushAudio(false);
            return;
        }

        // 被墙挡住时也保持推的姿势：人在使劲，只是箱子推不动。
        ApplyAttachedAnimation(moveAxis);

        IsBlocked = ProbeBlocked(moveAxis);
        if (IsBlocked)
        {
            body.velocity = new Vector3(0f, body.velocity.y, 0f);
            UpdatePushAudio(false);
            return;
        }

        Vector3 velocity = moveAxis * movementSpeed;
        body.velocity = new Vector3(velocity.x, body.velocity.y, velocity.z);
        UpdatePushAudio(true);
    }

    void Update()
    {
        // Update still runs when the physics loop is suspended by blocking UI.
        if (pushAudioPlaying && !FormalGameplayState.CanSimulate)
            StopPushAudioImmediate();
    }

    /// <summary>
    /// 按本帧真正生效的移动方向决定挂点角色的动画：
    /// 没有有效输入 = 待机；箱子被推离人 = Push；箱子被拉向人 = Pull。
    /// 方向判定走「人 -> 箱」这条向量，所以 Free 模式下横着推（A/D）也会出推的动作。
    /// </summary>
    void ApplyAttachedAnimation(Vector3 moveDirection)
    {
        if (human == null)
            return;

        if (moveDirection.sqrMagnitude < 0.01f)
        {
            SetAttachedIdleAnimation();
            return;
        }

        Vector3 awayFromHuman = transform.position - GetPointPosition(humanPoint);
        awayFromHuman.y = 0f;
        if (awayFromHuman.sqrMagnitude > 0.01f &&
            Vector3.Dot(moveDirection.normalized, awayFromHuman.normalized) < -0.35f)
        {
            SetAttachedPullAnimation();
            return;
        }

        SetAttachedPushAnimation();
    }

    public bool TryEngage(FormalPlayerActor actor)
    {
        if (!enabled)
            return false;

        if (actor == null || IsAttached(actor))
            return false;

        int point = FindMatchingPoint(actor);
        if (point < 0)
            return false;

        if (actor.Role == FormalPlayerActor.ActorRole.Human)
        {
            if (human != null)
                return false;
            human = actor;
            humanPoint = point;
        }
        else
        {
            if (dog != null || !CooperationNeeded())
                return false;
            dog = actor;
            dogPoint = point;
        }

        IgnoreCrateCollision(actor, true);
        actor.Stop();
        actor.LockMoverInteraction(transform.position);
        actor.SnapToMoverPoint(GetPointPosition(point));
        SettleOnGround();
        if (human == actor)
            body.isKinematic = false;
        return true;
    }

    public void Cancel()
    {
        StopPushAudioImmediate();
        Detach(human);
        Detach(dog);
        human = null;
        dog = null;
        humanPoint = -1;
        dogPoint = -1;
        IsBlocked = false;
        if (!body.isKinematic)
            body.velocity = Vector3.zero;
        body.isKinematic = true;
    }

    void OnDisable()
    {
        StopPushAudioImmediate();
    }

    void UpdatePushAudio(bool moving)
    {
        moving = moving && FormalGameplayState.CanSimulate;

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
                Debug.LogWarning("[FormalPushableCrate] Play_Crate_Push is not assigned.", this);
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
                Debug.LogWarning("[FormalPushableCrate] Stop_Crate_Push is not assigned; stopped the loop without its release tail.", this);
                warnedMissingStopPushEvent = true;
            }
        }

        pushAudioPlaying = false;
        pushAudioPlayingId = AkUnitySoundEngine.AK_INVALID_PLAYING_ID;
    }

    public void Move(Vector3 worldDirection, bool pushingAnimation)
    {
        // 实际移动在 FixedUpdate 中由物理驱动；这里只同步动画状态。
        if (!IsEngaged)
            return;

        if (pushingAnimation && human != null)
            human.SetMoverInteraction(true, transform.position);
        else if (human != null)
            human.SetMoverIdle();
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

    public void ResetTemporaryState()
    {
        Cancel();
        body.position = initialPosition;
        body.rotation = initialRotation;
        transform.SetPositionAndRotation(initialPosition, initialRotation);
        Physics.SyncTransforms();
        SettleOnGround();
    }

    void KeepActorsAtPoints()
    {
        if (human != null && humanPoint >= 0)
            human.SnapToMoverPoint(GetPointPosition(humanPoint));
        if (dog != null && dogPoint >= 0)
            dog.SnapToMoverPoint(GetPointPosition(dogPoint));
    }

    Vector3 ResolvePushAxis()
    {
        switch (axisMode)
        {
            case PushAxisMode.PlusX:
                return Vector3.right;
            case PushAxisMode.MinusX:
                return Vector3.left;
            case PushAxisMode.PlusZ:
                return Vector3.forward;
            case PushAxisMode.MinusZ:
                return Vector3.back;
        }

        if (humanPoint < 0)
            return Vector3.zero;
        // 推箱方向：从人指向箱子再延伸，即箱子被推离人的方向。
        Vector3 offset = transform.position - GetPointPosition(humanPoint);
        offset.y = 0f;
        return offset.sqrMagnitude > 0.01f ? offset.normalized : Vector3.zero;
    }

    /// <summary>
    /// 解析相机相对的水平输入向量为归一化方向；无有效输入时返回零向量。
    /// </summary>
    Vector3 ResolveWorldInput()
    {
        float vertical = Input.GetAxisRaw("Vertical");
        float horizontal = Input.GetAxisRaw("Horizontal");
        if (new Vector3(horizontal, 0f, vertical).sqrMagnitude < 0.01f)
            return Vector3.zero;

        Camera camera = null;
        CameraFollow follow = FindObjectOfType<CameraFollow>();
        if (follow != null)
            camera = follow.GetComponent<Camera>();
        if (camera == null)
            camera = Camera.main;
        Vector3 forward = camera != null ? camera.transform.forward : Vector3.forward;
        forward.y = 0f;
        forward.Normalize();
        Vector3 right = camera != null ? camera.transform.right : Vector3.right;
        right.y = 0f;
        right.Normalize();
        Vector3 world = forward * vertical + right * horizontal;
        return world.sqrMagnitude > 0.01f ? world.normalized : Vector3.zero;
    }

    /// <summary>
    /// 把相机相对输入解析成沿轴的移动方向：+1 推（沿轴离开人）、-1 拉（沿轴朝向人）、0 无有效输入。
    /// 固定轴模式（柜子等单向机关）只允许推。
    /// </summary>
    float ResolveInputDirection(Vector3 axis)
    {
        Vector3 world = ResolveWorldInput();
        if (world.sqrMagnitude < 0.01f)
            return 0f;

        if (Vector3.Dot(world, axis) > 0.35f)
            return 1f;
        if (axisMode == PushAxisMode.Auto && Vector3.Dot(world, -axis) > 0.35f)
            return -1f;
        return 0f;
    }

    bool CooperationNeeded()
    {
        return Mathf.Max(1, requiredPushers) >= 2;
    }

    int CountAttached()
    {
        int count = 0;
        if (human != null) count++;
        if (dog != null) count++;
        return count;
    }

    bool ProbeBlocked(Vector3 axis)
    {
        Vector3 half = Vector3.Scale(box.size * 0.5f, transform.lossyScale);
        float distance = Mathf.Abs(Vector3.Dot(half, axis)) + blockProbeSkin;
        Vector3 origin = transform.position + Vector3.up * 0.1f;
        RaycastHit hit;
        if (Physics.BoxCast(origin, Vector3.Scale(half, new Vector3(0.9f, 0.9f, 0.9f)), axis, out hit, transform.rotation, distance))
        {
            // IgnoreCollision 只影响接触解算，不影响 BoxCast；挂点角色就在运动方向上，
            // 拉动时必须把他们排除掉，否则永远误报 Blocked。
            if (hit.collider != null && !hit.collider.isTrigger
                && !hit.collider.transform.IsChildOf(transform)
                && !IsAttachedRider(hit.collider))
                return true;
        }
        return false;
    }

    bool IsAttachedRider(Collider collider)
    {
        return (human != null && collider.transform.IsChildOf(human.transform))
            || (dog != null && collider.transform.IsChildOf(dog.transform));
    }

    int FindMatchingPoint(FormalPlayerActor actor)
    {
        int best = -1;
        float bestDistance = interactionRange;
        for (int i = 0; i < interactionPoints.Length; i++)
        {
            if (interactionPoints[i] == null)
                continue;

            float distance = FlatDistance(actor.transform.position, interactionPoints[i].position);
            if (distance > bestDistance)
                continue;

            best = i;
            bestDistance = distance;
        }
        return best;
    }

    Vector3 GetPointPosition(int index)
    {
        Transform point = index >= 0 && index < interactionPoints.Length ? interactionPoints[index] : null;
        return point != null ? point.position : transform.position;
    }

    void Detach(FormalPlayerActor actor)
    {
        if (actor == null)
            return;

        IgnoreCrateCollision(actor, false);
        actor.ReleaseMoverInteraction();
        actor.Stop();
    }

    void IgnoreCrateCollision(FormalPlayerActor actor, bool ignore)
    {
        Collider actorCollider = actor.GetComponentInChildren<Collider>();
        if (actorCollider == null)
            return;

        foreach (Collider crateCollider in GetComponentsInChildren<Collider>())
            if (crateCollider != null && !crateCollider.isTrigger)
                Physics.IgnoreCollision(actorCollider, crateCollider, ignore);
    }

    static float FlatDistance(Vector3 first, Vector3 second)
    {
        first.y = 0f;
        second.y = 0f;
        return Vector3.Distance(first, second);
    }
}
