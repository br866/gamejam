using UnityEngine;

/// <summary>
/// 提示触发区：玩家进入本物体的 Trigger 碰撞体时，
/// 让摄像机平滑聚焦到指定的机关/道具对象(focusTarget)，停留后飞回当前角色。
/// 用法：给一个空物体加 BoxCollider 勾选 Is Trigger，挂本脚本，把要提示的机关拖到 focusTarget。
/// 兼容 Unity 2022.3（不使用 async/await）。
/// </summary>
[RequireComponent(typeof(Collider))]
public class HintTrigger : MonoBehaviour
{
    [Header("提示目标")]
    [Tooltip("镜头要聚焦/指向的机关或道具对象")]
    public Transform focusTarget;

    [Header("参数")]
    [Tooltip("在提示点停留的时间（秒）。<0 表示用相机的默认停留时间")]
    public float holdTime = -1f;

    [Tooltip("是否只触发一次")]
    public bool playOnce = true;

    [Tooltip("触发所需的 Tag（角色请设为 Player）")]
    public string playerTag = "Player";

    private bool used = false;
    private CameraFollow cam;

    void Reset()
    {
        // 自动把碰撞体设为 Trigger，方便美术/策划直接用
        var col = GetComponent<Collider>();
        if (col != null) col.isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (playOnce && used) return;
        if (!string.IsNullOrEmpty(playerTag) && !other.CompareTag(playerTag)) return;
        if (focusTarget == null)
        {
            Debug.LogWarning("[HintTrigger] focusTarget 未设置，无法播放提示镜头：" + name);
            return;
        }

        if (cam == null)
        {
            cam = PlayerManager.Instance != null ? PlayerManager.Instance.CameraFollow : null;
            if (cam == null) cam = FindObjectOfType<CameraFollow>();
        }
        if (cam == null)
        {
            Debug.LogWarning("[HintTrigger] 场景中找不到 CameraFollow。");
            return;
        }

        if (holdTime >= 0f)
            cam.PlayHint(focusTarget, holdTime);
        else
            cam.PlayHint(focusTarget);

        used = true;
    }
}
