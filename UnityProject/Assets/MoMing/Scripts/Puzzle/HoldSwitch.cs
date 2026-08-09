using UnityEngine;

/// <summary>
/// 持续按住开关：人角色按住F键维持激活状态，松开则关闭。
/// 可选 holdTime 模式：按住N秒后永久激活。
/// 用于体现"按住摇杆/重型闸阀"能力。
/// </summary>
public class HoldSwitch : MonoBehaviour
{
    [Header("Linked Gate")]
    public GateController linkedGate;

    [Header("Settings")]
    public bool permanentAfterHold = false;
    public float holdTimeRequired = 0f;

    [Header("Visual")]
    public Renderer indicatorRenderer;
    public Material inactiveMaterial;
    public Material activeMaterial;

    [Header("Audio")]
    [SerializeField] private AudioClip activateClip;
    [SerializeField] private AudioClip deactivateClip;

    private bool isActivated = false;
    private bool isPermanent = false;
    private float currentHoldTime = 0f;
    private bool isPlayerNearby = false;
    private bool isHolding = false;
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

    void Update()
    {
        if (isPermanent) return;

        // 只有人角色在附近且按住F才激活
        isHolding = isPlayerNearby && Input.GetKey(KeyCode.F);

        if (isHolding)
        {
            if (holdTimeRequired > 0f)
            {
                currentHoldTime += Time.deltaTime;
                if (currentHoldTime >= holdTimeRequired)
                {
                    SetActive(true);
                    isPermanent = permanentAfterHold;
                    return;
                }
            }
            else
            {
                SetActive(true);
            }
        }
        else
        {
            currentHoldTime = 0f;
            SetActive(false);
        }
    }

    void SetActive(bool active)
    {
        if (isActivated == active) return;

        isActivated = active;

        if (indicatorRenderer != null)
        {
            if (active && activeMaterial != null)
                indicatorRenderer.material = activeMaterial;
            else if (!active && inactiveMaterial != null)
                indicatorRenderer.material = inactiveMaterial;
        }

        PlayAudio(active ? activateClip : deactivateClip);

        if (linkedGate != null)
        {
            if (active) linkedGate.Open();
            else linkedGate.Close();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            var pc = other.GetComponent<PlayerController>();
            if (pc != null && pc.characterType == PlayerController.CharacterType.Human)
                isPlayerNearby = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            var pc = other.GetComponent<PlayerController>();
            if (pc != null && pc.characterType == PlayerController.CharacterType.Human)
                isPlayerNearby = false;
        }
    }

    void Reset()
    {
        isActivated = false;
        isPermanent = false;
        currentHoldTime = 0f;
        isPlayerNearby = false;
        isHolding = false;
        if (indicatorRenderer != null && inactiveMaterial != null)
            indicatorRenderer.material = inactiveMaterial;
        if (linkedGate != null)
            linkedGate.Close();
    }

    void PlayAudio(AudioClip clip)
    {
        if (clip != null && audioSource != null)
            audioSource.PlayOneShot(clip);
    }

    public bool IsActivated => isActivated;
}
