using UnityEngine;

[RequireComponent(typeof(FormalPlayerActor))]
public class FormalHumanCratePush : MonoBehaviour
{
    [SerializeField] private float range = 1.5f;
    [SerializeField] private float force = 18f;

    private FormalPlayerActor player;

    void Awake()
    {
        player = GetComponent<FormalPlayerActor>();
    }

    void FixedUpdate()
    {
        if (player == null || player.Role != FormalPlayerActor.ActorRole.Human || !Input.GetKey(KeyCode.F))
            return;

        Collider[] hits = Physics.OverlapSphere(transform.position + transform.forward * 0.8f, range);
        foreach (Collider hit in hits)
        {
            FormalPushableCrate crate = hit.GetComponentInParent<FormalPushableCrate>();
            if (crate == null)
                continue;

            Rigidbody body = crate.GetComponent<Rigidbody>();
            if (body == null)
                continue;

            Vector3 direction = crate.transform.position - transform.position;
            direction.y = 0f;
            if (direction.sqrMagnitude > 0.01f)
                body.AddForce(direction.normalized * force, ForceMode.Force);
            return;
        }
    }
}
