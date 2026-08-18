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

    // 推箱子音效：挂住时 Start，移动中循环 Loop，脱离时 Stop
    private AudioSource loopSource;
    private Vector3 lastPos;

    public bool IsAttached => isAttached;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotation;
        rb.isKinematic = true;

        loopSource = gameObject.AddComponent<AudioSource>();
        loopSource.playOnAwake = false;
        loopSource.loop = true;
        loopSource.spatialBlend = 1f;
        loopSource.rolloffMode = AudioRolloffMode.Linear;
        loopSource.maxDistance = 25f;
        loopSource.volume = 0f;
        lastPos = transform.position;
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

        SfxManager.PlayAt(Sfx.CratePushStart, transform.position);
        if (loopSource != null && loopSource.clip == null)
            loopSource.clip = SfxManager.GetClip(Sfx.CratePushLoop);
        if (loopSource != null && loopSource.clip != null)
        {
            loopSource.volume = 0f;
            loopSource.Play();
        }
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

        if (loopSource != null && loopSource.isPlaying)
            loopSource.Stop();
        SfxManager.PlayAt(Sfx.CratePushStop, transform.position);
    }

    void LateUpdate()
    {
        if (!isAttached || attachedPlayer == null) return;

        Vector3 targetPos = attachedPlayer.position + attachOffset;
        targetPos.y = transform.position.y;
        transform.position = Vector3.Lerp(transform.position, targetPos, followSpeed * Time.deltaTime);

        // 只有真的在动的时候循环声才出来，停住就淡下去
        float moved = (transform.position - lastPos).magnitude;
        lastPos = transform.position;
        if (loopSource != null)
        {
            float target = (moved > 0.002f) ? 1f : 0f;
            loopSource.volume = Mathf.MoveTowards(loopSource.volume, target, Time.deltaTime * 4f);
        }
    }
}
