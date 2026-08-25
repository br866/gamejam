using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Keeps the formal route's Wwise interactive-music States synchronized with
/// monster chase and anxiety state, then posts the configured AkAmbient once.
/// </summary>
[AddComponentMenu("MoMing/Audio/Formal Wwise Music Controller")]
[RequireComponent(typeof(AkAmbient))]
public sealed class FormalWwiseMusicController : MonoBehaviour
{
    [Header("Wwise Events")]
    [Tooltip("Stops Gameplay_Music. Its Wwise Stop Action currently fades for 0.5 seconds.")]
    [SerializeField] private AK.Wwise.Event stopGameplayMusicEvent = new AK.Wwise.Event();
    [Min(0f)]
    [SerializeField] private float restartFadeSeconds = 0.5f;

    [Header("Level 4.5 Long Corridor Music")]
    [Tooltip("The Wwise Event is named Level5 for design-document compatibility; its runtime scope is FormalLevel045.")]
    [SerializeField] private AK.Wwise.Event playLevel5MusicEvent = new AK.Wwise.Event();
    [SerializeField] private AK.Wwise.Event stopLevel5MusicEvent = new AK.Wwise.Event();
    [SerializeField] private string level5MusicScene = "FormalLevel045";
    [Min(0f)]
    [Tooltip("Matches the Stop_Level5_Music fade authored in Wwise.")]
    [SerializeField] private float level5RestartFadeSeconds = 2f;

    [Header("Wwise State Names")]
    [SerializeField] private string musicModeGroup = "MusicMode";
    [SerializeField] private string exploreState = "Explore";
    [SerializeField] private string combatState = "Combat";
    [SerializeField] private string anxietyLevelGroup = "AnxietyLevel";
    [SerializeField] private string lowAnxietyState = "Low";
    [SerializeField] private string midAnxietyState = "Mid";
    [SerializeField] private string highAnxietyState = "High";

    [Header("Anxiety Thresholds")]
    [Range(0f, 1f)]
    [SerializeField] private float midThreshold = 0.45f;
    [Range(0f, 1f)]
    [SerializeField] private float highThreshold = 0.75f;

    private enum AnxietyBand
    {
        Unset = -1,
        Low,
        Mid,
        High,
    }

