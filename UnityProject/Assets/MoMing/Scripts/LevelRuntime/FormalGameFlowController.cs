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
    private bool successorArrivalConfirmed;
    private Coroutine level045PursuitRoutine;
    private bool gmLevel045Pending;

    public string CurrentLevelScene => currentLevelScene;
    public IReadOnlyList<FormalRouteEntry> RouteCatalog => routeCatalog;
    public bool IsRouteComplete => routeComplete;
    public bool SuccessorArrivalConfirmed => successorArrivalConfirmed;

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

        if (Input.GetKeyDown(KeyCode.Alpha2))
            StartGmLevel045();
        else if (Input.GetKeyDown(KeyCode.Keypad2))
            GoToNextLevel();
        else if (Input.GetKeyDown(KeyCode.Keypad8))
            GoToPreviousLevel();
        else if (Input.GetKeyDown(KeyCode.Keypad5))
            ResetCurrentLevel();
        else if (Input.GetKeyDown(KeyCode.Keypad6))
            CompleteCurrentLevelConditionsAndAdvance();
    }

    public void LoadSuccessor(string sceneName)
    {
        if (operationInProgress)
            return;

        StartCoroutine(LoadLevelRoutine(sceneName, null, false, true));
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

    public void StartGmLevel045()
    {
        if (currentLevelScene == "FormalLevel045")
            return;

        if (operationInProgress)
        {
            gmLevel045Pending = true;
            return;
        }

        StartCoroutine(LoadGmLevel045Routine());
    }

    IEnumerator LoadGmLevel045Routine()
    {
        operationInProgress = true;

        yield return EnsureSceneLoaded("FormalSharedArt_L04_L045");
        yield return EnsureSceneLoaded("FormalSharedArt_L045_L05");
        yield return EnsureSceneLoaded("FormalLevel04");
        yield return EnsureSceneLoaded("FormalLevel045");

        currentLevelScene = "FormalLevel045";
        pendingUnloadScene = "FormalLevel04";
        successorArrivalConfirmed = false;
        SceneManager.SetActiveScene(SceneManager.GetSceneByName("FormalLevel045"));
        OpenAllDoorsInScene("FormalLevel04");
        CloseLevel045To05Doors();
        yield return EnsureSceneLoaded("FormalLevel04");
        yield return WaitForFormalActors();
        yield return null;
        FormalCheckpoint checkpoint = FindCheckpoint("FormalLevel045", "L045_Checkpoint");
        if (checkpoint != null)
            checkpoint.ActivateCheckpoint();

        FormalLevelController level045 = FormalLevelActors.FindLevelController(SceneManager.GetSceneByName("FormalLevel045"));
        if (level045 != null)
            level045.ResetLevel();

        BeginLevel045PursuitSequence();
        operationInProgress = false;
    }

    static FormalCheckpoint FindCheckpoint(string sceneName, string checkpointName)
    {
        Scene scene = SceneManager.GetSceneByName(sceneName);
        if (!scene.isLoaded)
            return null;

        foreach (GameObject root in scene.GetRootGameObjects())
            foreach (FormalCheckpoint checkpoint in root.GetComponentsInChildren<FormalCheckpoint>(true))
                if (checkpoint.name == checkpointName)
                    return checkpoint;

        return null;
    }

    IEnumerator EnsureSceneLoaded(string sceneName)
    {
        Scene scene = SceneManager.GetSceneByName(sceneName);
        if (!scene.isLoaded)
        {
            string scenePath = "Assets/MoMing/FormalLevels/" + sceneName + ".unity";
            yield return SceneManager.LoadSceneAsync(scenePath, LoadSceneMode.Additive);
        }

        float timeout = 10f;
        while (!SceneManager.GetSceneByName(sceneName).isLoaded && timeout > 0f)
        {
            timeout -= Time.unscaledDeltaTime;
            yield return null;
        }
    }

    IEnumerator WaitForFormalActors()
    {
        float timeout = 5f;
        while (FormalPlayerActors.Instance == null && timeout > 0f)
        {
            timeout -= Time.unscaledDeltaTime;
            yield return null;
        }
    }

    void CloseLevel045To05Doors()
    {
        foreach (string sceneName in new[] { "FormalLevel045", "FormalSharedArt_L045_L05" })
        {
            Scene scene = SceneManager.GetSceneByName(sceneName);
            if (!scene.isLoaded)
                continue;

            foreach (GameObject root in scene.GetRootGameObjects())
                foreach (FormalDoor door in root.GetComponentsInChildren<FormalDoor>(true))
                    if (door.name.IndexOf("ToLevel05", StringComparison.OrdinalIgnoreCase) >= 0)
                        door.SetClosedImmediate();
        }
    }

    void OpenAllDoorsInScene(string sceneName)
    {
        Scene scene = SceneManager.GetSceneByName(sceneName);
        if (!scene.isLoaded)
            return;

        foreach (GameObject root in scene.GetRootGameObjects())
            foreach (FormalDoor door in root.GetComponentsInChildren<FormalDoor>(true))
                door.OpenPermanently();
    }

    public void ResetCurrentLevel()
    {
        if (string.IsNullOrEmpty(currentLevelScene))
            return;

        if (currentLevelScene == "FormalLevel045" && pendingUnloadScene == "FormalLevel04")
        {
            FormalLevelController retainedLevel = FormalLevelActors.FindLevelController(SceneManager.GetSceneByName(currentLevelScene));
            if (retainedLevel != null)
                retainedLevel.ResetLevel();

            Scene retainedScene = SceneManager.GetSceneByName("FormalLevel04");
            if (retainedScene.isLoaded)
            {
                foreach (GameObject root in retainedScene.GetRootGameObjects())
                    foreach (MonsterPatrol monster in root.GetComponentsInChildren<MonsterPatrol>(true))
                        monster.ResetPatrol();
            }

            BeginLevel045PursuitSequence();
            return;
        }

        if (!string.IsNullOrEmpty(pendingUnloadScene))
        {
            if (!operationInProgress)
                StartCoroutine(RestartCurrentLevelRoutine());
            return;
        }

        FormalLevelController level = FormalLevelActors.FindLevelController(SceneManager.GetSceneByName(currentLevelScene));
        if (level != null)
            level.ResetLevel();
    }

    public void OpenTransitionDoor(string fromScene, string toScene)
    {
        FormalDoor door = FindTransitionDoor(fromScene, toScene);
        if (door != null)
            door.OpenPermanently();
        else
            Debug.LogWarning($"No shared transition door found from {fromScene} to {toScene}.");
    }

    public void OpenTransitionDoorToSuccessor(string fromScene)
    {
        int index = FindRouteIndex(fromScene);
        if (index < 0 || index + 1 >= routeCatalog.Length)
            return;

        OpenTransitionDoor(fromScene, routeCatalog[index + 1].sceneName);
    }

    public void NotifyLevel045DoorOpened()
    {
        if (currentLevelScene != "FormalLevel045" || pendingUnloadScene != "FormalLevel04")
            return;

        BeginLevel045PursuitSequence();
    }

    public void CompleteCurrentLevelConditionsAndAdvance()
    {
        Scene current = SceneManager.GetSceneByName(currentLevelScene);
        if (!current.isLoaded)
            return;

        OpenAllDoorsInCurrentLevelScope();

        // Keypad 6 is an editor/test shortcut: mark the current level's
        // completion state directly instead of simulating player movement.
        foreach (FormalMechanismState state in FindInScene<FormalMechanismState>(current))
            state.Complete();

        foreach (FormalActuatorTrigger trigger in FindInScene<FormalActuatorTrigger>(current))
            trigger.CompleteImmediately();

        int currentIndex = FindRouteIndex(currentLevelScene);
        if (currentIndex >= 0 && currentIndex + 1 < routeCatalog.Length)
        {
            string successor = routeCatalog[currentIndex + 1].sceneName;
            OpenTransitionDoor(currentLevelScene, successor);
            LoadSuccessor(successor);
        }
    }

    public void OpenAllDoorsInCurrentLevelScope()
    {
        foreach (Scene scene in GetCurrentLevelDoorScenes())
        {
            if (!scene.isLoaded)
                continue;

            foreach (GameObject root in scene.GetRootGameObjects())
                foreach (FormalDoor door in root.GetComponentsInChildren<FormalDoor>(true))
                    door.OpenPermanently();
        }
    }

    static T[] FindInScene<T>(Scene scene) where T : Component
    {
        List<T> result = new List<T>();
        foreach (GameObject root in scene.GetRootGameObjects())
            result.AddRange(root.GetComponentsInChildren<T>(true));
        return result.ToArray();
    }

    public void NotifySuccessorCheckpointActivated(string sceneName)
    {
        if (routeComplete || sceneName != currentLevelScene || operationInProgress)
            return;

        // Reaching the level-exit checkpoint advances the route: the shared
        // transition door opens permanently and the successor level loads
        // additively. Prior levels stay loaded because the dog may still be
        // inside them; they are only unloaded by an explicit level reset.
        successorArrivalConfirmed = true;

        int index = FindRouteIndex(sceneName);
        if (index >= 0 && index + 1 < routeCatalog.Length)
        {
            string successor = routeCatalog[index + 1].sceneName;
            OpenTransitionDoor(sceneName, successor);
            LoadSuccessor(successor);
        }
    }

    IEnumerator RestartCurrentLevelRoutine()
    {
        operationInProgress = true;
        string predecessorScene = pendingUnloadScene;
        FormalDoor door = FindTransitionDoor(predecessorScene, currentLevelScene);
        if (door != null)
            door.SetClosedImmediate();

        Scene predecessor = SceneManager.GetSceneByName(predecessorScene);
        if (predecessor.isLoaded)
            yield return SceneManager.UnloadSceneAsync(predecessor);

        pendingUnloadScene = null;
        successorArrivalConfirmed = false;
        FormalLevelController level = FormalLevelActors.FindLevelController(SceneManager.GetSceneByName(currentLevelScene));
        if (level != null)
            level.ResetLevel();

        yield return UnloadUnusedSharedArt();
        operationInProgress = false;
    }

    IEnumerator LoadLevelRoutine(string sceneName, Action<bool> completed, bool discardPriorLevel, bool keepPriorLevels = false)
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
        if (sceneName != "FormalLevel045")
        {
            FormalPlayerControl control = FindObjectOfType<FormalPlayerControl>();
            if (control != null)
                control.ForceHumanOnly(false);
        }

        yield return LoadSharedArtForEntries(predecessorScene, priorPendingScene, sceneName);

        bool alreadyLoaded = SceneManager.GetSceneByName(sceneName).isLoaded;
        if (!alreadyLoaded)
            yield return SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);

        currentLevelScene = sceneName;
        pendingUnloadScene = discardPriorLevel || predecessorScene == sceneName
            ? null
            : predecessorScene;
        successorArrivalConfirmed = false;
        SceneManager.SetActiveScene(SceneManager.GetSceneByName(currentLevelScene));
        if (string.IsNullOrEmpty(predecessorScene))
            PlacePlayersAtLoadedLevelSpawn();
        if (!keepPriorLevels)
            yield return UnloadIrrelevantLevels(priorPendingScene);
        if (!discardPriorLevel && predecessorScene != sceneName)
            yield return ArrivalCleanup();
        yield return UnloadUnusedSharedArt();
        operationInProgress = false;
        completed?.Invoke(true);

        if (gmLevel045Pending)
        {
            gmLevel045Pending = false;
            StartCoroutine(LoadGmLevel045Routine());
        }
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

    IEnumerator ArrivalCleanup()
    {
        int index = FindRouteIndex(currentLevelScene);
        if (index < 0)
            yield break;

        string predecessor = index - 1 >= 0 ? routeCatalog[index - 1].sceneName : null;
        string grandPredecessor = index - 2 >= 0 ? routeCatalog[index - 2].sceneName : null;
        foreach (string levelScene in new[] { predecessor, grandPredecessor })
        {
            if (string.IsNullOrEmpty(levelScene))
                continue;

            if (currentLevelScene == "FormalLevel045" && levelScene == "FormalLevel04")
                continue;

            Scene retained = SceneManager.GetSceneByName(levelScene);
            if (!retained.IsValid() || !retained.isLoaded || levelScene == currentLevelScene)
                continue;

            foreach (GameObject root in retained.GetRootGameObjects())
                foreach (FormalDoor door in root.GetComponentsInChildren<FormalDoor>(true))
                    door.Close();
        }

        HashSet<string> keep = new HashSet<string> { currentLevelScene };
        if (!string.IsNullOrEmpty(predecessor))
            keep.Add(predecessor);

        foreach (FormalRouteEntry entry in routeCatalog)
        {
            if (entry == null || string.IsNullOrEmpty(entry.sceneName) || keep.Contains(entry.sceneName))
                continue;

            Scene stale = SceneManager.GetSceneByName(entry.sceneName);
            if (stale.IsValid() && stale.isLoaded)
                yield return SceneManager.UnloadSceneAsync(stale);
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

    FormalDoor FindTransitionDoor(string fromScene, string toScene)
    {
        FormalRouteEntry from = FindRouteEntryByScene(fromScene);
        FormalRouteEntry to = FindRouteEntryByScene(toScene);
        if (from == null || to == null || from.sharedArtScenes == null || to.sharedArtScenes == null)
            return null;

        HashSet<string> sharedScenes = new HashSet<string>(from.sharedArtScenes);
        sharedScenes.IntersectWith(to.sharedArtScenes);
        foreach (string sharedSceneName in sharedScenes)
        {
            Scene sharedScene = SceneManager.GetSceneByName(sharedSceneName);
            if (!sharedScene.isLoaded)
                continue;

            foreach (GameObject root in sharedScene.GetRootGameObjects())
                foreach (FormalDoor door in root.GetComponentsInChildren<FormalDoor>(true))
                    if (door.name.IndexOf("ToLevel", StringComparison.OrdinalIgnoreCase) >= 0)
                        return door;
        }

        return null;
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

    void BeginLevel045PursuitSequence()
    {
        if (currentLevelScene != "FormalLevel045" || pendingUnloadScene != "FormalLevel04")
            return;

        if (level045PursuitRoutine != null)
            StopCoroutine(level045PursuitRoutine);
        StartCoroutine(BindLevel045PlayerState());
        level045PursuitRoutine = StartCoroutine(StartLevel045PursuitAfterDelay());
    }

    IEnumerator BindLevel045PlayerState()
    {
        float timeout = 10f;
        while (timeout > 0f)
        {
            FormalPlayerActors actors = FormalPlayerActors.Instance;
            FormalPlayerControl control = FindObjectOfType<FormalPlayerControl>();
            if (actors != null && actors.Human != null && actors.Dog != null && control != null)
            {
                control.ForceHumanOnly(true);
                FormalDogOrbitFollower follower = actors.Dog.GetComponent<FormalDogOrbitFollower>();
                if (follower == null)
                    follower = actors.Dog.gameObject.AddComponent<FormalDogOrbitFollower>();
                follower.BeginOrbit(actors.Human, actors.Dog);
                yield break;
            }

            timeout -= Time.unscaledDeltaTime;
            yield return null;
        }
    }

    IEnumerator StartLevel045PursuitAfterDelay()
    {
        yield return new WaitForSeconds(10f);

        FormalPlayerActors actors = FormalPlayerActors.Instance;
        if (actors == null || actors.Human == null)
            yield break;

        Scene level04 = SceneManager.GetSceneByName("FormalLevel04");
        if (!level04.isLoaded)
            yield break;

        foreach (GameObject root in level04.GetRootGameObjects())
            foreach (MonsterPatrol monster in root.GetComponentsInChildren<MonsterPatrol>(true))
                monster.BeginForcedChase(actors.Human.transform);
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
