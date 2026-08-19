using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FormalGameFlowController : MonoBehaviour
{
    [Serializable]
    public class FormalRouteEntry
    {
        public string levelId;
        public string sceneName;
        public string[] sharedArtScenes;
    }

    [SerializeField] private string initialLevelScene = "FormalLevel01";
    [SerializeField] private FormalRouteEntry[] routeCatalog;

    // Kept as serialized migration inputs for existing FormalPersistent scenes.
    [SerializeField] private string level01Level02SharedArtScene = "FormalSharedArt_L01_L02";
    [SerializeField] private string level02Level03SharedArtScene = "FormalSharedArt_L02_L03";
    [SerializeField] private string level03Level04SharedArtScene = "FormalSharedArt_L03_L04";
    [SerializeField] private string level04Level045SharedArtScene = "FormalSharedArt_L04_L045";
    [SerializeField] private string level045Level05SharedArtScene = "FormalSharedArt_L045_L05";

    private string currentLevelScene;
    private string pendingUnloadScene;
    private bool operationInProgress;
    private bool routeComplete;

    public string CurrentLevelScene => currentLevelScene;
    public IReadOnlyList<FormalRouteEntry> RouteCatalog => routeCatalog;
    public bool IsRouteComplete => routeComplete;

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
        EnsureRouteCatalog();
    }

    void Start()
    {
        if (!string.IsNullOrEmpty(initialLevelScene))
            LoadLevelAsync(initialLevelScene, null);
    }

    void Update()
    {
        if (routeComplete)
            return;

        if (Input.GetKeyDown(KeyCode.Keypad2))
            GoToNextLevel();
        else if (Input.GetKeyDown(KeyCode.Keypad8))
            GoToPreviousLevel();
        else if (Input.GetKeyDown(KeyCode.Keypad5))
            ResetCurrentLevel();
        else if (Input.GetKeyDown(KeyCode.Keypad6))
            OpenDoorsInCurrentLevelScope();
    }

    public void LoadSuccessor(string sceneName)
    {
        if (operationInProgress)
            return;

        StartCoroutine(LoadLevelRoutine(sceneName, null, false));
    }

    public void CompleteRoute()
    {
        if (currentLevelScene != "FormalLevel05")
        {
            Debug.LogWarning("Formal route completion can only be triggered from FormalLevel05.");
            return;
        }

        routeComplete = true;
        SetFormalControlsEnabled(false);
    }

    public void RestartRoute()
    {
        if (operationInProgress)
            return;

        routeComplete = false;
        LoadLevelAsync(initialLevelScene, _ => SetFormalControlsEnabled(true));
    }

    public void LoadLevel(string sceneName)
    {
        if (operationInProgress)
            return;

        StartCoroutine(LoadLevelRoutine(sceneName, null, true));
    }

    public void LoadLevelAsync(string sceneName, Action<bool> completed)
    {
        if (operationInProgress)
        {
            completed?.Invoke(false);
            return;
        }

        StartCoroutine(LoadLevelRoutine(sceneName, completed, true));
    }

    public void UnloadLevel(string sceneName)
    {
        if (operationInProgress || string.IsNullOrEmpty(sceneName) || sceneName == currentLevelScene)
            return;

        StartCoroutine(UnloadLevelRoutine(sceneName, null));
    }

    public void UnloadLevelAsync(string sceneName, Action<bool> completed)
    {
        if (operationInProgress || string.IsNullOrEmpty(sceneName) || sceneName == currentLevelScene)
        {
            completed?.Invoke(false);
            return;
        }

        StartCoroutine(UnloadLevelRoutine(sceneName, completed));
    }

    public void GoToNextLevel()
    {
        int index = FindRouteIndex(currentLevelScene);
        if (index >= 0 && index + 1 < routeCatalog.Length)
            LoadSuccessor(routeCatalog[index + 1].sceneName);
    }

    public void GoToPreviousLevel()
    {
        int index = FindRouteIndex(currentLevelScene);
        if (index > 0)
            LoadLevel(routeCatalog[index - 1].sceneName);
    }

    public void JumpToLevel(string levelId)
    {
        FormalRouteEntry entry = FindRouteEntry(levelId);
        if (entry != null)
            LoadLevel(entry.sceneName);
    }

    public void ResetCurrentLevel()
    {
        if (string.IsNullOrEmpty(currentLevelScene))
            return;

        FormalLevelController level = FormalLevelActors.FindLevelController(SceneManager.GetSceneByName(currentLevelScene));
        if (level != null)
            level.ResetLevel();
    }

    public void OpenDoorsInCurrentLevelScope()
    {
        foreach (Scene scene in GetCurrentLevelDoorScenes())
        {
            if (!scene.isLoaded)
                continue;

            foreach (GameObject root in scene.GetRootGameObjects())
            {
                foreach (FormalDoor door in root.GetComponentsInChildren<FormalDoor>(true))
                    door.Open();
            }
        }
    }

    public void NotifySuccessorCheckpointActivated(string sceneName)
    {
        if (sceneName != currentLevelScene || string.IsNullOrEmpty(pendingUnloadScene))
            return;

        string priorScene = pendingUnloadScene;
        pendingUnloadScene = null;
        UnloadLevelAsync(priorScene, null);
    }

    IEnumerator LoadLevelRoutine(string sceneName, Action<bool> completed, bool discardPriorLevel)
    {
        FormalRouteEntry target = FindRouteEntryByScene(sceneName);
        if (target == null)
        {
            completed?.Invoke(false);
            yield break;
        }

        operationInProgress = true;
        string predecessorScene = currentLevelScene;
        string priorPendingScene = pendingUnloadScene;
        yield return LoadSharedArtForEntries(predecessorScene, priorPendingScene, sceneName);

        bool alreadyLoaded = SceneManager.GetSceneByName(sceneName).isLoaded;
        if (!alreadyLoaded)
            yield return SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);

        currentLevelScene = sceneName;
        pendingUnloadScene = discardPriorLevel || predecessorScene == sceneName
            ? null
            : predecessorScene;
        SceneManager.SetActiveScene(SceneManager.GetSceneByName(currentLevelScene));
        PlacePlayersAtLoadedLevelSpawn();
        yield return UnloadIrrelevantLevels(priorPendingScene);
        yield return UnloadUnusedSharedArt();
        operationInProgress = false;
        completed?.Invoke(true);
    }

    IEnumerator UnloadLevelRoutine(string sceneName, Action<bool> completed)
    {
        operationInProgress = true;
        bool loaded = SceneManager.GetSceneByName(sceneName).isLoaded;
        if (loaded)
            yield return SceneManager.UnloadSceneAsync(sceneName);

        yield return UnloadUnusedSharedArt();
        operationInProgress = false;
        completed?.Invoke(loaded);
    }

    IEnumerator LoadSharedArtForEntries(string activeScene, string transitionalScene, string targetScene)
    {
        HashSet<string> required = GetRequiredSharedScenes(activeScene, transitionalScene, targetScene);
        foreach (string sharedScene in required)
        {
            if (!string.IsNullOrEmpty(sharedScene) && !SceneManager.GetSceneByName(sharedScene).isLoaded)
                yield return SceneManager.LoadSceneAsync(sharedScene, LoadSceneMode.Additive);
        }
    }

    IEnumerator UnloadUnusedSharedArt()
    {
        HashSet<string> required = GetRequiredSharedScenes(currentLevelScene, pendingUnloadScene, null);
        HashSet<string> known = new HashSet<string>();
        foreach (FormalRouteEntry entry in routeCatalog)
        {
            if (entry.sharedArtScenes == null)
                continue;
            foreach (string sharedScene in entry.sharedArtScenes)
                known.Add(sharedScene);
        }

        foreach (string sharedScene in known)
        {
            if (!required.Contains(sharedScene) && SceneManager.GetSceneByName(sharedScene).isLoaded)
                yield return SceneManager.UnloadSceneAsync(sharedScene);
        }
    }

    IEnumerator UnloadIrrelevantLevels(string priorPendingScene)
    {
        foreach (FormalRouteEntry entry in routeCatalog)
        {
            if (entry == null || string.IsNullOrEmpty(entry.sceneName) ||
                entry.sceneName == currentLevelScene || entry.sceneName == pendingUnloadScene)
                continue;

            Scene scene = SceneManager.GetSceneByName(entry.sceneName);
            if (scene.isLoaded)
                yield return SceneManager.UnloadSceneAsync(scene);
        }

        // A new transition replaces any older checkpoint fallback scene.
        if (!string.IsNullOrEmpty(priorPendingScene) &&
            priorPendingScene != currentLevelScene && priorPendingScene != pendingUnloadScene)
        {
            Scene priorPending = SceneManager.GetSceneByName(priorPendingScene);
            if (priorPending.isLoaded)
                yield return SceneManager.UnloadSceneAsync(priorPending);
        }
    }

    HashSet<string> GetRequiredSharedScenes(string firstScene, string secondScene, string thirdScene)
    {
        HashSet<string> result = new HashSet<string>();
        AddSharedScenes(result, FindRouteEntryByScene(firstScene));
        AddSharedScenes(result, FindRouteEntryByScene(secondScene));
        AddSharedScenes(result, FindRouteEntryByScene(thirdScene));
        return result;
    }

    IEnumerable<Scene> GetCurrentLevelDoorScenes()
    {
        Scene currentScene = SceneManager.GetSceneByName(currentLevelScene);
        if (currentScene.IsValid())
            yield return currentScene;

        int currentIndex = FindRouteIndex(currentLevelScene);
        if (currentIndex < 0 || currentIndex + 1 >= routeCatalog.Length)
            yield break;

        FormalRouteEntry current = routeCatalog[currentIndex];
        FormalRouteEntry next = routeCatalog[currentIndex + 1];
        if (current.sharedArtScenes == null || next.sharedArtScenes == null)
            yield break;

        HashSet<string> nextSharedScenes = new HashSet<string>(next.sharedArtScenes);
        foreach (string sharedSceneName in current.sharedArtScenes)
        {
            if (!nextSharedScenes.Contains(sharedSceneName))
                continue;

            Scene sharedScene = SceneManager.GetSceneByName(sharedSceneName);
            if (sharedScene.IsValid())
                yield return sharedScene;
        }
    }

    static void AddSharedScenes(HashSet<string> result, FormalRouteEntry entry)
    {
        if (entry == null || entry.sharedArtScenes == null)
            return;
        foreach (string sceneName in entry.sharedArtScenes)
            if (!string.IsNullOrEmpty(sceneName))
                result.Add(sceneName);
    }

    FormalRouteEntry FindRouteEntry(string levelId)
    {
        foreach (FormalRouteEntry entry in routeCatalog)
            if (entry != null && entry.levelId == levelId)
                return entry;
        return null;
    }

    FormalRouteEntry FindRouteEntryByScene(string sceneName)
    {
        foreach (FormalRouteEntry entry in routeCatalog)
            if (entry != null && entry.sceneName == sceneName)
                return entry;
        return null;
    }

    int FindRouteIndex(string sceneName)
    {
        for (int i = 0; i < routeCatalog.Length; i++)
            if (routeCatalog[i] != null && routeCatalog[i].sceneName == sceneName)
                return i;
        return -1;
    }

    void EnsureRouteCatalog()
    {
        if (routeCatalog != null && routeCatalog.Length > 0)
            return;

        routeCatalog = new[]
        {
            Entry("Level01", "FormalLevel01", level01Level02SharedArtScene),
            Entry("Level02", "FormalLevel02", level01Level02SharedArtScene, level02Level03SharedArtScene),
            Entry("Level03", "FormalLevel03", level02Level03SharedArtScene, level03Level04SharedArtScene),
            Entry("Level04", "FormalLevel04", level03Level04SharedArtScene, level04Level045SharedArtScene),
            Entry("Level04.5", "FormalLevel045", level04Level045SharedArtScene, level045Level05SharedArtScene),
            Entry("Level05", "FormalLevel05", level045Level05SharedArtScene)
        };
    }

    static FormalRouteEntry Entry(string id, string scene, params string[] sharedScenes)
    {
        return new FormalRouteEntry { levelId = id, sceneName = scene, sharedArtScenes = sharedScenes };
    }

    void PlacePlayersAtLoadedLevelSpawn()
    {
        FormalLevelController level = FormalLevelActors.FindLevelController(SceneManager.GetSceneByName(currentLevelScene));
        if (level != null)
            level.PlacePlayersAtSpawn();
    }

    void SetFormalControlsEnabled(bool enabled)
    {
        foreach (FormalPlayerControl control in FindObjectsOfType<FormalPlayerControl>())
            control.enabled = enabled;
    }

    void OnGUI()
    {
        if (!routeComplete)
            return;

        const float width = 360f;
        const float height = 180f;
        Rect panel = new Rect((Screen.width - width) * 0.5f, (Screen.height - height) * 0.5f, width, height);
        GUI.Box(panel, "Route Complete");
        GUI.Label(new Rect(panel.x + 24f, panel.y + 42f, width - 48f, 24f), "You reached the end of the formal route.");

        if (GUI.Button(new Rect(panel.x + 90f, panel.y + 105f, 180f, 36f), "Restart Formal Route"))
            RestartRoute();
    }
}
