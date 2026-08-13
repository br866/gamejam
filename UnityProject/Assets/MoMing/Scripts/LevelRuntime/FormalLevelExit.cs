using UnityEngine;

public class FormalLevelExit : MonoBehaviour
{
    [SerializeField] private string successorScene;

    void OnTriggerEnter(Collider other)
    {
        if (!FormalLevelActors.IsPlayer(other))
            return;

        FormalGameFlowController flow = FindObjectOfType<FormalGameFlowController>();
        if (flow != null)
            flow.LoadSuccessor(successorScene);
    }
}
