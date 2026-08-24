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
        musicEvent.data.Post(gameObject);
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
        midThreshold = Mathf.Clamp01(midThreshold);
        highThreshold = Mathf.Clamp(highThreshold, midThreshold, 1f);
    }
}

