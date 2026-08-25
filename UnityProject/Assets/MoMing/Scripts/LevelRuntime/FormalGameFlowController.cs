using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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
    [SerializeField] private bool level02TransitionDiagnostics;

    [Header("通关之后")]
    [Tooltip("走出第五关之后整场切到哪个场景。默认是结尾动画；\n" +
             "那个场景上的 CutscenePlayer 播完会自己跳去主菜单（Start）。\n" +
             "留空 = 通关之后什么都不做。")]
    [SerializeField] private string endingCutsceneScene = "Cutscene_End";

    [Tooltip("出门之后先停这么久，让玩家看清自己走出去了，然后才开始渐黑")]
    [SerializeField] private float endingDelaySeconds = 0.6f;

    [Tooltip("渐黑用多久（秒）。黑透之后才切场景，结尾动画自己会从黑幕淡入，接得上")]
    [SerializeField] private float endingFadeSeconds = 1.2f;

    [Tooltip("通关之后在屏幕中间画一个带 Restart 按钮的调试面板。正式版关掉")]
    [SerializeField] private bool showRouteCompleteDebugPanel;

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
    private Coroutine level045PlayerBindingRoutine;
    private string pendingAdvanceFromScene;
    private UnityEngine.Object pendingAdvanceSource;
    private string pendingAdvanceOriginStack;
    private string pendingPhysicalTransitionFromScene;
    private string pendingPhysicalTransitionToScene;
    private string retainedPhysicalPredecessorScene;
    private bool retainedPredecessorReleasedAtLevel05Checkpoint;

    public string CurrentLevelScene => currentLevelScene;
    public IReadOnlyList<FormalRouteEntry> RouteCatalog => routeCatalog;
    public bool IsRouteComplete => routeComplete;
    public bool SuccessorArrivalConfirmed => successorArrivalConfirmed;
    public bool HasPendingPhysicalTransition => !string.IsNullOrEmpty(pendingPhysicalTransitionToScene);

    private static FormalGameFlowController activeInstance;

    void Awake()
    {
        // 这个物体是 DontDestroyOnLoad 的：玩家回主菜单再点「开始游戏」时，
        // 上一局那个控制器会活着飘过来，新加载的 FormalPersistent 又带来一个新的，
        // 场里就同时存在两个。各处都是 FindObjectOfType<FormalGameFlowController>() 拿引用，
        // 抓到上一局那个陈旧的就会出各种怪事：CurrentLevelScene 还停在上一局的关卡，
        // 于是过关触发器、门的推进逻辑一律静默失效（踩踏板这类不碰控制器的机关反而照常）。
        // 新来的这个才属于这一局，把旧的立刻停掉并销毁。
        if (activeInstance != null && activeInstance != this)
        {
            // 先 SetActive(false)：Destroy 要等到帧末，
            // 中间这段时间 FindObjectOfType 还是会找到它。
            activeInstance.gameObject.SetActive(false);
            Destroy(activeInstance.gameObject);
            Debug.Log("[FormalGameFlowController] 清掉了上一局残留的流程控制器。", this);
        }

        activeInstance = this;
        DontDestroyOnLoad(gameObject);
        EnsureRouteCatalog();
    }

    void OnDestroy()
    {
        if (activeInstance == this)
            activeInstance = null;
    }

    void Start()
    {
        if (!string.IsNullOrEmpty(initialLevelScene))
            LoadLevelAsync(initialLevelScene, null);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Keypad7))
            ToggleDogSpeedMultiplier();
        else if (Input.GetKeyDown(KeyCode.Keypad4))
            ReportLevel02GateStatus();

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

    void ToggleDogSpeedMultiplier()
    {
        FormalPlayerActor dog = FormalPlayerActors.Instance != null
            ? FormalPlayerActors.Instance.Dog
            : null;
        if (dog == null)
            dog = FindDogActor();
        if (dog == null)
        {
            Debug.LogWarning("[L02GateGM] Dog speed toggle failed: dog actor is unavailable.", this);
            return;
        }

        bool accelerate = dog.RuntimeMovementSpeedMultiplier < 5f;
        dog.SetRuntimeMovementSpeedMultiplier(accelerate ? 5f : 1f);
        Debug.Log($"[L02GateGM] Dog speed multiplier: {dog.RuntimeMovementSpeedMultiplier:0}x " +
                  $"(base {dog.ConfiguredWalkSpeed:0.##}).", dog);
    }

    void ReportLevel02GateStatus()
    {
        const string level02 = "FormalLevel02";
        const string prefix = "[L02GateGM]";
        if (currentLevelScene != level02)
        {
            Debug.Log($"{prefix} L2 is inactive (active='{currentLevelScene ?? "<none>"}').", this);
            return;
        }

        Scene levelScene = SceneManager.GetSceneByName(level02);
        if (!levelScene.isLoaded)
        {
            Debug.LogWarning($"{prefix} L2 scene is not loaded.", this);
            return;
        }

        FormalDoorInteraction gate = null;
        foreach (FormalDoorInteraction interaction in FindInScene<FormalDoorInteraction>(levelScene))
        {
            if (!string.IsNullOrEmpty(interaction.DoorNameToken) &&
                interaction.DoorNameToken.IndexOf("ToLevel03", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                gate = interaction;
                break;
            }
        }

        if (gate == null)
        {
            Debug.LogWarning($"{prefix} Missing L2 E-interaction targeting ToLevel03.", this);
            return;
        }

        FormalActuatorTrigger safeZone = gate.GetComponent<FormalActuatorTrigger>();
        FormalActuatorTrigger pedal = null;
        List<string> prerequisiteStates = new List<string>();
        int prerequisiteIndex = 0;
        foreach (MonoBehaviour prerequisite in gate.Prerequisites)
        {
            if (prerequisite == null)
            {
                prerequisiteStates.Add($"#{prerequisiteIndex}=NULL");
                prerequisiteIndex++;
                continue;
            }

            IFormalLevelPermanentState state = prerequisite as IFormalLevelPermanentState;
            prerequisiteStates.Add(
                $"#{prerequisiteIndex}={prerequisite.GetType().Name} '{prerequisite.name}' " +
                (state == null ? "(does not implement permanent state)" : $"complete={state.IsComplete}"));
            FormalActuatorTrigger trigger = prerequisite as FormalActuatorTrigger;
            if (trigger != null && trigger != safeZone)
                pedal = trigger;
            prerequisiteIndex++;
        }

        string pedalState = pedal == null ? "not identified as FormalActuatorTrigger" : pedal.IsComplete ? "complete" : "MISSING";
        string safeZoneState = safeZone == null ? "MISSING REFERENCE" : safeZone.IsComplete ? "complete" : "MISSING";
        string occupancyState = gate.HasEligibleOccupant ? "human inside" : "MISSING (human must press E inside)";
        FormalDoor targetDoor = gate.TargetDoor;
        string doorState = targetDoor == null ? "MISSING REFERENCE" : $"resolved '{targetDoor.name}'";
        bool readyForE = gate.ArePrerequisitesComplete && targetDoor != null && !gate.IsOpened;

        Debug.Log(
            $"{prefix} L2->L3 gate: pedal={pedalState}; safe-zone(two players)={safeZoneState}; " +
            $"E-occupancy={occupancyState}; target-door={doorState}; " +
            $"prerequisites=[{string.Join("; ", prerequisiteStates.ToArray())}]; " +
            (gate.IsOpened ? "door already opened." : readyForE ? "READY: human can press E to open." : "NOT READY."),
            gate);
    }

    FormalPlayerActor FindDogActor()
    {
        foreach (FormalPlayerActor actor in FindObjectsOfType<FormalPlayerActor>())
            if (actor.Role == FormalPlayerActor.ActorRole.Dog)
                return actor;

        return null;
    }

    public void RequestRouteAdvance()
    {
        RequestRouteAdvance(null);
    }

    public void RequestRouteAdvance(UnityEngine.Object source)
    {
        string successor = GetRouteSuccessor(currentLevelScene);
        if (string.IsNullOrEmpty(successor))
        {
            // 已经站在路线最后一关（第五关）上还要求推进 = 通关了。
            // 于是最后一关的出口不用另配东西，和前面每一关用同一套触发器就行。
            if (IsLastRouteLevel(currentLevelScene))
                CompleteRoute();
            return;
        }

        LogLevel02AdvanceRequest(source, successor);

        if (operationInProgress)
        {
            if (!string.IsNullOrEmpty(pendingAdvanceFromScene))
                Debug.Log($"[FormalGameFlowController] Replacing deferred advance from {pendingAdvanceFromScene} with request from {currentLevelScene}.");
            pendingAdvanceFromScene = currentLevelScene;
            pendingAdvanceSource = source;
            pendingAdvanceOriginStack = IsDiagnosingLevel02Transition() && successor == "FormalLevel03"
                ? Environment.StackTrace
                : null;
            return;
        }

        StartCoroutine(AdvanceRoutine(successor));
    }

    /// <summary>
    /// 正常实体门使用：只把下一关加法加载进来，保持 currentLevelScene 和角色位置不变。
    /// </summary>
    public bool PreloadRouteSuccessor(UnityEngine.Object source = null, bool openTransitionDoor = false)
    {
        if (routeComplete || operationInProgress || HasPendingPhysicalTransition)
            return false;

        string successor = GetRouteSuccessor(currentLevelScene);
        if (string.IsNullOrEmpty(successor))
            return false;

        pendingPhysicalTransitionFromScene = currentLevelScene;
        pendingPhysicalTransitionToScene = successor;
        Debug.Log(
            $"[PhysicalDoorTransition] preload-request source={DescribeTransitionSource(source)} " +
            $"from='{currentLevelScene}' to='{successor}' openDoor={openTransitionDoor}.",
            source);
        StartCoroutine(PreloadSuccessorRoutine(currentLevelScene, successor, openTransitionDoor));
        return true;
    }

    /// <summary>由预加载目标关卡入口的双人触发区调用；不摆放角色。</summary>
    public bool ConfirmPreloadedPhysicalArrival(string arrivalScene, UnityEngine.Object source = null)
    {
        if (operationInProgress || string.IsNullOrEmpty(arrivalScene) ||
            pendingPhysicalTransitionFromScene != currentLevelScene ||
            pendingPhysicalTransitionToScene != arrivalScene)
            return false;

        Scene target = SceneManager.GetSceneByName(arrivalScene);
        if (!target.isLoaded)
            return false;

        string predecessor = currentLevelScene;
        pendingPhysicalTransitionFromScene = null;
        pendingPhysicalTransitionToScene = null;
        currentLevelScene = arrivalScene;
        pendingUnloadScene = predecessor;
        if (ShouldRetainPredecessor(FindRouteIndex(arrivalScene)))
            retainedPhysicalPredecessorScene = predecessor;
        successorArrivalConfirmed = true;
        SceneManager.SetActiveScene(target);

        // L4.5 的检查点可能位于入口确认区之前。实体过门时，它会在本关
        // 成为当前关卡前先完成，无法再通知一次；因此以成功的到达确认作为
        // 追击序列的可靠起点。
        if (HasArrivalSequence(FindRouteIndex(arrivalScene)))
            BeginRetainedPredecessorPursuitSequence();

        Debug.Log($"[FormalGameFlowController] Physical arrival confirmed: {predecessor} -> {arrivalScene}.", source);
        return true;
    }

    IEnumerator PreloadSuccessorRoutine(string originScene, string successorScene, bool openTransitionDoor)
    {
        operationInProgress = true;
        yield return LoadSharedArtForEntries(originScene, pendingUnloadScene, successorScene);

        if (openTransitionDoor)
        {
            FormalDoor door = FindTransitionDoor(originScene, successorScene);
            if (door != null)
            {
                door.OpenPermanently();
                Debug.Log(
                    $"[PhysicalDoorTransition] door-opened from='{originScene}' to='{successorScene}' door='{door.name}'.",
                    door);
            }
            else
            {
                Debug.LogWarning(
                    $"[PhysicalDoorTransition] door-missing from='{originScene}' to='{successorScene}'.",
                    this);
            }
        }

        Scene successor = SceneManager.GetSceneByName(successorScene);
        if (!successor.isLoaded)
            yield return SceneManager.LoadSceneAsync(successorScene, LoadSceneMode.Additive);

        operationInProgress = false;
        if (pendingPhysicalTransitionFromScene != originScene || pendingPhysicalTransitionToScene != successorScene)
            yield return UnloadUnusedSharedArt();
    }

    void CancelPendingPhysicalTransition()
    {
        pendingPhysicalTransitionFromScene = null;
        pendingPhysicalTransitionToScene = null;
        retainedPhysicalPredecessorScene = null;
        retainedPredecessorReleasedAtLevel05Checkpoint = false;
    }

    public void ReportTransitionDoorOpened(FormalDoor door)
    {
        if (!IsDiagnosingLevel02Transition() || door == null ||
            door.name.IndexOf("ToLevel03", StringComparison.OrdinalIgnoreCase) < 0)
            return;

        Debug.Log(
            $"[L02TransitionDiagnostics] door-opened door='{door.name}' scene='{door.gameObject.scene.name}' " +
            $"active='{currentLevelScene}' busy={operationInProgress}\n{Environment.StackTrace}",
            door);
    }

    void LogLevel02AdvanceRequest(UnityEngine.Object source, string successor)
    {
        if (!IsDiagnosingLevel02Transition() || successor != "FormalLevel03")
            return;

        Component sourceComponent = source as Component;
        string sourceDescription = source == null
            ? "<unspecified>"
            : $"{source.GetType().Name} '{source.name}'" +
              (sourceComponent == null ? string.Empty : $" scene='{sourceComponent.gameObject.scene.name}'");
        Debug.Log(
            $"[L02TransitionDiagnostics] advance-request source={sourceDescription} " +
            $"active='{currentLevelScene}' successor='{successor}' busy={operationInProgress} " +
            $"pending='{pendingAdvanceFromScene}'\n{Environment.StackTrace}",
            source);
    }

    bool IsDiagnosingLevel02Transition()
    {
        return level02TransitionDiagnostics && currentLevelScene == "FormalLevel02";
    }

    static string DescribeTransitionSource(UnityEngine.Object source)
    {
        Component component = source as Component;
        return source == null
            ? "<unspecified>"
            : $"{source.GetType().Name} '{source.name}'" +
              (component == null ? string.Empty : $" scene='{component.gameObject.scene.name}'");
    }

    public void NotifyCheckpointActivated(string sceneName)
    {
        if (routeComplete)
            return;

        if (ReleaseRetainedPredecessorAtLevel05Checkpoint(sceneName))
            return;

        if (sceneName != currentLevelScene)
            return;

        if (!HasArrivalSequence(FindRouteIndex(currentLevelScene)))
            return;

        BeginRetainedPredecessorPursuitSequence();
    }

    bool ReleaseRetainedPredecessorAtLevel05Checkpoint(string sceneName)
    {
        if (sceneName != "FormalLevel05" || operationInProgress ||
            pendingPhysicalTransitionToScene != sceneName ||
            string.IsNullOrEmpty(retainedPhysicalPredecessorScene))
            return false;

        Scene level05 = SceneManager.GetSceneByName(sceneName);
        if (!level05.isLoaded)
            return false;

        string predecessor = currentLevelScene;
        string retainedSceneName = retainedPhysicalPredecessorScene;
        pendingPhysicalTransitionFromScene = null;
        pendingPhysicalTransitionToScene = null;
        currentLevelScene = sceneName;
        pendingUnloadScene = predecessor;
        successorArrivalConfirmed = true;
        SceneManager.SetActiveScene(level05);
        foreach (FormalPlayerControl control in FindObjectsOfType<FormalPlayerControl>())
            control.ForceHumanOnly(false);
        retainedPhysicalPredecessorScene = null;
        retainedPredecessorReleasedAtLevel05Checkpoint = true;
        StartCoroutine(UnloadRetainedPredecessorAtLevel05Checkpoint(retainedSceneName));
        Debug.Log($"[L05RetainedCleanup] checkpoint committed: {predecessor} -> {sceneName}; no player placement.", this);
        return true;
    }

    IEnumerator UnloadRetainedPredecessorAtLevel05Checkpoint(string retainedSceneName)
    {
        operationInProgress = true;

        Scene retained = SceneManager.GetSceneByName(retainedSceneName);
        if (retained.IsValid() && retained.isLoaded)
            yield return SceneManager.UnloadSceneAsync(retained);

        yield return UnloadUnusedSharedArt();
        operationInProgress = false;
        Debug.Log($"[L05RetainedCleanup] unloaded retained scene '{retainedSceneName}'; Level 4.5 remains loaded.", this);
        DrainPendingAdvance();
    }

    public void NotifySuccessorCheckpointActivated(string sceneName, UnityEngine.Object source = null)
    {
        if (routeComplete || sceneName != currentLevelScene)
            return;

        successorArrivalConfirmed = true;
        RequestRouteAdvance(source);
    }

    public void CompleteRoute()
    {
        if (currentLevelScene != "FormalLevel05")
        {
            Debug.LogWarning("Formal route completion can only be triggered from FormalLevel05.");
            return;
        }

        if (routeComplete)
            return;

        routeComplete = true;
        SetFormalControlsEnabled(false);
        StartCoroutine(PlayEndingRoutine());
    }

    /// <summary>
    /// 通关之后：停一小会儿让玩家看清自己出去了，然后整场切到结尾动画场景。
    /// 结尾动画播完（或被跳过）由 CutscenePlayer 自己跳去主菜单，这边不管。
    ///
    /// 注意这里要先把自己从 DontDestroyOnLoad 里拿出来：FormalGameFlow 是常驻的，
    /// 不处理的话它会跟着飘进结尾动画和主菜单，玩家再点「开始游戏」时
    /// 场景里就会出现两个 FormalGameFlowController。
    /// </summary>
    IEnumerator PlayEndingRoutine()
    {
        Time.timeScale = 1f;

        if (endingDelaySeconds > 0f)
            yield return new WaitForSecondsRealtime(endingDelaySeconds);

        yield return FadeToBlackRoutine();

        if (string.IsNullOrEmpty(endingCutsceneScene))
        {
            Debug.LogWarning("[FormalGameFlowController] Ending Cutscene Scene 是空的，通关之后停在原地。", this);
            yield break;
        }

        if (!Application.CanStreamedLevelBeLoaded(endingCutsceneScene))
        {
            Debug.LogError("[FormalGameFlowController] 场景 \"" + endingCutsceneScene +
                           "\" 不在 Build Settings 里，结尾动画放不了。" +
                           "去 File → Build Settings 把它加进去。", this);
            yield break;
        }

        Debug.Log("[FormalGameFlowController] 通关，切到结尾动画 " + endingCutsceneScene + "。", this);

        // 从 DontDestroyOnLoad 搬回当前场景，下面的 LoadScene(Single) 就会顺手把它卸掉；
        // 直接 Destroy 的话销毁和加载都排在帧末，谁先谁后不保证。
        Scene host = SceneManager.GetActiveScene();
        if (host.IsValid() && host.isLoaded)
            SceneManager.MoveGameObjectToScene(gameObject, host);

        SceneManager.LoadScene(endingCutsceneScene);
    }

    /// <summary>
    /// 临时拉一层全屏黑幕把画面盖掉。
    ///
    /// 不用场景里现成的 UI：HUD、焦虑污渍、暂停菜单各挂各的 Canvas，
    /// 想盖住全部就得有个排在最上面的自己的 Canvas。sortingOrder 拉到很高，
    /// 保证盖在 HUD 和焦虑暗角上面。
    ///
    /// 这层黑幕不做 DontDestroyOnLoad —— 下一句 LoadScene 会把它卸掉，
    /// 而结尾动画场景自己就是从全黑淡入的，两边正好接上，中间不会闪一下。
    /// </summary>
    IEnumerator FadeToBlackRoutine()
    {
        GameObject canvasObject = new GameObject("FormalEndingFade");
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = short.MaxValue - 8;

        GameObject imageObject = new GameObject("Black");
        imageObject.transform.SetParent(canvasObject.transform, false);

        Image black = imageObject.AddComponent<Image>();
        black.color = new Color(0f, 0f, 0f, 0f);
        black.raycastTarget = false;

        RectTransform rect = black.rectTransform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        float duration = Mathf.Max(0f, endingFadeSeconds);
        float elapsed = 0f;
        while (elapsed < duration)
        {
            // 用 unscaled：万一有别的东西把 timeScale 压住了，渐黑照样按真实时间走
            elapsed += Time.unscaledDeltaTime;
            black.color = new Color(0f, 0f, 0f, Mathf.Clamp01(elapsed / duration));
            yield return null;
        }

        black.color = Color.black;
        yield return null;
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

        CancelPendingPhysicalTransition();
        StartCoroutine(LoadLevelRoutine(sceneName, null, true));
    }

    public void LoadLevelAsync(string sceneName, Action<bool> completed)
    {
        if (operationInProgress)
        {
            completed?.Invoke(false);
            return;
        }

        CancelPendingPhysicalTransition();
        StartCoroutine(LoadLevelRoutine(sceneName, completed, true));
    }

    /// <summary>
    /// 「封关」：关掉两关之间那道门，并卸载上一关。
    /// 由放在新关卡入口的 FormalLevelEntrySeal 在人和狗都进来之后调用。
    ///
    /// 为什么不在过关点直接卸载：过关点本身是摆在【上一关】场景里的，
    /// 触发那一刻玩家还站在上一关的地板上，当场卸载会让人直接掉下去。
    /// </summary>
    public bool SealPredecessorLevel(bool closeDoor = true, bool unloadPredecessor = true)
    {
        if (routeComplete || string.IsNullOrEmpty(pendingUnloadScene))
            return false;

        // 第 4.5 关要靠保留上一关做追逐。实体入场已确认，调用方可完成封条，
        // 但这里不关闭门也不卸载前关。
        if (ShouldRetainPredecessor(FindRouteIndex(currentLevelScene)))
            return true;

        // L05_Checkpoint has already released the pursuit-only retained L4.
        // The L5 entry seal confirms physical arrival but deliberately keeps L4.5.
        if (currentLevelScene == "FormalLevel05" && retainedPredecessorReleasedAtLevel05Checkpoint)
            return true;

        if (operationInProgress)
            return false;

        StartCoroutine(SealPredecessorRoutine(closeDoor, unloadPredecessor));
        return true;
    }

    IEnumerator SealPredecessorRoutine(bool closeDoor, bool unloadPredecessor)
    {
        operationInProgress = true;

        string predecessor = pendingUnloadScene;
        pendingUnloadScene = null;
        string retainedPredecessor = retainedPhysicalPredecessorScene;
        retainedPhysicalPredecessorScene = null;

        if (closeDoor)
        {
            FormalDoor door = FindTransitionDoor(predecessor, currentLevelScene);
            if (door != null)
                door.Close();
            else
                Debug.LogWarning($"[FormalGameFlowController] 没找到 {predecessor} -> {currentLevelScene} 的过关门，关不上。");
        }

        if (unloadPredecessor && !string.IsNullOrEmpty(predecessor) && predecessor != currentLevelScene)
        {
            Scene stale = SceneManager.GetSceneByName(predecessor);
            if (stale.IsValid() && stale.isLoaded)
                yield return SceneManager.UnloadSceneAsync(stale);
        }

        if (unloadPredecessor && !string.IsNullOrEmpty(retainedPredecessor) &&
            retainedPredecessor != currentLevelScene && retainedPredecessor != predecessor)
        {
            Scene retained = SceneManager.GetSceneByName(retainedPredecessor);
            if (retained.IsValid() && retained.isLoaded)
                yield return SceneManager.UnloadSceneAsync(retained);
        }

        yield return UnloadUnusedSharedArt();

        operationInProgress = false;
        DrainPendingAdvance();
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

    /// <summary>
    /// 调试用的跳下一关（小键盘 2）。走 LoadLevel 而不是 RequestRouteAdvance：
    /// 后者是"走过门"的无缝流程，不会传送角色、也保留上一关，
    /// 用调试键触发就会变成人狗还站在旧关卡、两关同时可见。
    /// 这里和 GoToPreviousLevel / JumpToLevel 保持一致，硬切并把两人都放到出生点。
    /// </summary>
    public void GoToNextLevel()
    {
        int index = FindRouteSuccessorIndex(currentLevelScene);
        if (index >= 0)
            LoadLevel(routeCatalog[index].sceneName);
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

        // 下面有一条异步重载关卡的分支走不到 FormalLevelController.RequestRecovery()，
        // 所以这里先无条件清一次焦虑。
        if (FormalAnxietyState.Instance != null)
            FormalAnxietyState.Instance.ResetAnxiety();

        if (!string.IsNullOrEmpty(pendingUnloadScene))
        {
            FormalLevelController retainedLevel = FormalLevelActors.FindLevelController(SceneManager.GetSceneByName(currentLevelScene));
            if (retainedLevel != null)
                retainedLevel.RequestRecovery();

            if (ShouldRetainPredecessor(FindRouteIndex(currentLevelScene)))
            {
                string retainedSceneName = !string.IsNullOrEmpty(retainedPhysicalPredecessorScene)
                    ? retainedPhysicalPredecessorScene
                    : pendingUnloadScene;
                StartCoroutine(RestoreRetainedPredecessorForLevel045Recovery(retainedSceneName));
            }
            return;
        }

        FormalLevelController level = FormalLevelActors.FindLevelController(SceneManager.GetSceneByName(currentLevelScene));
        if (level != null)
            level.RequestRecovery();
    }

    IEnumerator RestoreRetainedPredecessorForLevel045Recovery(string retainedSceneName)
    {
        if (string.IsNullOrEmpty(retainedSceneName))
            yield break;

        Scene retainedScene = SceneManager.GetSceneByName(retainedSceneName);
        if (!retainedScene.isLoaded)
        {
            operationInProgress = true;
            yield return LoadSharedArtForEntries(currentLevelScene, retainedSceneName, null);
            retainedScene = SceneManager.GetSceneByName(retainedSceneName);
            if (!retainedScene.isLoaded)
                yield return SceneManager.LoadSceneAsync(retainedSceneName, LoadSceneMode.Additive);
            operationInProgress = false;
            Debug.Log($"[L045Pursuit] restored retained scene '{retainedSceneName}' for recovery.", this);
        }

        foreach (GameObject root in retainedScene.GetRootGameObjects())
            foreach (MonsterPatrol monster in root.GetComponentsInChildren<MonsterPatrol>(true))
                monster.ResetPatrol();

        BeginRetainedPredecessorPursuitSequence(restart: true);
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

        // 小键盘 6 是直接跳关：只完成机关状态，不能让实体过门逻辑先启动预加载。
        foreach (FormalActuatorTrigger trigger in FindInScene<FormalActuatorTrigger>(current))
            trigger.CompleteImmediately(triggerRouteOutput: false);

        int successorIndex = FindRouteSuccessorIndex(currentLevelScene);
        if (successorIndex >= 0)
            LoadLevel(routeCatalog[successorIndex].sceneName);
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
        UnityEngine.Object originSource = pendingAdvanceSource;
        string originStack = pendingAdvanceOriginStack;
        pendingAdvanceFromScene = null;
        pendingAdvanceSource = null;
        pendingAdvanceOriginStack = null;

        if (originScene != currentLevelScene || routeComplete || operationInProgress)
        {
            Debug.Log($"[FormalGameFlowController] Discarded stale deferred advance from {originScene}.");
            return;
        }

        Debug.Log("[FormalGameFlowController] Executing deferred route advance.");
        if (IsDiagnosingLevel02Transition() && !string.IsNullOrEmpty(originStack))
            Debug.Log(
                $"[L02TransitionDiagnostics] deferred-origin source='{originSource}' " +
                $"origin='{originScene}'\n{originStack}",
                originSource);
        RequestRouteAdvance(originSource);
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
            level.RequestRecovery();

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

    /// <summary>这一关是不是路线里的最后一关（在表里，而且后面没有了）。</summary>
    bool IsLastRouteLevel(string sceneName)
    {
        int index = FindRouteIndex(sceneName);
        return index >= 0 && index + 1 >= routeCatalog.Length;
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
            level.PlacePlayersAtRespawnAnchors();
    }

    void SetFormalControlsEnabled(bool enabled)
    {
        foreach (FormalPlayerControl control in FindObjectsOfType<FormalPlayerControl>())
            control.enabled = enabled;
    }

    void BeginRetainedPredecessorPursuitSequence(bool restart = false)
    {
        if (!ShouldRetainPredecessor(FindRouteIndex(currentLevelScene)) || string.IsNullOrEmpty(pendingUnloadScene))
        {
            Debug.LogWarning(
                $"[L045Pursuit] not-started current='{currentLevelScene}' retained='{pendingUnloadScene}'.",
                this);
            return;
        }

        if (level045PursuitRoutine != null)
        {
            if (!restart)
            {
                Debug.Log($"[L045Pursuit] already-running retained='{pendingUnloadScene}'; timer preserved.", this);
                return;
            }

            StopCoroutine(level045PursuitRoutine);
            level045PursuitRoutine = null;
        }

        if (level045PlayerBindingRoutine != null)
        {
            StopCoroutine(level045PlayerBindingRoutine);
            level045PlayerBindingRoutine = null;
        }

        level045PlayerBindingRoutine = StartCoroutine(BindLevel045PlayerState());
        level045PursuitRoutine = StartCoroutine(StartLevel045PursuitAfterDelay());
        Debug.Log($"[L045Pursuit] started retained='{pendingUnloadScene}' restart={restart}; attack-delay=10s.", this);
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
                level045PlayerBindingRoutine = null;
                Debug.Log("[L045Pursuit] dog-following bound to human.", this);
                yield break;
            }

            timeout -= Time.unscaledDeltaTime;
            yield return null;
        }

        level045PlayerBindingRoutine = null;
        Debug.LogWarning("[L045Pursuit] dog-following failed: player actors were unavailable for 10 seconds.", this);
    }

    IEnumerator StartLevel045PursuitAfterDelay()
    {
        yield return new WaitForSeconds(10f);

        FormalPlayerActors actors = FormalPlayerActors.Instance;
        if (actors == null || actors.Human == null)
        {
            Debug.LogWarning("[L045Pursuit] attack-not-started: human actor is unavailable after delay.", this);
            yield break;
        }

        int currentIndex = FindRouteIndex(currentLevelScene);
        if (!ShouldRetainPredecessor(currentIndex) || string.IsNullOrEmpty(pendingUnloadScene))
        {
            Debug.LogWarning(
                $"[L045Pursuit] attack-not-started: current='{currentLevelScene}' retained='{pendingUnloadScene}'.",
                this);
            yield break;
        }

        Scene retainedScene = SceneManager.GetSceneByName(pendingUnloadScene);
        if (!retainedScene.isLoaded)
        {
            Debug.LogWarning($"[L045Pursuit] attack-not-started: retained scene '{pendingUnloadScene}' is not loaded.", this);
            yield break;
        }

        int monsters = 0;
        foreach (GameObject root in retainedScene.GetRootGameObjects())
            foreach (MonsterPatrol monster in root.GetComponentsInChildren<MonsterPatrol>(true))
            {
                monsters++;
                monster.BeginForcedChase(actors.Human.transform, actors.Dog != null ? actors.Dog.transform : null);
            }

        Debug.Log($"[L045Pursuit] forced-chase issued to {monsters} monster(s) in '{retainedScene.name}'.", this);
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
        if (!routeComplete || !showRouteCompleteDebugPanel)
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
