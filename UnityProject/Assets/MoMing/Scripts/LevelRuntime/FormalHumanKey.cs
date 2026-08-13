using UnityEngine;

public class FormalHumanKey : MonoBehaviour, IFormalLevelTemporaryState
{
    [SerializeField] private FormalDoor exitDoor;

    private Vector3 initialPosition;
    private Quaternion initialRotation;
    private bool collected;

    void Awake()
    {
        initialPosition = transform.position;
        initialRotation = transform.rotation;
        FormalLevelController level = FormalLevelActors.FindLevelController(gameObject.scene);
        if (level != null)
            level.RegisterTemporaryState(this);
    }

    void OnTriggerEnter(Collider other)
    {
        if (collected || !FormalLevelActors.IsHuman(other))
            return;

        collected = true;
        if (exitDoor != null)
            exitDoor.OpenPermanently();
        gameObject.SetActive(false);
    }

    public void ResetTemporaryState()
    {
        collected = false;
        transform.SetPositionAndRotation(initialPosition, initialRotation);
        gameObject.SetActive(true);
    }
}
