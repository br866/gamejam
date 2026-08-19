using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class FormalActuatorTrigger : MonoBehaviour, IFormalLevelTemporaryState
{
    [SerializeField] private FormalTriggerRequirement requirement = FormalTriggerRequirement.EitherPlayer;
    [SerializeField] private FormalMechanismState[] prerequisites;
    [SerializeField] private FormalMechanismState completionState;
    [SerializeField] private MonoBehaviour[] actuators;
    [SerializeField] private bool permanent = true;

    private readonly HashSet<Object> occupants = new HashSet<Object>();
    private bool complete;

    void Awake()
    {
        FormalLevelController level = FormalLevelActors.FindLevelController(gameObject.scene);
        if (level != null)
            level.RegisterTemporaryState(this);
    }

    void OnTriggerEnter(Collider other)
    {
        Object occupant = FormalTriggerEligibility.ResolveOccupant(other, requirement);
        if (occupant == null || !occupants.Add(occupant) || complete ||
            !PrerequisitesComplete() || !RequirementSatisfied())
            return;

        complete = true;
        if (completionState != null)
            completionState.Complete();
        foreach (MonoBehaviour behaviour in actuators)
        {
            IFormalLevelActuator actuator = behaviour as IFormalLevelActuator;
            if (actuator != null)
                actuator.Open();
        }
    }

    void OnTriggerExit(Collider other)
    {
        Object occupant = FormalTriggerEligibility.ResolveOccupant(other, requirement);
        if (occupant != null)
            occupants.Remove(occupant);
    }

    void LateUpdate()
    {
        occupants.RemoveWhere(occupant => occupant == null);
        TryComplete();
    }

    public void ResetTemporaryState()
    {
        occupants.Clear();
        if (permanent || !complete)
            return;

        complete = false;
        foreach (MonoBehaviour behaviour in actuators)
        {
            IFormalLevelActuator actuator = behaviour as IFormalLevelActuator;
            if (actuator != null)
                actuator.Close();
        }
    }

    bool RequirementSatisfied()
    {
        if (requirement != FormalTriggerRequirement.BothPlayers)
            return occupants.Count > 0;

        bool hasHuman = false;
        bool hasDog = false;
        foreach (Object occupant in occupants)
        {
            FormalPlayerActor player = occupant as FormalPlayerActor;
            if (player == null)
                continue;

            hasHuman |= player.Role == FormalPlayerActor.ActorRole.Human;
            hasDog |= player.Role == FormalPlayerActor.ActorRole.Dog;
        }

        return hasHuman && hasDog;
    }

    void TryComplete()
    {
        if (complete || !PrerequisitesComplete() || !RequirementSatisfied())
            return;

        complete = true;
        if (completionState != null)
            completionState.Complete();
        foreach (MonoBehaviour behaviour in actuators)
        {
            IFormalLevelActuator actuator = behaviour as IFormalLevelActuator;
            if (actuator != null)
                actuator.Open();
        }
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
