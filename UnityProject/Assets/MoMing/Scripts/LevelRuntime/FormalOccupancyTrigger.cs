using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider))]
public class FormalOccupancyTrigger : MonoBehaviour
{
    [SerializeField] private FormalTriggerRequirement requirement = FormalTriggerRequirement.EitherPlayer;
    [SerializeField] private UnityEvent onFirstAcceptedEnter;
    [SerializeField] private UnityEvent onLastAcceptedExit;
    [SerializeField] private UnityEvent onRequirementSatisfied;
    [SerializeField] private UnityEvent onRequirementUnsatisfied;

    private readonly HashSet<Object> occupants = new HashSet<Object>();
    private bool requirementSatisfied;

    public int OccupantCount => occupants.Count;
    public bool IsOccupied => occupants.Count > 0;
    public bool IsRequirementSatisfied => requirement == FormalTriggerRequirement.BothPlayers
        ? HasBothPlayers()
        : IsOccupied;

    void OnTriggerEnter(Collider other)
    {
        Object occupant = FormalTriggerEligibility.ResolveOccupant(other, requirement);
        if (occupant == null || !occupants.Add(occupant))
            return;

        if (occupants.Count == 1)
            onFirstAcceptedEnter?.Invoke();

        UpdateRequirementState();
    }

    void OnTriggerExit(Collider other)
    {
        Object occupant = FormalTriggerEligibility.ResolveOccupant(other, requirement);
        if (occupant == null || !occupants.Remove(occupant))
            return;

        UpdateRequirementState();

        if (occupants.Count == 0)
            onLastAcceptedExit?.Invoke();
    }

    void LateUpdate()
    {
        if (occupants.RemoveWhere(occupant => occupant == null) > 0)
            UpdateRequirementState();
    }

    bool HasBothPlayers()
    {
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

    void UpdateRequirementState()
    {
        bool isSatisfied = IsRequirementSatisfied;
        if (isSatisfied == requirementSatisfied)
            return;

        requirementSatisfied = isSatisfied;
        if (requirementSatisfied)
            onRequirementSatisfied?.Invoke();
        else
            onRequirementUnsatisfied?.Invoke();
    }
}
