using UnityEngine;

public interface IFormalLevelTemporaryState
{
    void ResetTemporaryState();
}

public interface IFormalLevelPermanentState
{
    bool IsComplete { get; }
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
        FormalPlayerActor player = other.GetComponentInParent<FormalPlayerActor>();
        return player != null && player.Role == FormalPlayerActor.ActorRole.Human;
    }

    public static bool IsPlayer(Collider other)
    {
        return other.GetComponentInParent<FormalPlayerActor>() != null;
    }
}
