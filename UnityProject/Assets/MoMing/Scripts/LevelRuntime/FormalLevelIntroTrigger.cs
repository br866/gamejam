using UnityEngine;

/// <summary>
/// 本关介绍触发区。人走进（或者直接被传送到）这块范围里，就弹一次本关介绍图。
///
/// 摆放位置：关卡出生点／门口那一带。第五关放在 L05 的复活锚点上。
/// 为什么不用 Collider：GM 传送是把人瞬移过来的，瞬移进触发器有时候不会走
/// OnTriggerEnter；这里直接每帧量距离，传送、走进来、复活都吃得到。
///
/// 图配在 FormalPersistent 的 FormalUI / Formal Tutorial Popup 的 Level Intro Pages 上，
/// 和入口封关区、存档点共用同一个「只弹一次」，谁先碰到算谁的。
/// </summary>
[AddComponentMenu("MoMing/Formal Level Intro Trigger")]
public class FormalLevelIntroTrigger : MonoBehaviour
{
    [Tooltip("离这个点多近算进来了（米）")]
    [SerializeField] private float radius = 6f;

    [Tooltip("勾上=人和狗都进范围才弹；取消=人进来就弹")]
    [SerializeField] private bool requireBothActors = false;

    [Tooltip("关卡刚加载完的这几秒内不判定，等人被摆到位再说")]
    [SerializeField] private float armDelay = 0.3f;

    [Tooltip("弹过一次之后这个触发区就不再做任何事")]
    [SerializeField] private bool logWhenTriggered = true;

    private float armTime;
    private bool done;

    void OnEnable()
    {
        armTime = Time.unscaledTime + Mathf.Max(0f, armDelay);
        done = false;
    }

    void Update()
    {
        if (done || Time.unscaledTime < armTime)
            return;

        FormalPlayerActors actors = FormalPlayerActors.Instance;
        if (actors == null || actors.Human == null)
            return;

        if (!InRange(actors.Human.transform))
        {
            if (Time.frameCount % 120 == 0)
                FormalTutorialPopup.Trace("出生点触发区：人还没进范围，人在 " +
                    actors.Human.transform.position.ToString("F2") + "，触发区在 " +
                    transform.position.ToString("F2") + "，半径 " + radius);
            return;
        }

        if (requireBothActors && (actors.Dog == null || !InRange(actors.Dog.transform)))
            return;

        done = true;

        if (FormalTutorialPopup.Instance == null)
        {
            Debug.LogWarning("[FormalLevelIntroTrigger] 找不到 FormalTutorialPopup，弹不了。", this);
            return;
        }

        FormalTutorialPopup.Trace("出生点触发区命中（" + gameObject.scene.name + "）");

        if (logWhenTriggered)
            Debug.Log("[FormalLevelIntroTrigger] " + gameObject.scene.name + " 出生点触发区已命中。", this);

        FormalTutorialPopup.Instance.ShowLevelIntroTutorial();
    }

    bool InRange(Transform actor)
    {
        if (actor == null)
            return false;

        // 只量水平距离，站在平台上下一层不算
        Vector3 delta = actor.position - transform.position;
        if (Mathf.Abs(delta.y) > Mathf.Max(3f, radius))
            return false;

        delta.y = 0f;
        return delta.sqrMagnitude <= radius * radius;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.95f, 0.75f, 0.25f, 0.25f);
        Gizmos.DrawSphere(transform.position, radius);
        Gizmos.color = new Color(0.95f, 0.75f, 0.25f, 0.9f);
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
