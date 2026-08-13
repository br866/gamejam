using UnityEngine;

public class FormalCheckpoint : MonoBehaviour, IFormalLevelPermanentState
{
    [SerializeField] private FormalLevelController level;
    [SerializeField] private string successorScene;

    public bool IsComplete { get; private set; }

    void OnTriggerEnter(Collider other)
    {
        if (IsComplete || !FormalLevelActors.IsPlayer(other))
            return;

        IsComplete = true;
        level.SetCheckpoint(other.transform);

        FormalGameFlowController flow = FindObjectOfType<FormalGameFlowController>();
        if (flow != null)
            flow.NotifySuccessorCheckpointActivated(gameObject.scene.name);
    }
}
