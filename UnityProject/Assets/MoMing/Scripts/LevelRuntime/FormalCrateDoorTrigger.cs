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
        Debug.Log("[FormalCrateDoorTrigger] Ready: " + name + " bounds=" + trigger.bounds + " door=" + (door != null ? door.name : "null"));
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log("[FormalCrateDoorTrigger] Enter: trigger=" + name + " collider=" + other.name + " root=" + other.transform.root.name);
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
        {
            Debug.Log("[FormalCrateDoorTrigger] Ignored collider: " + other.name + " has no FormalPushableCrate parent.");
            return;
        }

        Debug.Log("[FormalCrateDoorTrigger] Crate detected: " + crate.name + " position=" + crate.transform.position + " door=" + (door != null ? door.name : "null"));

        completed = true;
        if (door != null)
        {
            door.OpenPermanently();
            Debug.Log("[FormalCrateDoorTrigger] Door opened: " + door.name);
        }
        else
        {
            Debug.LogError("[FormalCrateDoorTrigger] Door reference is missing on " + name);
        }

        FormalGameFlowController flow = FindObjectOfType<FormalGameFlowController>();
        if (flow != null)
        {
            flow.NotifyLevel045DoorOpened();
            flow.OpenTransitionDoor("FormalLevel045", "FormalLevel05");
            flow.LoadSuccessor("FormalLevel05");
            Debug.Log("[FormalCrateDoorTrigger] Opened shared FormalLevel045/FormalLevel05 door and requested FormalLevel05 load.");
        }
        else
            Debug.LogError("[FormalCrateDoorTrigger] FormalGameFlowController not found.");
    }
}
