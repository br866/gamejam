using UnityEngine;

public class FormalCheckpoint : MonoBehaviour, IFormalLevelPermanentState
{
    [SerializeField] private FormalLevelController level;
    [SerializeField] private Transform humanRespawnAnchor;
    [SerializeField] private Transform dogRespawnAnchor;
    [SerializeField] private FormalMechanismState[] prerequisites;
    [SerializeField] private bool successorRegistrationPoint;

    public bool IsComplete { get; private set; }

    public void ActivateCheckpoint()
    {
        if (level == null)
            level = FormalLevelActors.FindLevelController(gameObject.scene);

        if (level == null || !PrerequisitesComplete())
            return;

        IsComplete = true;
        level.SetCheckpoint(humanRespawnAnchor, dogRespawnAnchor);
    }

    void OnTriggerEnter(Collider other)
    {
        if (IsComplete || !FormalLevelActors.IsPlayer(other) || !PrerequisitesComplete())
            return;

        ActivateCheckpoint();

        if (!successorRegistrationPoint)
            return;

        FormalGameFlowController flow = FindObjectOfType<FormalGameFlowController>();
        if (flow != null)
            flow.NotifySuccessorCheckpointActivated(gameObject.scene.name);
    }

    bool PrerequisitesComplete()
    {
        if (prerequisites == null)
            return true;

        foreach (FormalMechanismState prerequisite in prerequisites)
        {
            if (prerequisite == null || !prerequisite.IsComplete)
                return false;
        }

        return true;
    }
}
