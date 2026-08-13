using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
public class FormalPlayerActor : MonoBehaviour
{
    public enum ActorRole { Human, Dog }

    [SerializeField] private ActorRole role;
    [SerializeField] private float walkSpeed = 4f;
    [SerializeField] private float sprintSpeed = 7f;
    [SerializeField] private float turnSpeed = 12f;

    private Rigidbody body;

    public ActorRole Role => role;

    void Awake()
    {
        body = GetComponent<Rigidbody>();
        body.useGravity = false;
        body.constraints = RigidbodyConstraints.FreezeRotation;
        body.interpolation = RigidbodyInterpolation.Interpolate;
    }

    public void Move(Vector3 direction, bool sprint)
    {
        float speed = sprint && role == ActorRole.Dog ? sprintSpeed : walkSpeed;
        body.velocity = direction * speed;

        if (direction.sqrMagnitude > 0.01f)
            body.MoveRotation(Quaternion.Slerp(body.rotation, Quaternion.LookRotation(direction), turnSpeed * Time.fixedDeltaTime));
    }

    public void Stop()
    {
        body.velocity = Vector3.zero;
    }

    public void SetPosition(Vector3 position)
    {
        body.position = position;
        body.velocity = Vector3.zero;
        body.angularVelocity = Vector3.zero;
    }
}
