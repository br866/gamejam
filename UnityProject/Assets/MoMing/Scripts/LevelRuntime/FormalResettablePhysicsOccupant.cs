using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class FormalResettablePhysicsOccupant : MonoBehaviour, IFormalLevelTemporaryState
{
    private Rigidbody body;
    private Vector3 initialPosition;
    private Quaternion initialRotation;

    void Awake()
    {
        body = GetComponent<Rigidbody>();
        initialPosition = transform.position;
        initialRotation = transform.rotation;

        FormalLevelController level = FormalLevelActors.FindLevelController(gameObject.scene);
        if (level != null)
            level.RegisterTemporaryState(this);
    }

    public void ResetTemporaryState()
    {
        body.velocity = Vector3.zero;
        body.angularVelocity = Vector3.zero;
        body.position = initialPosition;
        body.rotation = initialRotation;
    }
}
