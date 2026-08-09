using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 序列门控制器：玩家需要按顺序踩踏板才能解锁门。
/// 监听关联PressurePlate的激活状态，按sequencePlates顺序验证。
/// 踩错顺序时重置进度（需要重新从第一个踏板开始）。
/// 建议关联踏板设为 requireContinuousPressure=true, requiredCount=1, linkedGates 留空。
/// </summary>
public class SequenceGateController : MonoBehaviour
{
    [Header("Gate")]
    public GateController gate;

    [Header("Sequence Plates (按踩踏顺序排列)")]
    public List<PressurePlate> sequencePlates = new List<PressurePlate>();

    [Header("Settings")]
    [Tooltip("踩错顺序时是否重置进度")]
    public bool resetOnWrongOrder = true;

    [Header("Visual")]
    [Tooltip("踩对时踏板显示的材质（绿色）")]
    public Material correctMaterial;
    [Tooltip("重置时踏板显示的材质（黄色）")]
    public Material inactiveMaterial;

    [Header("Audio")]
    [SerializeField] private AudioClip correctStepClip;
    [SerializeField] private AudioClip wrongOrderClip;
    [SerializeField] private AudioClip unlockedClip;

    private int currentStep = 0;
    private bool[] wasActive;
    private bool isUnlocked = false;
    private AudioSource audioSource;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    void Start()
    {
        InitWasActive();
        if (GameManager.Instance != null)
            GameManager.Instance.OnLevelReset += ResetSequence;
    }

    void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnLevelReset -= ResetSequence;
    }

    void InitWasActive()
    {
        wasActive = new bool[sequencePlates.Count];
        for (int i = 0; i < sequencePlates.Count; i++)
        {
            if (sequencePlates[i] != null)
                wasActive[i] = sequencePlates[i].IsActive;
        }
    }

    void Update()
    {
        if (isUnlocked || wasActive == null || sequencePlates.Count == 0) return;

        for (int i = 0; i < sequencePlates.Count; i++)
        {
            if (sequencePlates[i] == null) continue;

            bool nowActive = sequencePlates[i].IsActive;

            if (nowActive && !wasActive[i])
                OnPlatePressed(i);

            wasActive[i] = nowActive;
        }
    }

    void OnPlatePressed(int plateIndex)
    {
        if (plateIndex == currentStep)
        {
            currentStep++;
            SetPlateVisual(plateIndex, true);
            PlayAudio(correctStepClip);
            Debug.Log("[SequenceGate] Correct step " + currentStep + "/" + sequencePlates.Count);

            if (currentStep >= sequencePlates.Count)
                Unlock();
        }
        else if (resetOnWrongOrder)
        {
            currentStep = 0;
            ResetAllPlateVisuals();
            for (int i = 0; i < sequencePlates.Count; i++)
            {
                if (sequencePlates[i] != null)
                {
                    sequencePlates[i].ForceReset();
                    wasActive[i] = false;
                }
            }
            PlayAudio(wrongOrderClip);
            Debug.Log("[SequenceGate] Wrong order! Sequence reset.");
        }
    }

    void Unlock()
    {
        isUnlocked = true;
        PlayAudio(unlockedClip);
        if (gate != null)
            gate.Open();
        Debug.Log("[SequenceGate] Unlocked! Gate opening.");
    }

    void ResetSequence()
    {
        currentStep = 0;
        isUnlocked = false;
        for (int i = 0; i < sequencePlates.Count; i++)
        {
            if (sequencePlates[i] != null)
            {
                sequencePlates[i].ForceReset();
                wasActive[i] = false;
            }
        }
        if (gate != null)
            gate.Close();
    }

    void SetPlateVisual(int index, bool correct)
    {
        var plate = sequencePlates[index];
        if (plate == null || plate.indicatorRenderer == null) return;

        plate.lockVisual = true;
        plate.indicatorRenderer.material = correct ? correctMaterial : inactiveMaterial;
    }

    void ResetAllPlateVisuals()
    {
        for (int i = 0; i < sequencePlates.Count; i++)
        {
            if (sequencePlates[i] == null) continue;
            sequencePlates[i].lockVisual = false;
            if (sequencePlates[i].indicatorRenderer != null && inactiveMaterial != null)
                sequencePlates[i].indicatorRenderer.material = inactiveMaterial;
        }
    }

    void PlayAudio(AudioClip clip)
    {
        if (clip != null && audioSource != null)
            audioSource.PlayOneShot(clip);
    }

    public bool IsUnlocked => isUnlocked;
    public int CurrentStep => currentStep;
}
