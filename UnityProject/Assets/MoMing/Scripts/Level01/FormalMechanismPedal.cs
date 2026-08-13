using UnityEngine;

public class FormalMechanismPedal : MonoBehaviour, IFormalLevelPermanentState
{
    [SerializeField] private FormalDoor linkedDoor;

    public bool IsComplete { get; private set; }

    void OnTriggerEnter(Collider other)
    {
        if (IsComplete || !FormalLevelActors.IsHuman(other))
            return;

        IsComplete = true;
        if (linkedDoor != null)
            linkedDoor.OpenPermanently();
    }
}
