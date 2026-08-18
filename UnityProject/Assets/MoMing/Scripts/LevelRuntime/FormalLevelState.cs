using UnityEngine;

public interface IFormalLevelTemporaryState
{
    void ResetTemporaryState();
}

public interface IFormalLevelPermanentState
{
    bool IsComplete { get; }
}

public interface IFormalLevelActuator
{
    void Open();
    void Close();
}

public static class FormalLevelActors
{
    public static FormalLevelController FindLevelController(UnityEngine.SceneManagement.Scene scene)
    {
        GameObject[] roots = scene.GetRootGameObjects();
        foreach (GameObject root in roots)
        {
            FormalLevelController controller = root.GetComponentInChildren<FormalLevelController>(true);
            if (controller != null)
                return controller;
        }

        return null;
    }

    public static bool IsHuman(Collider other)
    {
        FormalPlayerActor player = ResolvePlayer(other);
        return player != null && player.Role == FormalPlayerActor.ActorRole.Human;
    }

    public static bool IsDog(Collider other)
    {
        FormalPlayerActor player = ResolvePlayer(other);
        return player != null && player.Role == FormalPlayerActor.ActorRole.Dog;
    }

    public static bool IsPlayer(Collider other)
    {
        return ResolvePlayer(other) != null;
    }

    public static FormalPlayerActor ResolvePlayer(Collider other)
    {
        return other != null ? other.GetComponentInParent<FormalPlayerActor>() : null;
    }
}
