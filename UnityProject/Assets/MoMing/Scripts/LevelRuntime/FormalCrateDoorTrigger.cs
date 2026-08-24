using UnityEngine;

[RequireComponent(typeof(Collider))]
public class FormalCrateDoorTrigger : MonoBehaviour
{
    [SerializeField] private FormalDoor door;
    private bool completed;

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
            flow.RequestRouteAdvance(this);
        else
            Debug.LogError("[FormalCrateDoorTrigger] FormalGameFlowController not found.");
    }
}
