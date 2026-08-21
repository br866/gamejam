using UnityEngine;

/// <summary>
/// 物理推箱：动态 Rigidbody + 速度驱动，墙体自然阻挡（不会穿墙）。
/// 支持推不动：前方 BoxCast 检测到障碍时箱子不施力，并报告 Blocked。
/// requiredPushers=2 时需要人类和狗同时挂点才能推动（协作箱）。
/// </summary>
[RequireComponent(typeof(Rigidbody), typeof(BoxCollider))]
public class FormalPushableCrate : MonoBehaviour, IFormalLevelTemporaryState, IFormalPushMover
{
    [SerializeField] private float movementSpeed = 2f;
    [SerializeField] private float interactionRange = 2.5f;
    [SerializeField] private int requiredPushers = 1;
    [SerializeField] private Transform[] interactionPoints = new Transform[4];
    [SerializeField] private float blockProbeSkin = 0.12f;

    private Rigidbody body;
    private BoxCollider box;
    private FormalPlayerActor human;
    private FormalPlayerActor dog;
    private int humanPoint = -1;
    private int dogPoint = -1;
    private Vector3 initialPosition;
    private Quaternion initialRotation;

    public bool IsEngaged { get { return human != null; } }
    public bool IsBlocked { get; private set; }
    public bool IsAttached(FormalPlayerActor actor) { return actor != null && (actor == human || actor == dog); }

    void Awake()
    {
        body = GetComponent<Rigidbody>();
        box = GetComponent<BoxCollider>();
        initialPosition = transform.position;
        initialRotation = transform.rotation;
        body.isKinematic = false;
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

        bool hasHumanInput = human != null && HumanWantsMove(axis);
        bool enoughPushers = CountAttached() >= Mathf.Max(1, requiredPushers);
        if (!hasHumanInput || !enoughPushers)
        {
            IsBlocked = false;
            return;
        }

        IsBlocked = ProbeBlocked(axis);
        if (IsBlocked)
        {
            body.velocity = new Vector3(0f, body.velocity.y, 0f);
            return;
        }

        Vector3 velocity = axis * movementSpeed;
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
        body.velocity = new Vector3(0f, body.velocity.y, 0f);
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
        body.velocity = Vector3.zero;
        body.angularVelocity = Vector3.zero;
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
        if (humanPoint < 0)
            return Vector3.zero;
        Vector3 offset = GetPointPosition(humanPoint) - transform.position;
        offset.y = 0f;
        return offset.sqrMagnitude > 0.01f ? offset.normalized : Vector3.zero;
    }

    bool HumanWantsMove(Vector3 axis)
    {
        float vertical = Input.GetAxisRaw("Vertical");
        float horizontal = Input.GetAxisRaw("Horizontal");
        Vector3 input = new Vector3(horizontal, 0f, vertical);
        if (input.sqrMagnitude < 0.01f)
            return false;

        Camera camera = Camera.main;
        Vector3 forward = camera != null ? camera.transform.forward : Vector3.forward;
        forward.y = 0f;
        forward.Normalize();
        Vector3 right = camera != null ? camera.transform.right : Vector3.right;
        right.y = 0f;
        right.Normalize();
        Vector3 world = forward * vertical + right * horizontal;
        return world.sqrMagnitude > 0.01f && Vector3.Dot(world.normalized, axis) > 0.35f;
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
            if (hit.collider != null && !hit.collider.isTrigger && !hit.collider.transform.IsChildOf(transform))
                return true;
        }
        return false;
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
