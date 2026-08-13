using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FormalGameFlowController : MonoBehaviour
{
    [SerializeField] private string initialLevelScene = "FormalLevel01";

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
        if (!SceneManager.GetSceneByName(initialLevelScene).isLoaded)
            yield return SceneManager.LoadSceneAsync(initialLevelScene, LoadSceneMode.Additive);

        currentLevelScene = initialLevelScene;
        SceneManager.SetActiveScene(SceneManager.GetSceneByName(currentLevelScene));
        PlacePlayersAtLoadedLevelSpawn();
    }

    IEnumerator LoadSuccessorRoutine(string sceneName)
    {
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
    }

    void PlacePlayersAtLoadedLevelSpawn()
    {
        FormalLevelController level = FormalLevelActors.FindLevelController(SceneManager.GetSceneByName(currentLevelScene));
        if (level != null)
            level.PlacePlayersAtSpawn();
    }
}
