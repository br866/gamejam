using UnityEngine;

[RequireComponent(typeof(PlayerController))]
public class FormalHumanCratePush : MonoBehaviour
{
    [SerializeField] private float range = 1.5f;
    [SerializeField] private float force = 18f;

    private PlayerController player;

    void Awake()
    {
        player = GetComponent<PlayerController>();
    }

    void FixedUpdate()
    {
        if (player == null || player.characterType != PlayerController.CharacterType.Human || !Input.GetKey(KeyCode.F))
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