    private AkAmbient musicEvent;
    private FormalAnxietyState anxietyState;
    private FormalGameFlowController gameFlow;
    private MonsterPatrol[] monsters = new MonsterPatrol[0];
    private int appliedMusicMode = -1;
    private AnxietyBand appliedAnxietyBand = AnxietyBand.Unset;
    private Coroutine restartMusicRoutine;
    private bool level5MusicActive;
    private bool warnedMissingPlayEvent;
    private bool warnedMissingStopEvent;
    private bool warnedMissingPlayLevel5Event;
    private bool warnedMissingStopLevel5Event;

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
    }

    IEnumerator Start()
    {
        musicEvent = GetComponent<AkAmbient>();

        // FormalPersistent initializes all of its Awake methods before Start,
        // but waiting also keeps direct/additive scene testing deterministic.
        while (FormalAnxietyState.Instance == null)
            yield return null;

        anxietyState = FormalAnxietyState.Instance;
        gameFlow = FindObjectOfType<FormalGameFlowController>();
        RefreshMonsters();
        ApplyStates(true);

        // AkAmbient must use Trigger On = Nothing, otherwise the Event is
        // posted once by AkAmbient and once again here.
        level5MusicActive = IsLevel5MusicSceneCurrent();
        PostCurrentMusic();
    }

    void Update()
    {
        bool force = false;
        if (anxietyState != FormalAnxietyState.Instance)
        {
            anxietyState = FormalAnxietyState.Instance;
            force = true;
        }

        if (gameFlow == null)
            gameFlow = FindObjectOfType<FormalGameFlowController>();

        ApplyStates(force);
        ApplyRouteMusic();
    }

    void ApplyStates(bool force)
    {
        ApplyMusicMode(force);

        if (anxietyState != null)
            ApplyAnxietyLevel(force);
    }

    /// <summary>
    /// Stops the active route-music instance and starts the correct one fresh,
    /// so a level restart also resets its timeline.
    /// </summary>
    public void RestartFromBeginning()
    {
        if (!isActiveAndEnabled)
            return;

        if (restartMusicRoutine != null)
            StopCoroutine(restartMusicRoutine);
        restartMusicRoutine = StartCoroutine(RestartFromBeginningRoutine());
    }

    /// <summary>Stops the active route soundtrack without reposting it.</summary>
    public void StopGameplayMusic()
    {
        if (restartMusicRoutine != null)
        {
            StopCoroutine(restartMusicRoutine);
            restartMusicRoutine = null;
        }

        TryPostStopCurrentMusic();
    }

    IEnumerator RestartFromBeginningRoutine()
    {
        bool restartingLevel5Music = IsLevel5MusicSceneCurrent();
        if (!TryPostStopCurrentMusic())
        {
            restartMusicRoutine = null;
            yield break;
        }

        float fadeSeconds = restartingLevel5Music
            ? level5RestartFadeSeconds
            : restartFadeSeconds;
        if (fadeSeconds > 0f)
            yield return new WaitForSecondsRealtime(fadeSeconds);

        appliedMusicMode = -1;
        appliedAnxietyBand = AnxietyBand.Unset;
        ApplyStates(true);
        level5MusicActive = IsLevel5MusicSceneCurrent();
        PostCurrentMusic();
        restartMusicRoutine = null;
    }

    bool TryPostStopCurrentMusic()
    {
        return level5MusicActive
            ? TryPostStopLevel5Music()
            : TryPostStopGameplayMusic();
    }

    bool TryPostStopGameplayMusic()
    {
        if (stopGameplayMusicEvent != null && stopGameplayMusicEvent.IsValid())
        {
            stopGameplayMusicEvent.Post(gameObject);
            return true;
        }

        if (!warnedMissingStopEvent)
        {
            Debug.LogWarning("[FormalWwiseMusicController] Stop_Gameplay_Music is not assigned.", this);
            warnedMissingStopEvent = true;
        }

        return false;
    }

    void PostGameplayMusic()
    {
        if (musicEvent != null && musicEvent.data != null && musicEvent.data.IsValid())
        {
            musicEvent.data.Post(gameObject);
            return;
        }

        if (!warnedMissingPlayEvent)
        {
            Debug.LogWarning("[FormalWwiseMusicController] Play_Gameplay_Music is not assigned on AkAmbient.", this);
            warnedMissingPlayEvent = true;
        }
    }

    void PostCurrentMusic()
    {
        if (level5MusicActive)
            PostLevel5Music();
        else
            PostGameplayMusic();
    }

    void PostLevel5Music()
    {
        if (playLevel5MusicEvent != null && playLevel5MusicEvent.IsValid())
        {
            playLevel5MusicEvent.Post(gameObject);
            return;
        }

        if (!warnedMissingPlayLevel5Event)
        {
            Debug.LogWarning("[FormalWwiseMusicController] Play_Level5_Music is not assigned.", this);
            warnedMissingPlayLevel5Event = true;
        }
    }

    bool TryPostStopLevel5Music()
    {
        if (stopLevel5MusicEvent != null && stopLevel5MusicEvent.IsValid())
        {
            stopLevel5MusicEvent.Post(gameObject);
            return true;
        }

        if (!warnedMissingStopLevel5Event)
        {
            Debug.LogWarning("[FormalWwiseMusicController] Stop_Level5_Music is not assigned.", this);
            warnedMissingStopLevel5Event = true;
        }

        return false;
    }

    void ApplyRouteMusic()
    {
        if (gameFlow == null || string.IsNullOrEmpty(gameFlow.CurrentLevelScene))
            return;

        bool shouldUseLevel5Music = IsLevel5MusicSceneCurrent();
        if (shouldUseLevel5Music == level5MusicActive)
            return;

        if (restartMusicRoutine != null)
        {
            StopCoroutine(restartMusicRoutine);
            restartMusicRoutine = null;
        }

        if (shouldUseLevel5Music)
        {
            TryPostStopGameplayMusic();
            level5MusicActive = true;
            PostLevel5Music();
            Debug.Log("[FormalWwiseMusicController] Entered FormalLevel045; playing Level5 corridor music.", this);
            return;
        }

        TryPostStopLevel5Music();
        level5MusicActive = false;
        appliedMusicMode = -1;
        appliedAnxietyBand = AnxietyBand.Unset;
        ApplyStates(true);
        PostGameplayMusic();
        Debug.Log("[FormalWwiseMusicController] Left FormalLevel045; restored gameplay music.", this);
    }

    bool IsLevel5MusicSceneCurrent()
    {
        return gameFlow != null
            && !string.IsNullOrEmpty(level5MusicScene)
            && gameFlow.CurrentLevelScene == level5MusicScene;
    }

    void ApplyMusicMode(bool force)
    {
        int desiredMode = AnyMonsterIsChasing() ? 1 : 0;
        if (!force && desiredMode == appliedMusicMode)
            return;

        string desiredState = desiredMode == 1 ? combatState : exploreState;
        SetWwiseState(musicModeGroup, desiredState);
        appliedMusicMode = desiredMode;
    }

    void ApplyAnxietyLevel(bool force)
    {
        float normalized = anxietyState.Normalized;
        AnxietyBand desiredBand = normalized >= highThreshold
            ? AnxietyBand.High
            : normalized >= midThreshold
                ? AnxietyBand.Mid
                : AnxietyBand.Low;

        if (!force && desiredBand == appliedAnxietyBand)
            return;

        string desiredState = desiredBand == AnxietyBand.High
            ? highAnxietyState
            : desiredBand == AnxietyBand.Mid
                ? midAnxietyState
                : lowAnxietyState;

        SetWwiseState(anxietyLevelGroup, desiredState);
        appliedAnxietyBand = desiredBand;
    }

    bool AnyMonsterIsChasing()
    {
        for (int i = 0; i < monsters.Length; i++)
        {
            MonsterPatrol monster = monsters[i];
            if (monster != null && monster.isActiveAndEnabled && monster.IsChasing)
                return true;
        }

        return false;
    }

    void RefreshMonsters()
    {
        monsters = FindObjectsOfType<MonsterPatrol>();
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        RefreshMonsters();
    }

    void OnSceneUnloaded(Scene scene)
    {
        RefreshMonsters();
    }

    static void SetWwiseState(string groupName, string stateName)
    {
        if (string.IsNullOrEmpty(groupName) || string.IsNullOrEmpty(stateName))
        {
            Debug.LogError("[FormalWwiseMusicController] Wwise State Group and State names cannot be empty.");
            return;
        }

        AKRESULT result = AkUnitySoundEngine.SetState(groupName, stateName);
        if (result != AKRESULT.AK_Success)
        {
            Debug.LogWarning(
                "[FormalWwiseMusicController] Failed to set Wwise State " +
                groupName + "/" + stateName + ": " + result);
        }
    }

    void OnValidate()
    {
        restartFadeSeconds = Mathf.Max(0f, restartFadeSeconds);
        level5RestartFadeSeconds = Mathf.Max(0f, level5RestartFadeSeconds);
        midThreshold = Mathf.Clamp01(midThreshold);
        highThreshold = Mathf.Clamp(highThreshold, midThreshold, 1f);
    }
}
