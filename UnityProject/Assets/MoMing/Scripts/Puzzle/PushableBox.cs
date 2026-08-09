using UnityEngine;

/// <summary>
/// 重型可推箱子：双人模式下按F挂住，挂住后箱子维持与玩家的相对位置（类似狗跟随人）。
/// 默认kinematic锁定，挂住后保持kinematic + 位置跟随。
/// </summary>
public class PushableBox : MonoBehaviour
{
    [Tooltip("箱子跟随玩家的平滑速度")]
    public float followSpeed = 10f;

    private Rigidbody rb;
    private bool isAttached = false;
    private Transform attachedPlayer;
    private Vector3 attachOffset;

    public bool IsAttached => isAttached;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotation;
        rb.isKinematic = true;
    }

    public void Attach(Transform player)
    {
        isAttached = true;
        attachedPlayer = player;
        attachOffset = transform.position - player.position;
        attachOffset.y = 0f;
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.isKinematic = true;
        }

        // 禁用玩家与箱子的碰撞，避免前进时被箱子阻挡
        var playerCol = player.GetComponent<Collider>();
        var boxCol = GetComponent<Collider>();
        if (playerCol != null && boxCol != null)
            Physics.IgnoreCollision(playerCol, boxCol, true);
    }

    public void Detach()
    {
        isAttached = false;
        if (attachedPlayer != null)
        {
            var playerCol = attachedPlayer.GetComponent<Collider>();
            var boxCol = GetComponent<Collider>();
            if (playerCol != null && boxCol != null)
                Physics.IgnoreCollision(playerCol, boxCol, false);
        }
        attachedPlayer = null;
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.isKinematic = true;
        }
    }

    void LateUpdate()
    {
        if (!isAttached || attachedPlayer == null) return;

        Vector3 targetPos = attachedPlayer.position + attachOffset;
        targetPos.y = transform.position.y;
        transform.position = Vector3.Lerp(transform.position, targetPos, followSpeed * Time.deltaTime);
    }
}
