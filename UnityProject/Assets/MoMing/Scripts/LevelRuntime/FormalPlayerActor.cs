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

    private Rigidbody body;
    private CapsuleCollider capsule;
    private Animator animator;
    private ActorState state;
    private bool animationInitialized;
    private bool moverRotationLocked;
    private Quaternion moverRotation;
    private RigidbodyConstraints constraintsBeforeMoverLock;
    private float nextIdleVariationTime;
    private bool idleVariation;
    private bool moverAttachPointResolved;

    public ActorRole Role => role;
    public ActorState State => state;

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

        if (state == ActorState.Idle && role == ActorRole.Human && Time.time >= nextIdleVariationTime)
        {
            idleVariation = !idleVariation;
            nextIdleVariationTime = Time.time + 4f;
            PlayAnimationForState();
        }
    }

    public void Move(Vector3 direction, bool sprint)
    {
        if (moverRotationLocked)
            return;

        bool sprinting = sprint && role == ActorRole.Human;
        float speed = sprinting ? sprintSpeed : walkSpeed;
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
        if (!canJump || !IsGrounded())
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
            state == ActorState.Walking || state == ActorState.Sprinting ? "Walk" :
            idleVariation ? "Idle2" : "Idle1";
        float blendTime = state == ActorState.Jumping ? 0.12f : 0.25f;
        string resolvedState = ResolveAnimationState(stateName);
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
        if (animator != null)
            animator.speed = state == ActorState.Sprinting ? 1.5f : 1f;
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
        Physics.SyncTransforms();
    }
}
