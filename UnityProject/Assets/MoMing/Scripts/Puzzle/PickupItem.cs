using UnityEngine;

/// <summary>
/// 可拾取道具：人角色按 E 拾取，再按 E 丢弃。
/// </summary>
public class PickupItem : MonoBehaviour
{
    private bool isHeld = false;
    private Transform holder;
    private Vector3 originalPosition;
    private Collider col;
    private Rigidbody rb;

    public bool IsHeld => isHeld;

    void Awake()
    {
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

        Debug.Log("[PickupItem] Picked up by " + newHolder.name);
    }

    public void Drop()
    {
        if (!isHeld) return;

        isHeld = false;
        Vector3 dropPos = holder.position + holder.forward * 1f;
        dropPos.y = holder.position.y;
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
            rb.position = dropPos;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
        else
        {
            transform.position = dropPos;
        }
        holder = null;

        if (col != null) col.enabled = true;

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

}
