using UnityEngine;

public class FormalPrerequisiteActuator : MonoBehaviour, IFormalLevelTemporaryState
{
    [SerializeField] private FormalMechanismState[] prerequisites;
    [SerializeField] private MonoBehaviour[] actuators;
    [SerializeField] private bool permanentResult = true;

    private bool applied;

    void Awake()
    {
        FormalLevelController level = FormalLevelActors.FindLevelController(gameObject.scene);
        if (level != null)
            level.RegisterTemporaryState(this);
    }

    void Update()
    {
        if (!applied && ArePrerequisitesComplete())
            ApplyOpenState();
    }

    public bool ArePrerequisitesComplete()
    {
        if (prerequisites == null || prerequisites.Length == 0)
            return false;

        foreach (FormalMechanismState prerequisite in prerequisites)
        {
            if (prerequisite == null || !prerequisite.IsComplete)
                return false;
        }

        return true;
    }

    public void ResetTemporaryState()
    {
        if (permanentResult || !applied)
            return;

        applied = false;
        foreach (MonoBehaviour behaviour in actuators)
        {
            IFormalLevelActuator actuator = behaviour as IFormalLevelActuator;
            if (actuator != null)
                actuator.Close();
        }
    }

    void ApplyOpenState()
    {
        applied = true;
        foreach (MonoBehaviour behaviour in actuators)
        {
            IFormalLevelActuator actuator = behaviour as IFormalLevelActuator;
            if (actuator != null)
                actuator.Open();
        }
    }
}
