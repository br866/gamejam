using UnityEngine;

/// <summary>
/// 踏板被踩下的下沉反馈动画。
///
/// 挂在踏板根节点上（和 FormalActuatorTrigger 同一个物体）。踏板一旦触发，
/// 把踏板面（子物体那块板）平滑往下压一段，玩家一眼就知道"我踩到了、生效了"。
/// 关卡重置、踏板复位之后会再平滑抬回去。
///
/// 只动子物体的视觉，不动根节点上的触发 Collider ——
/// 碰撞盒跟着一起沉的话，站在上面的角色会被判定成离开触发区，机关会抖。
///
/// Press Target 留空会自动找第一个带 Renderer 的子物体，
/// 所以直接加组件就能用，不用手动拖引用。
/// </summary>
[RequireComponent(typeof(FormalActuatorTrigger))]
[AddComponentMenu("MoMing/Formal Pedal Press")]
public class FormalPedalPress : MonoBehaviour
{
    [Tooltip("看哪个触发器的状态。留空 = 用本物体上的 FormalActuatorTrigger")]
    [SerializeField] private FormalActuatorTrigger trigger;

    [Tooltip("往下压的是哪个物体。留空 = 自动找第一个带 Renderer 的子物体")]
    [SerializeField] private Transform pressTarget;

    [Tooltip("垂直往下压多深（米）。始终是世界方向的「下」，\n" +
             "旋转过、缩放过的踏板也是同样的观感")]
    [SerializeField] private float pressDepth = 0.1f;

    [Tooltip("压下去用多久（秒）。快一点更有「咔哒」的手感")]
    [SerializeField] private float pressDuration = 0.18f;

    [Tooltip("复位抬起来用多久（秒）。比压下去慢一点更自然")]
    [SerializeField] private float releaseDuration = 0.35f;

    private Vector3 restLocalPosition;

    private float progress;   // 0 = 原位，1 = 压到底
    private bool ready;

    void Awake()
    {
        Resolve();
    }

    void Resolve()
    {
        if (trigger == null)
            trigger = GetComponent<FormalActuatorTrigger>();

        if (pressTarget == null)
            pressTarget = FindVisualChild();

        if (trigger == null || pressTarget == null)
        {
            Debug.LogWarning("[FormalPedalPress] " + name +
                             " 找不到触发器或者可以往下压的子物体，这块踏板不做下沉动画。", this);
            enabled = false;
            return;
        }

        restLocalPosition = pressTarget.localPosition;
        ready = true;
    }

    /// <summary>
    /// 把「世界里往下 pressDepth 米」换算成父节点局部空间的位移。
    ///
    /// 不能直接用局部 -Y：踏板模型是 Z-up 导出的，第一关和第二关的实例整个绕 X 轴转了 -90°
    /// 才摆正，那边的局部 -Y 指的是世界的水平方向，照着压会横着滑。
    /// InverseTransformVector 会把旋转和缩放一起吃掉，所以不管这块踏板被怎么转怎么缩放，
    /// 结果永远是「垂直往下 pressDepth 米」。
    /// </summary>
    Vector3 LocalPressOffset()
    {
        Vector3 worldOffset = Vector3.down * pressDepth;
        Transform parent = pressTarget.parent;
        return parent != null ? parent.InverseTransformVector(worldOffset) : worldOffset;
    }

    /// <summary>第一个带 Renderer 的子物体就是那块板；根节点上只有触发 Collider，不能动。</summary>
    Transform FindVisualChild()
    {
        foreach (Transform child in transform)
            if (child.GetComponentInChildren<Renderer>() != null)
                return child;

        return null;
    }

    void Update()
    {
        if (!ready)
            return;

        bool pressed = trigger.IsComplete;
        float duration = pressed ? pressDuration : releaseDuration;

        float target = pressed ? 1f : 0f;
        progress = duration <= 0f
            ? target
            : Mathf.MoveTowards(progress, target, Time.deltaTime / duration);

        // smoothstep：两头慢中间快，比线性像机械件
        float eased = progress * progress * (3f - 2f * progress);
        pressTarget.localPosition = restLocalPosition + LocalPressOffset() * eased;
    }

    /// <summary>关卡重置时把板子直接摆回原位，不播抬起动画。</summary>
    public void SnapToRest()
    {
        if (!ready)
            return;

        progress = 0f;
        pressTarget.localPosition = restLocalPosition;
    }

    void OnDrawGizmosSelected()
    {
        Transform target = pressTarget != null ? pressTarget : FindVisualChild();
        if (target == null)
            return;

        Gizmos.color = new Color(0.4f, 0.9f, 0.5f, 0.9f);
        Vector3 from = target.position;
        Vector3 to = from + Vector3.down * pressDepth;
        Gizmos.DrawLine(from, to);
        Gizmos.DrawWireCube(to, new Vector3(0.3f, 0.01f, 0.3f));
    }
}
