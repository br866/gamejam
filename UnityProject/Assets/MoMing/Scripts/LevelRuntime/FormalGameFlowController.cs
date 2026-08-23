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
        public string arrivalTransitionDoorName;
    }

    [SerializeField] private string initialLevelScene = "FormalLevel01";
    [SerializeField] private FormalRouteEntry[] routeCatalog;

    // Kept as serialized migration inputs for existing FormalPersistent scenes.
    [SerializeField] private string level01Level02SharedArtScene = "FormalSharedArt_L01_L02";
    [SerializeField] private string level02Level03SharedArtScene = "FormalSharedArt_L02_L03";
    [SerializeField] private string level03Level04SharedArtScene = "FormalSharedArt_L03_L04";
    [SerializeField] private string level04Level045SharedArtScene = "FormalSharedArt_L04_L045";
    [SerializeField] private string level045Level05SharedArtScene = "FormalSharedArt_L045_L05";

    private const string RetainedPredecessorLevelId = "Level04.5";

    private string currentLevelScene;
    private string pendingUnloadScene;
    private bool operationInProgress;
    private bool routeComplete;
    private bool successorArrivalConfirmed;
    private Coroutine level045PursuitRoutine;
    private string pendingAdvanceFromScene;

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

        if (Input.GetKeyDown(KeyCode.Keypad2))
            GoToNextLevel();
        else if (Input.GetKeyDown(KeyCode.Keypad8))
            GoToPreviousLevel();
        else if (Input.GetKeyDown(KeyCode.Keypad5))
            ResetCurrentLevel();
        else if (Input.GetKeyDown(KeyCode.Keypad6))
            CompleteCurrentLevelConditionsAndAdvance();
    }

    public void RequestRouteAdvance()
    {
        string successor = GetRouteSuccessor(currentLevelScene);
        if (string.IsNullOrEmpty(successor))
            return;

        if (operationInProgress)
        {
            if (!string.IsNullOrEmpty(pendingAdvanceFromScene))
                Debug.Log($"[FormalGameFlowController] Replacing deferred advance from {pendingAdvanceFromScene} with request from {currentLevelScene}.");
            pendingAdvanceFromScene = currentLevelScene;
            return;
        }

        StartCoroutine(AdvanceRoutine(successor));
    }

    public void NotifyCheckpointActivated(string sceneName)
    {
        if (routeComplete || sceneName != currentLevelScene)
            return;

        if (!HasArrivalSequence(FindRouteIndex(currentLevelScene)))
            return;

        BeginRetainedPredecessorPursuitSequence();
    }

    public void NotifySuccessorCheckpointActivated(string sceneName)
    {
        if (routeComplete || sceneName != currentLevelScene)
            return;

        successorArrivalConfirmed = true;
        RequestRouteAdvance();
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
        if (FindRouteSuccessorIndex(currentLevelScene) >= 0)
            RequestRouteAdvance();
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

        if (!string.IsNullOrEmpty(pendingUnloadScene) && ShouldRetainPredecessor(FindRouteIndex(currentLevelScene)))
        {
            FormalLevelController retainedLevel = FormalLevelActors.FindLevelController(SceneManager.GetSceneByName(currentLevelScene));
            if (retainedLevel != null)
                retainedLevel.ResetLevel();

            Scene retainedScene = SceneManager.GetSceneByName(pendingUnloadScene);
            if (retainedScene.isLoaded)
            {
                foreach (GameObject root in retainedScene.GetRootGameObjects())
                    foreach (MonsterPatrol monster in root.GetComponentsInChildren<MonsterPatrol>(true))
                        monster.ResetPatrol();
            }

            BeginRetainedPredecessorPursuitSequence();
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

        if (FindRouteSuccessorIndex(currentLevelScene) >= 0)
            RequestRouteAdvance();
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

    IEnumerator AdvanceRoutine(string successorScene)
    {
        operationInProgress = true;

        yield return LoadSharedArtForEntries(currentLevelScene, pendingUnloadScene, successorScene);
        OpenTransitionDoor(currentLevelScene, successorScene);

        yield return LoadLevelRoutineCore(successorScene, null, discardPriorLevel: false, keepPriorLevels: true, manageOperationGuard: false);

        operationInProgress = false;
        DrainPendingAdvance();
    }

    void DrainPendingAdvance()
    {
        if (string.IsNullOrEmpty(pendingAdvanceFromScene))
            return;

        string originScene = pendingAdvanceFromScene;
        pendingAdvanceFromScene = null;

        if (originScene != currentLevelScene || routeComplete || operationInProgress)
        {
            Debug.Log($"[FormalGameFlowController] Discarded stale deferred advance from {originScene}.");
            return;
        }

        Debug.Log("[FormalGameFlowController] Executing deferred route advance.");
        RequestRouteAdvance();
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
        DrainPendingAdvance();
    }

    IEnumerator LoadLevelRoutine(string sceneName, Action<bool> completed, bool discardPriorLevel, bool keepPriorLevels = false)
    {
        return LoadLevelRoutineCore(sceneName, completed, discardPriorLevel, keepPriorLevels, manageOperationGuard: true);
    }

    IEnumerator LoadLevelRoutineCore(string sceneName, Action<bool> completed, bool discardPriorLevel, bool keepPriorLevels, bool manageOperationGuard)
    {
        FormalRouteEntry target = FindRouteEntryByScene(sceneName);
        if (target == null)
        {
            completed?.Invoke(false);
            yield break;
        }

        if (manageOperationGuard)
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
        if (discardPriorLevel || string.IsNullOrEmpty(predecessorScene))
            PlacePlayersAtLoadedLevelSpawn();
        if (!keepPriorLevels)
            yield return UnloadIrrelevantLevels(priorPendingScene);
        if (!discardPriorLevel && predecessorScene != sceneName)
            yield return ArrivalCleanup();
        yield return UnloadUnusedSharedArt();
        completed?.Invoke(true);

        if (manageOperationGuard)
        {
            operationInProgress = false;
            DrainPendingAdvance();
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
        DrainPendingAdvance();
        completed?.Invoke(loaded);
    }

    IEnumerator LoadSharedArtForEntries(string activeScene, string transitionalScene, string targetScene)
    {
        HashSet<string> required = GetRequiredSharedScenes(activeScene, transitionalScene, targetScene);
        foreach (string sharedScene in required)
        {
            if (!string.IsNullOrEmpty(sharedScene) && !SceneManager.GetSceneByName(sharedScene).isLoaded)
            {
                string scenePath = "Assets/MoMing/FormalLevels/" + sharedScene + ".unity";
                yield return SceneManager.LoadSceneAsync(scenePath, LoadSceneMode.Additive);
            }
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

            if (ShouldRetainPredecessor(index) && levelScene == predecessor)
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
                    if (MatchesRegisteredDoorToken(door.name, to.arrivalTransitionDoorName))
                        return door;
        }

        return null;
    }

    static bool MatchesRegisteredDoorToken(string doorName, string registeredToken)
    {
        if (string.IsNullOrEmpty(doorName))
            return false;

        string token = string.IsNullOrEmpty(registeredToken) ? "ToLevel" : registeredToken;
        return doorName.IndexOf(token, StringComparison.OrdinalIgnoreCase) >= 0;
    }

    string GetRouteSuccessor(string fromScene)
    {
        int index = FindRouteSuccessorIndex(fromScene);
        return index < 0 ? null : routeCatalog[index].sceneName;
    }

    int FindRouteSuccessorIndex(string fromScene)
    {
        int index = FindRouteIndex(fromScene);
        if (index < 0 || index + 1 >= routeCatalog.Length)
            return -1;
        return index + 1;
    }

    bool ShouldRetainPredecessor(int routeIndex)
    {
        return routeIndex >= 0 && routeIndex < routeCatalog.Length &&
               routeCatalog[routeIndex] != null &&
               routeCatalog[routeIndex].levelId == RetainedPredecessorLevelId;
    }

    bool HasArrivalSequence(int routeIndex)
    {
        return ShouldRetainPredecessor(routeIndex);
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
            Entry("Level01", "FormalLevel01", "", level01Level02SharedArtScene),
            Entry("Level02", "FormalLevel02", "ToLevel02", level01Level02SharedArtScene, level02Level03SharedArtScene),
            Entry("Level03", "FormalLevel03", "ToLevel03", level02Level03SharedArtScene, level03Level04SharedArtScene),
            Entry("Level04", "FormalLevel04", "ToLevel04", level03Level04SharedArtScene, level04Level045SharedArtScene),
            Entry("Level04.5", "FormalLevel045", "ToLevel045", level04Level045SharedArtScene, level045Level05SharedArtScene),
            Entry("Level05", "FormalLevel05", "ToLevel05", level045Level05SharedArtScene)
        };
    }

    static FormalRouteEntry Entry(string id, string scene, string arrivalDoorToken, params string[] sharedScenes)
    {
        return new FormalRouteEntry
        {
            levelId = id,
            sceneName = scene,
            arrivalTransitionDoorName = arrivalDoorToken,
            sharedArtScenes = sharedScenes
        };
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

    void BeginRetainedPredecessorPursuitSequence()
    {
        if (!ShouldRetainPredecessor(FindRouteIndex(currentLevelScene)) || string.IsNullOrEmpty(pendingUnloadScene))
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

        int currentIndex = FindRouteIndex(currentLevelScene);
        if (!ShouldRetainPredecessor(currentIndex) || string.IsNullOrEmpty(pendingUnloadScene))
            yield break;

        Scene retainedScene = SceneManager.GetSceneByName(pendingUnloadScene);
        if (!retainedScene.isLoaded)
            yield break;

        foreach (GameObject root in retainedScene.GetRootGameObjects())
            foreach (MonsterPatrol monster in root.GetComponentsInChildren<MonsterPatrol>(true))
                monster.BeginForcedChase(actors.Human.transform);
    }

    static T[] FindInScene<T>(Scene scene) where T : Component
    {
        List<T> result = new List<T>();
        foreach (GameObject root in scene.GetRootGameObjects())
            result.AddRange(root.GetComponentsInChildren<T>(true));
        return result.ToArray();
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
