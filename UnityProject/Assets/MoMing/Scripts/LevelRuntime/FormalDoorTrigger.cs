using UnityEngine;

[RequireComponent(typeof(Collider))]
public class FormalDoorTrigger : MonoBehaviour
{
    [SerializeField] private FormalTriggerRequirement requirement = FormalTriggerRequirement.EitherPlayer;
    [SerializeField] private FormalDoor[] doors;
    [SerializeField] private bool closeWhenEmpty;

    private readonly System.Collections.Generic.HashSet<Object> occupants =
        new System.Collections.Generic.HashSet<Object>();

    void OnTriggerEnter(Collider other)
    {
        Object occupant = FormalTriggerEligibility.ResolveOccupant(other, requirement);
        if (occupant == null || !occupants.Add(occupant))
            return;

        foreach (FormalDoor door in doors)
            if (door != null)
                door.Open();
    }

    void OnTriggerExit(Collider other)
    {
        Object occupant = FormalTriggerEligibility.ResolveOccupant(other, requirement);
        if (occupant == null || !occupants.Remove(occupant) || !closeWhenEmpty || occupants.Count > 0)
            return;

        foreach (FormalDoor door in doors)
            if (door != null)
                door.Close();
    }

    void LateUpdate()
    {
        occupants.RemoveWhere(occupant => occupant == null);
    }
}
