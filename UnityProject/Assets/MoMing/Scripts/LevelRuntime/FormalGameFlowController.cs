using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FormalGameFlowController : MonoBehaviour
{
    [SerializeField] private string initialLevelScene = "FormalLevel01";
    [SerializeField] private string level01Level02SharedArtScene = "FormalSharedArt_L01_L02";
    [SerializeField] private string level02Level03SharedArtScene = "FormalSharedArt_L02_L03";
    [SerializeField] private string level03Level04SharedArtScene = "FormalSharedArt_L03_L04";
    [SerializeField] private string level04Level045SharedArtScene = "FormalSharedArt_L04_L045";
    [SerializeField] private string level045Level05SharedArtScene = "FormalSharedArt_L045_L05";

    private string currentLevelScene;
    private string pendingUnloadScene;

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        if (!string.IsNullOrEmpty(initialLevelScene))
            StartCoroutine(LoadInitialLevel());
    }

    public void LoadSuccessor(string sceneName)
    {
        if (!string.IsNullOrEmpty(sceneName) && SceneManager.GetSceneByName(sceneName).isLoaded == false)
            StartCoroutine(LoadSuccessorRoutine(sceneName));
    }

    public void NotifySuccessorCheckpointActivated(string sceneName)
    {
        if (sceneName != currentLevelScene || string.IsNullOrEmpty(pendingUnloadScene))
            return;

        StartCoroutine(UnloadPriorLevel());
    }

    IEnumerator LoadInitialLevel()
    {
        yield return LoadSharedArtForLevel(initialLevelScene);

        if (!SceneManager.GetSceneByName(initialLevelScene).isLoaded)
            yield return SceneManager.LoadSceneAsync(initialLevelScene, LoadSceneMode.Additive);

        currentLevelScene = initialLevelScene;
        SceneManager.SetActiveScene(SceneManager.GetSceneByName(currentLevelScene));
        PlacePlayersAtLoadedLevelSpawn();
    }

    IEnumerator LoadSuccessorRoutine(string sceneName)
    {
        yield return LoadSharedArtForLevel(sceneName);

        yield return SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        pendingUnloadScene = currentLevelScene;
        currentLevelScene = sceneName;
        SceneManager.SetActiveScene(SceneManager.GetSceneByName(currentLevelScene));
        PlacePlayersAtLoadedLevelSpawn();
    }

    IEnumerator UnloadPriorLevel()
    {
        string sceneToUnload = pendingUnloadScene;
        pendingUnloadScene = null;
        if (!string.IsNullOrEmpty(sceneToUnload) && SceneManager.GetSceneByName(sceneToUnload).isLoaded)
            yield return SceneManager.UnloadSceneAsync(sceneToUnload);

        yield return UnloadUnusedSharedArt();
    }

    IEnumerator LoadSharedArtForLevel(string sceneName)
    {
        if (UsesLevel01Level02SharedArt(sceneName) &&
            !string.IsNullOrEmpty(level01Level02SharedArtScene) &&
            !SceneManager.GetSceneByName(level01Level02SharedArtScene).isLoaded)
            yield return SceneManager.LoadSceneAsync(level01Level02SharedArtScene, LoadSceneMode.Additive);

        if (UsesLevel02Level03SharedArt(sceneName) &&
            !string.IsNullOrEmpty(level02Level03SharedArtScene) &&
            !SceneManager.GetSceneByName(level02Level03SharedArtScene).isLoaded)
            yield return SceneManager.LoadSceneAsync(level02Level03SharedArtScene, LoadSceneMode.Additive);

        if (UsesLevel03Level04SharedArt(sceneName) &&
            !string.IsNullOrEmpty(level03Level04SharedArtScene) &&
            !SceneManager.GetSceneByName(level03Level04SharedArtScene).isLoaded)
            yield return SceneManager.LoadSceneAsync(level03Level04SharedArtScene, LoadSceneMode.Additive);

        if (UsesLevel04Level045SharedArt(sceneName) &&
            !string.IsNullOrEmpty(level04Level045SharedArtScene) &&
            !SceneManager.GetSceneByName(level04Level045SharedArtScene).isLoaded)
            yield return SceneManager.LoadSceneAsync(level04Level045SharedArtScene, LoadSceneMode.Additive);

        if (UsesLevel045Level05SharedArt(sceneName) &&
            !string.IsNullOrEmpty(level045Level05SharedArtScene) &&
            !SceneManager.GetSceneByName(level045Level05SharedArtScene).isLoaded)
            yield return SceneManager.LoadSceneAsync(level045Level05SharedArtScene, LoadSceneMode.Additive);
    }

    IEnumerator UnloadUnusedSharedArt()
    {
        if (!UsesLevel01Level02SharedArt(currentLevelScene) &&
            !UsesLevel01Level02SharedArt(pendingUnloadScene) &&
            !string.IsNullOrEmpty(level01Level02SharedArtScene) &&
            SceneManager.GetSceneByName(level01Level02SharedArtScene).isLoaded)
            yield return SceneManager.UnloadSceneAsync(level01Level02SharedArtScene);

        if (!UsesLevel02Level03SharedArt(currentLevelScene) &&
            !UsesLevel02Level03SharedArt(pendingUnloadScene) &&
            !string.IsNullOrEmpty(level02Level03SharedArtScene) &&
            SceneManager.GetSceneByName(level02Level03SharedArtScene).isLoaded)
            yield return SceneManager.UnloadSceneAsync(level02Level03SharedArtScene);

        if (!UsesLevel03Level04SharedArt(currentLevelScene) &&
            !UsesLevel03Level04SharedArt(pendingUnloadScene) &&
            !string.IsNullOrEmpty(level03Level04SharedArtScene) &&
            SceneManager.GetSceneByName(level03Level04SharedArtScene).isLoaded)
            yield return SceneManager.UnloadSceneAsync(level03Level04SharedArtScene);

        if (!UsesLevel04Level045SharedArt(currentLevelScene) &&
            !UsesLevel04Level045SharedArt(pendingUnloadScene) &&
            !string.IsNullOrEmpty(level04Level045SharedArtScene) &&
            SceneManager.GetSceneByName(level04Level045SharedArtScene).isLoaded)
            yield return SceneManager.UnloadSceneAsync(level04Level045SharedArtScene);

        if (!UsesLevel045Level05SharedArt(currentLevelScene) &&
            !UsesLevel045Level05SharedArt(pendingUnloadScene) &&
            !string.IsNullOrEmpty(level045Level05SharedArtScene) &&
            SceneManager.GetSceneByName(level045Level05SharedArtScene).isLoaded)
            yield return SceneManager.UnloadSceneAsync(level045Level05SharedArtScene);
    }

    static bool UsesLevel01Level02SharedArt(string sceneName)
    {
        return sceneName == "FormalLevel01" || sceneName == "FormalLevel02";
    }

    static bool UsesLevel02Level03SharedArt(string sceneName)
    {
        return sceneName == "FormalLevel02" || sceneName == "FormalLevel03";
    }

    static bool UsesLevel03Level04SharedArt(string sceneName)
    {
        return sceneName == "FormalLevel03" || sceneName == "FormalLevel04";
    }

    static bool UsesLevel04Level045SharedArt(string sceneName)
    {
        return sceneName == "FormalLevel04" || sceneName == "FormalLevel045";
    }

    static bool UsesLevel045Level05SharedArt(string sceneName)
    {
        return sceneName == "FormalLevel045" || sceneName == "FormalLevel05";
    }

    void PlacePlayersAtLoadedLevelSpawn()
    {
        FormalLevelController level = FormalLevelActors.FindLevelController(SceneManager.GetSceneByName(currentLevelScene));
        if (level != null)
            level.PlacePlayersAtSpawn();
    }
}
