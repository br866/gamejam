using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class FormalPlayerActor : MonoBehaviour
{
    public enum ActorRole { Human, Dog }
    public enum ActorState { Idle, Walking, Sprinting, Jumping, Linked, Pushing, Pulling }

    [SerializeField] private ActorRole role;
    [SerializeField] private float walkSpeed = 4f;
    [SerializeField] private float sprintSpeed = 7f;
    [SerializeField] private float turnSpeed = 12f;
    [SerializeField] private float jumpHeight = 2.2f;
    [SerializeField] private bool walkOnlyAnimation;
    [SerializeField] private bool canJump = true;
    [SerializeField] private Transform focusAnchor;
    [SerializeField] private Transform moverAttachPoint;

    [Header("Audio")]
    [Tooltip("Human 脚步触发的 Wwise Event；FormalHumanActor 使用 Play_Footstep_Human")]
    [SerializeField] private AK.Wwise.Event humanFootstepEvent = new AK.Wwise.Event();
    [Tooltip("Dog 脚步触发的 Wwise Event；FormalDogActor 使用 Play_Footstep_Dog")]
    [SerializeField] private AK.Wwise.Event dogFootstepEvent = new AK.Wwise.Event();
    [Tooltip("Walk 循环约 1.033 秒，4 m/s 下每半循环约移动 2.07 米")]
    [SerializeField] private float walkFootstepDistance = 2.07f;
    [Tooltip("Run 循环约 0.633 秒，7 m/s 下每半循环约移动 2.22 米")]
    [SerializeField] private float sprintFootstepDistance = 2.22f;
    [Tooltip("Dog Walk 循环约 1 秒，3 m/s 下每半循环约移动 1.5 米")]
    [SerializeField] private float dogWalkFootstepDistance = 1.5f;

    private Rigidbody body;
    private CapsuleCollider capsule;
    private Animator animator;
    private ActorState state;
    private bool animationInitialized;
    // 控制器里没有 Run 状态时，退回旧行为：播 Walk 并把播放速度调快
    private bool sprintFallsBackToWalk = true;
    private bool moverRotationLocked;
    private Quaternion moverRotation;
    private RigidbodyConstraints constraintsBeforeMoverLock;
    private float nextIdleVariationTime;
    private bool idleVariation;
    private bool moverAttachPointResolved;
    private Vector3 lastFootstepPosition;
    private bool hasWarnedMissingFootstepEvent;
    private int executionLockCount;
    private float runtimeMovementSpeedMultiplier = 1f;

    public ActorRole Role => role;
    public ActorState State => state;
    public bool IsExecutionLocked => executionLockCount > 0;
    public float ConfiguredWalkSpeed => walkSpeed;
    public float RuntimeMovementSpeedMultiplier => runtimeMovementSpeedMultiplier;

    /// <summary>相机注视点；未配置时回退到根节点。</summary>
    public Transform FocusAnchor => focusAnchor != null ? focusAnchor : transform;

    /// <summary>
    /// 根节点(脚底)到挂接锚点的局部偏移。所有"把角色放到某个点位"的摆放
    /// 都必须用点位减去该偏移，保证锚点与点位重合（脚底 pivot 约定）。
    /// 未配置时为零向量，行为退化为旧的"根节点直接落在点位"。
    /// </summary>
    public Vector3 MoverAttachOffset => moverAttachPoint != null ? moverAttachPoint.localPosition : Vector3.zero;

    /// <summary>
    /// 把挂接锚点对齐到世界坐标：优先用 Inspector 指定或名为 MoverAttachPoint 的子节点，
    /// 都不存在时退化为根节点直接落在点位。锚点引用只在首次调用时解析一次，不会每帧查找。
    /// </summary>
    public void SnapToMoverPoint(Vector3 worldPoint)
    {
        if (!moverAttachPointResolved)
        {
            moverAttachPointResolved = true;
            if (moverAttachPoint == null)
            {
                moverAttachPoint = transform.Find("MoverAttachPoint");
                if (moverAttachPoint == null)
                {
                    Transform[] children = GetComponentsInChildren<Transform>(true);
                    foreach (Transform child in children)
                    {
                        if (child.name == "MoverAttachPoint")
                        {
                            moverAttachPoint = child;
                            break;
                        }
                    }
                }
            }
        }

        Vector3 target = worldPoint;
        if (moverAttachPoint != null)
            target -= moverAttachPoint.position - transform.position;

        // 挂接吸附只接管 X/Z；Y 保持当前值且不清垂直速度，垂直交给重力与地面碰撞。
        // 若整只 SetPosition 硬传送，锚点高度和地面接触会每帧互相打架，角色就会弹来弹去。
        Vector3 velocity = body.velocity;
        target.y = transform.position.y;
        body.position = target;
        transform.SetPositionAndRotation(target, transform.rotation);
        if (moverRotationLocked)
            transform.rotation = moverRotation;
        body.velocity = velocity;
        Physics.SyncTransforms();
    }

    void Awake()
    {
        body = GetComponent<Rigidbody>();
        capsule = GetComponentInChildren<CapsuleCollider>();
        body.useGravity = true;
        body.constraints = RigidbodyConstraints.FreezeRotation;
        body.interpolation = RigidbodyInterpolation.Interpolate;
        body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        lastFootstepPosition = transform.position;
    }

    void Update()
    {
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
            if (animator != null)
            {
                animator.applyRootMotion = false;
                animationInitialized = true;
                PlayAnimationForState();
            }
        }

        ApplyAnimationState();
        HandleFootsteps();

        if (state == ActorState.Idle && role == ActorRole.Human && Time.time >= nextIdleVariationTime)
        {
            idleVariation = !idleVariation;
            nextIdleVariationTime = Time.time + 4f;
            PlayAnimationForState();
        }
    }

    void HandleFootsteps()
    {
        if (!IsGrounded())
            return;

        Vector3 current = transform.position;
        Vector3 previous = lastFootstepPosition;
        current.y = 0f;
        previous.y = 0f;
        bool isHuman = role == ActorRole.Human;
        float requiredDistance = isHuman
            ? state == ActorState.Sprinting ? sprintFootstepDistance : walkFootstepDistance
            : dogWalkFootstepDistance;
        if ((current - previous).sqrMagnitude < requiredDistance * requiredDistance)
            return;

        lastFootstepPosition = transform.position;
        AK.Wwise.Event footstepEvent = isHuman ? humanFootstepEvent : dogFootstepEvent;
        if (footstepEvent != null && footstepEvent.IsValid())
        {
            footstepEvent.Post(gameObject);
        }
        else if (!hasWarnedMissingFootstepEvent)
        {
            Debug.LogWarning($"FormalPlayerActor: {role} Footstep Event is not assigned.", this);
            hasWarnedMissingFootstepEvent = true;
        }
    }

    public void Move(Vector3 direction, bool sprint)
    {
        if (moverRotationLocked || IsExecutionLocked)
            return;

        bool sprinting = sprint && role == ActorRole.Human;
        float speed = sprinting ? sprintSpeed : walkSpeed;
        if (role == ActorRole.Dog)
            speed *= runtimeMovementSpeedMultiplier;
        body.velocity = new Vector3(direction.x * speed, ClampFall(body.velocity.y), direction.z * speed);
        SetState(!IsGrounded()
            ? ActorState.Jumping
            : direction.sqrMagnitude > 0.01f
                ? sprinting ? ActorState.Sprinting : ActorState.Walking
                : ActorState.Idle);

        if (direction.sqrMagnitude > 0.01f && !moverRotationLocked)
            body.MoveRotation(Quaternion.Slerp(body.rotation, Quaternion.LookRotation(direction), turnSpeed * Time.fixedDeltaTime));
    }

    public void Stop()
    {
        body.velocity = new Vector3(0f, body.velocity.y, 0f);
        SetState(IsGrounded() ? ActorState.Idle : ActorState.Jumping);
    }

    public void Jump()
    {
        if (IsExecutionLocked || !canJump || !IsGrounded())
            return;

        float jumpVelocity = Mathf.Sqrt(2f * Mathf.Abs(Physics.gravity.y) * jumpHeight);
        body.velocity = new Vector3(body.velocity.x, jumpVelocity, body.velocity.z);
        SetState(ActorState.Jumping);
    }

    static float ClampFall(float vertical)
    {
        // 限制最大下落速度，避免落地帧穿透地面。
        return Mathf.Max(vertical, -20f);
    }

    public void SetLinked(bool isMoving)
    {
        SetState(isMoving ? ActorState.Linked : ActorState.Idle);
    }

    public void SetPushing(bool isPushing)
    {
        SetState(isPushing ? ActorState.Pushing : ActorState.Idle);
    }

    public void SetMoverInteraction(bool pushing, Vector3 moverPosition)
    {
        LockMoverInteraction(moverPosition);
        SetState(pushing ? ActorState.Pushing : ActorState.Pulling);
    }

    public void SetMoverIdle()
    {
        if (moverRotationLocked)
            SetState(ActorState.Idle);
    }

    public void LockMoverInteraction(Vector3 moverPosition)
    {
        Vector3 lookDirection = moverPosition - transform.position;
        lookDirection.y = 0f;
        if (lookDirection.sqrMagnitude > 0.01f)
        {
            moverRotation = Quaternion.LookRotation(lookDirection);
            if (!moverRotationLocked)
            {
                moverRotationLocked = true;
                constraintsBeforeMoverLock = body.constraints;
                body.constraints = RigidbodyConstraints.FreezeRotation;
            }
            body.rotation = moverRotation;
            transform.rotation = moverRotation;
        }
    }

    public void ReleaseMoverInteraction()
    {
        if (moverRotationLocked)
            body.constraints = constraintsBeforeMoverLock;
        moverRotationLocked = false;
    }

    /// <summary>仅供测试工具临时调整；不会改写 Inspector 中配置的基础速度。</summary>
    public void SetRuntimeMovementSpeedMultiplier(float multiplier)
    {
        runtimeMovementSpeedMultiplier = float.IsNaN(multiplier) || float.IsInfinity(multiplier)
            ? 1f
            : Mathf.Max(0f, multiplier);
    }

    /// <summary>
    /// 暂时锁住处决目标的移动和互动。锁可重入，调用方必须成对释放。
    /// </summary>
    public void AcquireExecutionLock()
    {
        executionLockCount++;
        Stop();
    }

    public void ReleaseExecutionLock()
    {
        if (executionLockCount <= 0)
            return;

        executionLockCount--;
        if (!IsExecutionLocked)
            Stop();
    }

    void FixedUpdate()
    {
        if (!IsExecutionLocked)
            return;

        body.velocity = new Vector3(0f, body.velocity.y, 0f);
        body.angularVelocity = Vector3.zero;
    }

    void SetState(ActorState nextState)
    {
        if (state == nextState)
            return;

        state = nextState;
        PlayAnimationForState();
        ApplyAnimationState();
    }

    void PlayAnimationForState()
    {
        if (!animationInitialized || animator == null)
            return;

        string stateName = walkOnlyAnimation ? "Walk" :
            state == ActorState.Jumping ? "Jump" :
            state == ActorState.Pushing ? "Push" :
            state == ActorState.Pulling ? "Pull" :
            state == ActorState.Linked ? "Walk" :
            state == ActorState.Sprinting ? "Run" :
            state == ActorState.Walking ? "Walk" :
            idleVariation ? "Idle2" : "Idle1";
        float blendTime = state == ActorState.Jumping ? 0.12f : 0.25f;
        string resolvedState = ResolveAnimationState(stateName);

        // 冲刺：优先用独立的 Run 动画；控制器里没有的话再退回“Walk 加速”的老做法
        sprintFallsBackToWalk = false;
        if (resolvedState == null && stateName == "Run")
        {
            sprintFallsBackToWalk = true;
            stateName = "Walk";
            resolvedState = ResolveAnimationState(stateName);
        }

        if (resolvedState == null)
        {
            Debug.LogWarning("[FormalPlayerActor] " + name + " missing animation state " + stateName);
            return;
        }

        animator.CrossFadeInFixedTime(resolvedState, blendTime, 0, 0f);
        //Debug.Log("[FormalPlayerActor] " + name + " play animation " + resolvedState + " for " + state);
    }

    bool HasAnimationState(string stateName)
    {
        return ResolveAnimationState(stateName) != null;
    }

    string ResolveAnimationState(string stateName)
    {
        if (animator.HasState(0, Animator.StringToHash(stateName)))
            return stateName;

        string fullName = "Base Layer." + stateName;
        return animator.HasState(0, Animator.StringToHash(fullName)) ? fullName : null;
    }

    void ApplyAnimationState()
    {
        if (animator == null)
            return;

        // 有真正的 Run 动画时不要再加速，不然跑起来像快进
        bool speedUpWalk = state == ActorState.Sprinting && sprintFallsBackToWalk;
        animator.speed = speedUpWalk ? 1.5f : 1f;
    }

    bool IsGrounded()
    {
        if (capsule == null)
            return false;

        Bounds bounds = capsule.bounds;
        RaycastHit[] hits = Physics.RaycastAll(
            bounds.center,
            Vector3.down,
            bounds.extents.y + 0.08f,
            Physics.DefaultRaycastLayers,
            QueryTriggerInteraction.Ignore);

        foreach (RaycastHit hit in hits)
        {
            if (hit.collider != capsule && !hit.transform.IsChildOf(transform))
                return true;
        }

        return false;
    }

    public void SetPosition(Vector3 position)
    {
        SetPositionAndRotation(position, transform.rotation);
    }

    public void SetPositionAndRotation(Vector3 position, Quaternion rotation)
    {
        body.position = position;
        body.rotation = rotation;
        transform.SetPositionAndRotation(position, rotation);
        if (moverRotationLocked)
        {
            body.rotation = moverRotation;
            transform.rotation = moverRotation;
        }
        body.velocity = Vector3.zero;
        body.angularVelocity = Vector3.zero;
        lastFootstepPosition = position;
        Physics.SyncTransforms();
    }
}
