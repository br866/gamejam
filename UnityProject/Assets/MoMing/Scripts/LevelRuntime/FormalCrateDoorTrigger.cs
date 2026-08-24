using UnityEngine;

[RequireComponent(typeof(Collider))]
public class FormalCrateDoorTrigger : MonoBehaviour
{
    [SerializeField] private FormalDoor door;
    private bool preloadRouteSuccessor;
    private bool completed;

    /// <summary>由场景自有的实体出口绑定在运行时设置，不改动 Prefab 资源。</summary>
    public void SetPreloadRouteSuccessor(bool enabled)
    {
        preloadRouteSuccessor = enabled;
    }

    void Awake()
    {
        Collider trigger = GetComponent<Collider>();
        trigger.isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        TryOpenFromCollider(other);
    }

    void OnTriggerStay(Collider other)
    {
        if (!completed)
            TryOpenFromCollider(other);
    }

    void TryOpenFromCollider(Collider other)
    {
        if (completed)
            return;

        FormalPushableCrate crate = other.GetComponentInParent<FormalPushableCrate>();
        if (crate == null)
            return;

        completed = true;
        if (door != null)
            door.OpenPermanently();
        else
            Debug.LogError("[FormalCrateDoorTrigger] Door reference is missing on " + name);

        FormalGameFlowController flow = FindObjectOfType<FormalGameFlowController>();
        if (flow != null)
        {
            if (preloadRouteSuccessor)
            {
                Debug.Log(
                    $"[PhysicalDoorTransition] crate-exit trigger='{name}' scene='{gameObject.scene.name}' mode=preload.",
                    this);
                flow.PreloadRouteSuccessor(this, openTransitionDoor: true);
            }
            else
            {
                flow.RequestRouteAdvance(this);
            }
        }
        else
            Debug.LogError("[FormalCrateDoorTrigger] FormalGameFlowController not found.");
    }
}
