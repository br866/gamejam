using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider))]
public class FormalOccupancyTrigger : MonoBehaviour
{
    [SerializeField] private FormalTriggerRequirement requirement = FormalTriggerRequirement.EitherPlayer;
    [SerializeField] private UnityEvent onFirstAcceptedEnter;
    [SerializeField] private UnityEvent onLastAcceptedExit;

    private readonly HashSet<Object> occupants = new HashSet<Object>();

    public int OccupantCount => occupants.Count;
    public bool IsOccupied => occupants.Count > 0;

    void OnTriggerEnter(Collider other)
    {
        Object occupant = FormalTriggerEligibility.ResolveOccupant(other, requirement);
        if (occupant == null || !occupants.Add(occupant))
            return;

        if (occupants.Count == 1)
            onFirstAcceptedEnter?.Invoke();
    }

    void OnTriggerExit(Collider other)
    {
        Object occupant = FormalTriggerEligibility.ResolveOccupant(other, requirement);
        if (occupant == null || !occupants.Remove(occupant))
            return;

        if (occupants.Count == 0)
            onLastAcceptedExit?.Invoke();
    }

    void LateUpdate()
    {
        occupants.RemoveWhere(occupant => occupant == null);
    }
}
