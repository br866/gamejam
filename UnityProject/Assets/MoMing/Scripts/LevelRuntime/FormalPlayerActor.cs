using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
public class FormalPlayerActor : MonoBehaviour
{
    public enum ActorRole { Human, Dog }

    [SerializeField] private ActorRole role;

    private Rigidbody body;

    public ActorRole Role => role;

    void Awake()
    {
        body = GetComponent<Rigidbody>();
        body.useGravity = false;
        body.constraints = RigidbodyConstraints.FreezeRotation;
    }

    public void SetPosition(Vector3 position)
    {
        transform.position = position;
        body.velocity = Vector3.zero;
        body.angularVelocity = Vector3.zero;
    }
}
