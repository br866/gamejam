using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Serialization;

/// <summary>
/// 压力踏板：可控制多个门。
/// requireContinuousPressure=false（默认）：踩过即永久计入，达到 requiredCount 后永久开门。
/// requireContinuousPressure=true：需要持续踩着，离开后门恢复关闭。
/// </summary>
public class PressurePlate : MonoBehaviour
{
    [Header("Linked Gates")]
    [FormerlySerializedAs("linkedGate")]
    public List<GateController> linkedGates = new List<GateController>();

    [Header("Settings")]
    public int requiredCount = 2;
    [Tooltip("true=需要持续踩着，离开后门关闭；false=踩过即永久计入")]
    public bool requireContinuousPressure = false;

    [Header("Visual")]
    public Renderer indicatorRenderer;
    public Material inactiveMaterial;
    public Material activeMaterial;

    [Header("Audio")]
    [SerializeField] private AudioClip activateClip;
    [SerializeField] private AudioClip deactivateClip;

    [HideInInspector] public bool lockVisual = false;

    private HashSet<Collider> steppedObjects = new HashSet<Collider>();
    private bool isActive = false;
    private AudioSource audioSource;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    void Start()
    {
        if (indicatorRenderer != null && inactiveMaterial != null)
            indicatorRenderer.material = inactiveMaterial;

        if (GameManager.Instance != null)
            GameManager.Instance.OnLevelReset += Reset;
    }

    void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnLevelReset -= Reset;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!steppedObjects.Contains(other))
        {
            steppedObjects.Add(other);
            Debug.Log("[PressurePlate] Object stepped on. Total=" + steppedObjects.Count + "/" + requiredCount);
            UpdateState();
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (steppedObjects.Remove(other))
        {
            Debug.Log("[PressurePlate] Object left. Total=" + steppedObjects.Count + "/" + requiredCount);
            UpdateState();
        }
    }

    void UpdateState()
    {
        steppedObjects.RemoveWhere(c => c == null);

        if (!isActive && steppedObjects.Count >= requiredCount)
        {
            isActive = true;
            if (!lockVisual && indicatorRenderer != null && activeMaterial != null)
                indicatorRenderer.material = activeMaterial;
            PlayAudio(activateClip);
            OpenAllGates();
            Debug.Log("[PressurePlate] Activated! Total=" + steppedObjects.Count);
        }
        else if (isActive && requireContinuousPressure && steppedObjects.Count < requiredCount)
        {
            isActive = false;
            if (!lockVisual && indicatorRenderer != null && inactiveMaterial != null)
                indicatorRenderer.material = inactiveMaterial;
            PlayAudio(deactivateClip);
            CloseAllGates();
            Debug.Log("[PressurePlate] Deactivated (pressure lost). Total=" + steppedObjects.Count);
        }
    }

    void OpenAllGates()
    {
        if (linkedGates == null) return;
        foreach (var gate in linkedGates)
        {
            if (gate != null)
                gate.Open();
        }
    }

    void CloseAllGates()
    {
        if (linkedGates == null) return;
        foreach (var gate in linkedGates)
        {
            if (gate != null)
                gate.Close();
        }
    }

    void Reset()
    {
        steppedObjects.Clear();
        isActive = false;
        lockVisual = false;
        if (indicatorRenderer != null && inactiveMaterial != null)
            indicatorRenderer.material = inactiveMaterial;
        CloseAllGates();
    }

    public void ForceReset()
    {
        steppedObjects.Clear();
        isActive = false;
        lockVisual = false;
        if (indicatorRenderer != null && inactiveMaterial != null)
            indicatorRenderer.material = inactiveMaterial;
    }

    void PlayAudio(AudioClip clip)
    {
        if (clip != null && audioSource != null)
            audioSource.PlayOneShot(clip);
    }

    public bool IsActive => isActive;
}
