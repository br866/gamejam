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
    private MonsterPatrol[] monsters = new MonsterPatrol[0];
    private int appliedMusicMode = -1;
    private AnxietyBand appliedAnxietyBand = AnxietyBand.Unset;
    private Coroutine restartMusicRoutine;
    private bool warnedMissingPlayEvent;
    private bool warnedMissingStopEvent;

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
        RefreshMonsters();
        ApplyStates(true);

        // AkAmbient must use Trigger On = Nothing, otherwise the Event is
        // posted once by AkAmbient and once again here.
        PostGameplayMusic();
    }

    void Update()
    {
        bool force = false;
        if (anxietyState != FormalAnxietyState.Instance)
        {
            anxietyState = FormalAnxietyState.Instance;
            force = true;
        }

        ApplyStates(force);
    }

    void ApplyStates(bool force)
    {
        ApplyMusicMode(force);

        if (anxietyState != null)
            ApplyAnxietyLevel(force);
    }

    /// <summary>
    /// Stops the persistent gameplay-music instance and starts a fresh one after
    /// the Wwise Stop Event's fade, so a level restart also resets the timeline.
    /// </summary>
    public void RestartFromBeginning()
    {
        if (!isActiveAndEnabled)
            return;

        if (restartMusicRoutine != null)
            StopCoroutine(restartMusicRoutine);
        restartMusicRoutine = StartCoroutine(RestartFromBeginningRoutine());
    }

    IEnumerator RestartFromBeginningRoutine()
    {
        if (stopGameplayMusicEvent == null || !stopGameplayMusicEvent.IsValid())
        {
            if (!warnedMissingStopEvent)
            {
                Debug.LogWarning("[FormalWwiseMusicController] Stop_Gameplay_Music is not assigned.", this);
                warnedMissingStopEvent = true;
            }
            restartMusicRoutine = null;
            yield break;
        }

        stopGameplayMusicEvent.Post(gameObject);

        if (restartFadeSeconds > 0f)
            yield return new WaitForSecondsRealtime(restartFadeSeconds);

        appliedMusicMode = -1;
        appliedAnxietyBand = AnxietyBand.Unset;
        ApplyStates(true);
        PostGameplayMusic();
        restartMusicRoutine = null;
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
        midThreshold = Mathf.Clamp01(midThreshold);
        highThreshold = Mathf.Clamp(highThreshold, midThreshold, 1f);
    }
}
