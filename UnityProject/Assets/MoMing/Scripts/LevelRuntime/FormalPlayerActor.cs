using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
public class FormalPlayerActor : MonoBehaviour
{
    public enum ActorRole { Human, Dog }
    public enum ActorState { Idle, Walking, Sprinting, Jumping, Linked, Pushing, Pulling }

    [SerializeField] private ActorRole role;
    [SerializeField] private float walkSpeed = 4f;
    [SerializeField] private float sprintSpeed = 7f;
    [SerializeField] private float turnSpeed = 12f;
    [SerializeField] private float jumpHeight = 2.2f;

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

    public ActorRole Role => role;
    public ActorState State => state;

    void Awake()
    {
        body = GetComponent<Rigidbody>();
        capsule = GetComponent<CapsuleCollider>();
        body.useGravity = true;
        body.constraints = RigidbodyConstraints.FreezeRotation;
        body.interpolation = RigidbodyInterpolation.Interpolate;
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

        float speed = sprint && role == ActorRole.Dog ? sprintSpeed : walkSpeed;
        body.velocity = new Vector3(direction.x * speed, body.velocity.y, direction.z * speed);
        SetState(!IsGrounded()
            ? ActorState.Jumping
            : direction.sqrMagnitude > 0.01f
                ? sprint && role == ActorRole.Dog ? ActorState.Sprinting : ActorState.Walking
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
        if (!IsGrounded())
            return;

        float jumpVelocity = Mathf.Sqrt(2f * Mathf.Abs(Physics.gravity.y) * jumpHeight);
        body.velocity = new Vector3(body.velocity.x, jumpVelocity, body.velocity.z);
        SetState(ActorState.Jumping);
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
                body.constraints = RigidbodyConstraints.FreezeAll;
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

        string stateName = state == ActorState.Jumping ? "Jump" :
            state == ActorState.Pushing ? "Push" :
            state == ActorState.Pulling ? "Pull" :
            state == ActorState.Linked ? "Walk" :
            state == ActorState.Walking || state == ActorState.Sprinting ? "Walk" :
            idleVariation ? "Idle2" : "Idle1";
        float blendTime = state == ActorState.Jumping ? 0.08f : 0.15f;
        string resolvedState = ResolveAnimationState(stateName);
        if (resolvedState == null)
        {
            Debug.LogWarning("[FormalPlayerActor] " + name + " missing animation state " + stateName);
            return;
        }

        animator.CrossFadeInFixedTime(resolvedState, blendTime, 0, 0f);
        Debug.Log("[FormalPlayerActor] " + name + " play animation " + resolvedState + " for " + state);
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
        body.position = position;
        if (moverRotationLocked)
        {
            body.rotation = moverRotation;
            transform.rotation = moverRotation;
        }
        Physics.SyncTransforms();
        body.velocity = Vector3.zero;
        body.angularVelocity = Vector3.zero;
    }
}
