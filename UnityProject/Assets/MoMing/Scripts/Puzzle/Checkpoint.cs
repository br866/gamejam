using UnityEngine;

/// <summary>
/// 存档点：玩家踩中后保存当前双角色位置，之后重开（焦虑值过高/被怪物捉住）从存档点复活。
/// 激活后永久保持，不随关卡重置而清除。多个存档点按踩中顺序覆盖，最后踩中的为当前复活点。
/// </summary>
public class Checkpoint : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("狗的复活偏移（相对存档点中心）")]
    public Vector3 dogOffset = new Vector3(1.5f, 0f, 0f);

    [Header("Visual")]
    public Renderer indicatorRenderer;
    public Material inactiveMaterial;
    public Material activeMaterial;

    [Header("Audio")]
    [SerializeField] private AudioClip activateClip;

    private bool isActivated = false;
    private AudioSource audioSource;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    void Start()
    {
        if (indicatorRenderer != null && inactiveMaterial != null)
            indicatorRenderer.material = inactiveMaterial;
    }

    void OnTriggerEnter(Collider other)
    {
        if (isActivated) return;
        if (!other.CompareTag("Player")) return;

        isActivated = true;

        if (indicatorRenderer != null && activeMaterial != null)
            indicatorRenderer.material = activeMaterial;

        PlayAudio(activateClip);

        if (GameManager.Instance != null)
        {
            Vector3 humanPos = transform.position + new Vector3(0f, 1f, 0f);
            Vector3 dogPos = transform.position + dogOffset + new Vector3(0f, 1f, 0f);

            if (GameManager.Instance.humanPlayer != null)
                humanPos = GameManager.Instance.humanPlayer.position;
            if (GameManager.Instance.dogPlayer != null)
                dogPos = GameManager.Instance.dogPlayer.position;

            GameManager.Instance.SetCheckpoint(humanPos, dogPos);
        }

        Debug.Log("[Checkpoint] Activated! Respawn point saved.");
    }

    void PlayAudio(AudioClip clip)
    {
        if (clip != null && audioSource != null)
            audioSource.PlayOneShot(clip);
    }

    public bool IsActivated => isActivated;
}
