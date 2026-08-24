using UnityEngine;

/// <summary>
/// 提示触发区：玩家进入本物体的 Trigger 碰撞体时，可以
///   A) 让摄像机平滑聚焦到指定的机关/道具对象(focusTarget)，停留后飞回当前角色；
///   B) 在屏幕上弹一条文字提示（走 FormalHUDController）。
/// 两者可以只开一个，也可以同时开。
///
/// 用法：给一个空物体加 BoxCollider 勾选 Is Trigger，挂本脚本。
///   - 只要文字：hintMessage 填内容，focusTarget 留空。
///   - 只要镜头：focusTarget 拖机关，hintMessage 留空。
/// 兼容 Unity 2022.3（不使用 async/await）。
/// </summary>
[RequireComponent(typeof(Collider))]
public class HintTrigger : MonoBehaviour
{
    [Header("镜头提示（可选）")]
    [Tooltip("镜头要聚焦/指向的机关或道具对象。留空 = 不推镜头")]
    public Transform focusTarget;

    [Tooltip("在提示点停留的时间（秒）。<0 表示用相机的默认停留时间")]
    public float holdTime = -1f;

    [Header("文字提示（可选）")]
    [TextArea(1, 4)]
    [Tooltip("要弹在屏幕上的提示文字。留空 = 不弹字")]
    public string hintMessage = "";

    [Tooltip("文字停留时长（秒）。<=0 表示用 HUD 的默认时长")]
    public float hintDuration = -1f;

    [Header("通用参数")]
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

        bool didSomething = false;

        if (!string.IsNullOrEmpty(hintMessage))
            didSomething |= TryShowText();

        if (focusTarget != null)
            didSomething |= TryPlayCamera();

        if (!didSomething && focusTarget == null && string.IsNullOrEmpty(hintMessage))
        {
            Debug.LogWarning("[HintTrigger] focusTarget 和 hintMessage 都为空，这个触发区什么都不会做：" + name);
            return;
        }

        if (didSomething)
            used = true;
    }

    private bool TryShowText()
    {
        var hud = FormalHUDController.Instance;
        if (hud == null)
        {
            Debug.LogWarning("[HintTrigger] 场景里找不到 FormalHUDController，文字提示无法显示：" + name);
            return false;
        }

        if (hintDuration > 0f)
            hud.ShowHint(hintMessage, hintDuration);
        else
            hud.ShowHint(hintMessage);

        return true;
    }

    private bool TryPlayCamera()
    {
        if (cam == null)
        {
            cam = PlayerManager.Instance != null ? PlayerManager.Instance.CameraFollow : null;
            if (cam == null) cam = FindObjectOfType<CameraFollow>();
        }

        if (cam == null)
        {
            Debug.LogWarning("[HintTrigger] 场景中找不到 CameraFollow。");
            return false;
        }

        if (holdTime >= 0f)
            cam.PlayHint(focusTarget, holdTime);
        else
            cam.PlayHint(focusTarget);

        return true;
    }
}
