using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
public class FormalPlayerActor : MonoBehaviour
{
    public enum ActorRole { Human, Dog }
    public enum ActorState { Idle, Walking, Sprinting, Jumping, Linked }

    [SerializeField] private ActorRole role;
    [SerializeField] private float walkSpeed = 4f;
    [SerializeField] private float sprintSpeed = 7f;
    [SerializeField] private float turnSpeed = 12f;
    [SerializeField] private float jumpHeight = 1.5f;

    private Rigidbody body;
    private CapsuleCollider capsule;
    private Animator animator;
    private ActorState state;

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
            animator = GetComponentInChildren<Animator>();

        ApplyAnimationState();
    }

    public void Move(Vector3 direction, bool sprint)
    {
        float speed = sprint && role == ActorRole.Dog ? sprintSpeed : walkSpeed;
        body.velocity = new Vector3(direction.x * speed, body.velocity.y, direction.z * speed);
        SetState(!IsGrounded()
            ? ActorState.Jumping
            : direction.sqrMagnitude > 0.01f
                ? sprint && role == ActorRole.Dog ? ActorState.Sprinting : ActorState.Walking
                : ActorState.Idle);

        if (direction.sqrMagnitude > 0.01f)
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

    void SetState(ActorState nextState)
    {
        if (state == nextState)
            return;

        state = nextState;
        ApplyAnimationState();
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
        body.velocity = Vector3.zero;
        body.angularVelocity = Vector3.zero;
    }
}
