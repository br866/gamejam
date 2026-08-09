using UnityEngine;

/// <summary>
/// 可拾取道具：人角色按 E 拾取，再按 E 丢弃。
/// </summary>
public class PickupItem : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] private AudioClip pickupClip;
    [SerializeField] private AudioClip dropClip;

    private bool isHeld = false;
    private Transform holder;
    private Vector3 originalPosition;
    private AudioSource audioSource;
    private Collider col;
    private Rigidbody rb;

    public bool IsHeld => isHeld;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        col = GetComponent<Collider>();
        rb = GetComponent<Rigidbody>();
        originalPosition = transform.position;
    }

    void Start()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnLevelReset += ResetItem;
    }

    void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnLevelReset -= ResetItem;
    }

    void Update()
    {
        if (isHeld && holder != null)
        {
            transform.position = holder.position + holder.forward * 0.8f + Vector3.up * 0.5f;
        }
    }

    public void Pickup(Transform newHolder)
    {
        if (isHeld) return;

        isHeld = true;
        holder = newHolder;

        if (rb != null) { rb.isKinematic = true; rb.useGravity = false; }
        if (col != null) col.enabled = false;

        PlayAudio(pickupClip);
        Debug.Log("[PickupItem] Picked up by " + newHolder.name);
    }

    public void Drop()
    {
        if (!isHeld) return;

        isHeld = false;
        Vector3 dropPos = holder.position + holder.forward * 1f;
        dropPos.y = 0.5f;
        transform.position = dropPos;
        holder = null;

        if (rb != null) { rb.isKinematic = false; rb.useGravity = true; }
        if (col != null) col.enabled = true;

        PlayAudio(dropClip);
        Debug.Log("[PickupItem] Dropped.");
    }

    void ResetItem()
    {
        isHeld = false;
        holder = null;
        transform.position = originalPosition;
        if (rb != null) { rb.isKinematic = false; rb.useGravity = true; }
        if (col != null) col.enabled = true;
    }

    void PlayAudio(AudioClip clip)
    {
        if (clip != null && audioSource != null)
            audioSource.PlayOneShot(clip);
    }
}
