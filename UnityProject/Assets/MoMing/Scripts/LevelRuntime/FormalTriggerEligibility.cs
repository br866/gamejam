using UnityEngine;

public enum FormalTriggerRequirement
{
    EitherPlayer,
    HumanOnly,
    DogOnly,
    BothPlayers,
    ResettablePhysicsOccupant
}

public static class FormalTriggerEligibility
{
    public static bool Accepts(Collider other, FormalTriggerRequirement requirement)
    {
        if (other == null)
            return false;

        if (requirement == FormalTriggerRequirement.HumanOnly)
            return FormalLevelActors.IsHuman(other);

        if (requirement == FormalTriggerRequirement.DogOnly)
            return FormalLevelActors.IsDog(other);

        if (requirement == FormalTriggerRequirement.EitherPlayer || requirement == FormalTriggerRequirement.BothPlayers)
            return FormalLevelActors.IsPlayer(other);

        return other.GetComponentInParent<FormalResettablePhysicsOccupant>() != null;
    }

    public static Object ResolveOccupant(Collider other, FormalTriggerRequirement requirement)
    {
        if (!Accepts(other, requirement))
            return null;

        FormalPlayerActor player = FormalLevelActors.ResolvePlayer(other);
        if (player != null)
            return player;

        return other.GetComponentInParent<FormalResettablePhysicsOccupant>();
    }
}
