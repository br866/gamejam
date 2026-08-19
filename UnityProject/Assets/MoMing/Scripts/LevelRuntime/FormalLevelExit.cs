using UnityEngine;

public class FormalLevelExit : MonoBehaviour
{
    [SerializeField] private string successorScene;
    [SerializeField] private FormalDoor requiredDoor;

    void Awake()
    {
        if (requiredDoor == null)
            requiredDoor = GetComponent<FormalDoor>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (!FormalLevelActors.IsPlayer(other) ||
            requiredDoor != null && !requiredDoor.IsOpen)
            return;

        FormalGameFlowController flow = FindObjectOfType<FormalGameFlowController>();
        if (flow != null)
            flow.LoadSuccessor(successorScene);
    }
}
