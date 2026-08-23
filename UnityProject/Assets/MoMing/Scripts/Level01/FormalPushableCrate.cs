using UnityEngine;

/// <summary>
/// 物理推箱：未挂点时 kinematic 完全不可推动；挂点后切换为动态刚体，
/// 速度驱动 + 墙体自然阻挡（不会穿墙）。前方 BoxCast 检测到障碍时报告 Blocked。
/// requiredPushers=2 时需要人类和狗同时挂点才能推动（协作箱/柜子）。
/// axisMode=Auto 时移动方向由挂点位置推导，支持推（W）和拉（S）；固定轴模式用于柜子等单向机关。
/// </summary>
[RequireComponent(typeof(Rigidbody), typeof(BoxCollider))]
public class FormalPushableCrate : MonoBehaviour, IFormalLevelTemporaryState, IFormalPushMover
{
    public enum PushAxisMode { Auto, PlusX, MinusX, PlusZ, MinusZ }

    [SerializeField] private float movementSpeed = 2f;
    [SerializeField] private float interactionRange = 2.5f;
    [SerializeField] private int requiredPushers = 1;
    [SerializeField] private Transform[] interactionPoints = new Transform[4];
    [SerializeField] private float blockProbeSkin = 0.12f;
    [SerializeField] private PushAxisMode axisMode = PushAxisMode.Auto;
    [Tooltip("沿推轴最大位移，0 = 不限制")]
    [SerializeField] private float travelLimit = 0f;

    private Rigidbody body;
    private BoxCollider box;
    private FormalPlayerActor human;
    private FormalPlayerActor dog;
    private int humanPoint = -1;
    private int dogPoint = -1;
    private Vector3 initialPosition;
    private Quaternion initialRotation;
    private Vector3 engageOrigin;

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

    void FixedUpdate()
    {
        if (!IsEngaged)
            return;

        KeepActorsAtPoints();
        Vector3 axis = ResolvePushAxis();
        if (axis.sqrMagnitude < 0.01f)
            return;

        bool enoughPushers = CountAttached() >= Mathf.Max(1, requiredPushers);
        float inputSign = human != null ? ResolveInputDirection(axis) : 0f;
        if (inputSign == 0f || !enoughPushers)
        {
            IsBlocked = false;
            return;
        }

        Vector3 moveAxis = axis * inputSign;
        if (inputSign > 0f && travelLimit > 0f && Vector3.Dot(transform.position - engageOrigin, axis) >= travelLimit)
        {
            IsBlocked = true;
            body.velocity = new Vector3(0f, body.velocity.y, 0f);
            return;
        }

        IsBlocked = ProbeBlocked(moveAxis);
        if (IsBlocked)
        {
            body.velocity = new Vector3(0f, body.velocity.y, 0f);
            return;
        }

        Vector3 velocity = moveAxis * movementSpeed;
        body.velocity = new Vector3(velocity.x, body.velocity.y, velocity.z);
    }

    public bool TryEngage(FormalPlayerActor actor)
    {
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
        actor.LockMoverInteraction(transform.position);
        actor.SetPosition(GetPointPosition(point));
        if (human == actor)
        {
            engageOrigin = transform.position;
            body.isKinematic = false;
        }
        return true;
    }

    public void Cancel()
    {
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
            human.SetMoverIdle();
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
    }

    void KeepActorsAtPoints()
    {
        if (human != null && humanPoint >= 0)
            human.SetPosition(GetPointPosition(humanPoint));
        if (dog != null && dogPoint >= 0)
            dog.SetPosition(GetPointPosition(dogPoint));
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
    /// 把相机相对输入解析成沿轴的移动方向：+1 推（沿轴离开人）、-1 拉（沿轴朝向人）、0 无有效输入。
    /// 固定轴模式（柜子等单向机关）只允许推。
    /// </summary>
    float ResolveInputDirection(Vector3 axis)
    {
        float vertical = Input.GetAxisRaw("Vertical");
        float horizontal = Input.GetAxisRaw("Horizontal");
        Vector3 input = new Vector3(horizontal, 0f, vertical);
        if (input.sqrMagnitude < 0.01f)
            return 0f;

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
        if (world.sqrMagnitude < 0.01f)
            return 0f;

        world.Normalize();
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
        Collider actorCollider = actor.GetComponent<Collider>();
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
