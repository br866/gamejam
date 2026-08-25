using UnityEngine;

/// <summary>
/// 墙上/地上的贴纸：本体只是一张图，不能交互，但玩家每次靠近都会在屏幕上弹一句话。
///
/// 和公告牌（FormalNoticeBoard）的区别：
///   公告牌  = 走近提示「按 F 阅读」，按 F 才弹整页大图，一般只需要看一次；
///   贴纸    = 不用按键，走近就出文字，走远再走回来还会再出，随便看多少次。
///
/// 文字走的是 FormalHUDController 的居中提示（和第一关那些引导同一套）。
/// hintMessage 留空 = 纯装饰，只有图，不出字。
///
/// 摆放：用菜单 Tools / SuperBreadMan / 贴纸 / 把选中的图贴到当前视角中心 生成，
/// 生成出来的结构是：
///   Sticker_xxx   (本脚本，触发范围球心在这儿)
///     └ Face      (SpriteRenderer，图本身)
/// </summary>
[AddComponentMenu("MoMing/Formal Wall Sticker")]
public class FormalWallSticker : MonoBehaviour
{
    [Header("靠近时弹的文字")]
    [Tooltip("留空 = 纯装饰，只有图不出字")]
    [TextArea(1, 4)]
    [SerializeField] private string hintMessage = "";

    [Tooltip("文字停留时长（秒）。<=0 用 HUD 的默认时长")]
    [SerializeField] private float hintDuration = 3f;

    [Header("触发范围")]
    [Tooltip("离贴纸多近开始弹字（米）。只量水平距离")]
    [SerializeField] private float triggerRadius = 3.5f;

    [Tooltip("要走出 触发半径 + 这个距离 才会重新武装，防止站在边界上来回刷屏")]
    [SerializeField] private float releasePadding = 1.5f;

    [Tooltip("两次弹字之间至少隔这么久（秒），再快也不弹")]
    [SerializeField] private float minRepeatSeconds = 4f;

    [Tooltip("上下差超过这个高度就不算靠近，避免楼上楼下互相触发")]
    [SerializeField] private float heightTolerance = 3f;

    [Header("谁能触发")]
    [SerializeField] private FormalTriggerRequirement requirement = FormalTriggerRequirement.EitherPlayer;

    // armed = 现在处于「还没弹过、等人走进来」的状态
    private bool armed = true;
    private float nextAllowedTime;

    void OnEnable()
    {
        armed = true;
        nextAllowedTime = 0f;
    }

    void Update()
    {
        // 教程/公告大图正开着的时候别插嘴
        if (FormalTutorialPopup.IsShowing)
            return;

        float distance = NearestActorDistance();

        if (armed)
        {
            if (distance <= triggerRadius && Time.unscaledTime >= nextAllowedTime)
            {
                ShowHint();
                armed = false;
                nextAllowedTime = Time.unscaledTime + Mathf.Max(0f, minRepeatSeconds);
            }
            return;
        }

        // 走远了才重新武装 —— 于是「走开再回来」能再看一次，站着不动不会刷屏
        if (distance > triggerRadius + Mathf.Max(0f, releasePadding))
            armed = true;
    }

    void ShowHint()
    {
        if (string.IsNullOrEmpty(hintMessage))
            return;

        FormalHUDController hud = FormalHUDController.Instance;
        if (hud == null)
        {
            Debug.LogWarning("[FormalWallSticker] 场景里找不到 FormalHUDController，文字弹不出来：" + name, this);
            return;
        }

        if (hintDuration > 0f)
            hud.ShowHint(hintMessage, hintDuration);
        else
            hud.ShowHint(hintMessage);
    }

    /// <summary>按 requirement 挑角色，返回最近那一个的水平距离；没人符合就返回无穷大。</summary>
    float NearestActorDistance()
    {
        FormalPlayerActors actors = FormalPlayerActors.Instance;
        if (actors == null)
            return float.MaxValue;

        float best = float.MaxValue;

        if (requirement != FormalTriggerRequirement.DogOnly)
            best = Mathf.Min(best, FlatDistanceTo(actors.Human));

        if (requirement != FormalTriggerRequirement.HumanOnly)
            best = Mathf.Min(best, FlatDistanceTo(actors.Dog));

        return best;
    }

    float FlatDistanceTo(FormalPlayerActor actor)
    {
        if (actor == null || !actor.gameObject.activeInHierarchy)
            return float.MaxValue;

        Vector3 delta = actor.transform.position - transform.position;
        if (Mathf.Abs(delta.y) > Mathf.Max(heightTolerance, triggerRadius))
            return float.MaxValue;

        delta.y = 0f;
        return delta.magnitude;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.95f, 0.6f, 0.3f, 0.18f);
        Gizmos.DrawSphere(transform.position, triggerRadius);
        Gizmos.color = new Color(0.95f, 0.6f, 0.3f, 0.9f);
        Gizmos.DrawWireSphere(transform.position, triggerRadius);
        Gizmos.color = new Color(0.95f, 0.6f, 0.3f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, triggerRadius + Mathf.Max(0f, releasePadding));
    }
}
