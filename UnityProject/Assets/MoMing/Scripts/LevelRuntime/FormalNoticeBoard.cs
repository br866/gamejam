using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 场景里的公告牌：走近弹一句「按 F 可以阅读」，按 F 弹出整页公告图。
///
/// 人和狗看到的内容不一样：Human Pages 给人，Dog Pages 给狗；各自都能配多张。
/// 弹图复用 FormalPersistent 场景里那个 FormalTutorialPopup 面板（同一套翻页/关闭/暂停逻辑），
/// 但不写 PlayerPrefs，所以公告牌可以反复阅读，不是只能看一次。
///
/// 按键不是本脚本自己抢的：F 由 FormalPlayerControl 统一分发，
/// 挂着箱子的时候 F 仍然是「松开箱子」，只有空手站在牌子前才算阅读。
///
/// 摆放位置：牌子模型下面挂一个空物体，或者直接挂在牌子根节点上，
/// readRadius 调到玩家会走到的范围（默认 3 米）。
/// </summary>
[AddComponentMenu("MoMing/Formal Notice Board")]
public class FormalNoticeBoard : MonoBehaviour
{
    [Header("阅读范围")]
    [Tooltip("离牌子多近才能读（米）。只量水平距离")]
    [SerializeField] private float readRadius = 3f;

    [Tooltip("上下差超过这个高度就不算站在牌子前，避免楼上楼下互相触发")]
    [SerializeField] private float heightTolerance = 3f;

    [Header("公告内容")]
    [Tooltip("人读到的页面，按数组顺序翻阅。留空 = 人读不了。")]
    [SerializeField] private Sprite[] humanPages;

    [Tooltip("狗读到的页面，按数组顺序翻阅。留空 = 狗读不了。")]
    [SerializeField] private Sprite[] dogPages;

    [Header("引导提示")]
    [Tooltip("走进范围时弹在屏幕上的提示。留空 = 不提示")]
    [TextArea(1, 3)]
    [SerializeField] private string prompt = "这边有一个公告板，按 F 可以阅读";

    [Tooltip("提示停留时长（秒）。<=0 用 HUD 的默认时长")]
    [SerializeField] private float promptDuration = 3f;

    [Tooltip("一直站在范围里的话，隔多久再提示一次（秒）")]
    [SerializeField] private float promptRepeatInterval = 10f;

    [Tooltip("读过一次之后就不再弹引导提示")]
    [SerializeField] private bool stopPromptingAfterRead = true;

    private static readonly List<FormalNoticeBoard> boards = new List<FormalNoticeBoard>();

    private bool someoneInside;
    private bool hasBeenRead;
    private float nextPromptTime;

    void OnEnable()
    {
        boards.Add(this);
        someoneInside = false;
        nextPromptTime = 0f;
    }

    void OnDisable()
    {
        boards.Remove(this);
    }

    void Update()
    {
        if (FormalTutorialPopup.IsShowing)
        {
            someoneInside = false;
            return;
        }

        bool inside = InRange(ActorOf(FormalPlayerActor.ActorRole.Human))
                   || InRange(ActorOf(FormalPlayerActor.ActorRole.Dog));

        if (inside && ShouldPrompt())
        {
            ShowPrompt();
            nextPromptTime = Time.unscaledTime + Mathf.Max(0.5f, promptRepeatInterval);
        }

        someoneInside = inside;
    }

    bool ShouldPrompt()
    {
        if (string.IsNullOrEmpty(prompt))
            return false;

        if (stopPromptingAfterRead && hasBeenRead)
            return false;

        // 刚走进来立刻提示一次；一直赖在范围里就按间隔重复。
        return !someoneInside || Time.unscaledTime >= nextPromptTime;
    }

    void ShowPrompt()
    {
        FormalHUDController hud = FormalHUDController.Instance;
        if (hud == null)
        {
            Debug.LogWarning("[FormalNoticeBoard] 场景里找不到 FormalHUDController，提示弹不出来：" + name, this);
            return;
        }

        if (promptDuration > 0f)
            hud.ShowHint(prompt, promptDuration);
        else
            hud.ShowHint(prompt);
    }

    /// <summary>
    /// FormalPlayerControl 收到 F 时转发过来。返回 true 表示这次 F 被公告牌吃掉了。
    /// 附近有多块牌子时读最近的那一块。
    /// </summary>
    public static bool TryRead(FormalPlayerActor actor)
    {
        if (actor == null || actor.IsExecutionLocked || FormalTutorialPopup.IsShowing)
            return false;

        FormalNoticeBoard nearest = null;
        float nearestDistance = float.MaxValue;

        for (int i = 0; i < boards.Count; i++)
        {
            FormalNoticeBoard board = boards[i];
            if (board == null || !board.isActiveAndEnabled || !board.InRange(actor))
                continue;

            float distance = board.FlatDistanceTo(actor.transform.position);
            if (distance >= nearestDistance)
                continue;

            nearest = board;
            nearestDistance = distance;
        }

        return nearest != null && nearest.Read(actor);
    }

    bool Read(FormalPlayerActor actor)
    {
        bool isDog = actor.Role == FormalPlayerActor.ActorRole.Dog;
        Sprite[] pages = PagesFor(isDog);
        if (pages == null)
        {
            Debug.LogWarning("[FormalNoticeBoard] " + name + " 没配 " +
                             (isDog ? "Dog Pages" : "Human Pages") +
                             "，这次读不出东西。", this);
            return false;
        }

        FormalTutorialPopup popup = FormalTutorialPopup.Instance;
        if (popup == null)
        {
            Debug.LogWarning("[FormalNoticeBoard] 找不到 FormalTutorialPopup（在 FormalPersistent 场景里），公告弹不出来。", this);
            return false;
        }

        // prefKey 传空：公告牌可以反复读，不写“看过了”。
        if (!popup.ShowOnce(pages, null))
            return false;

        hasBeenRead = true;
        return true;
    }

    Sprite[] PagesFor(bool isDog)
    {
        Sprite[] configuredPages = isDog ? dogPages : humanPages;
        if (configuredPages != null && configuredPages.Length > 0)
        {
            int validCount = 0;
            for (int i = 0; i < configuredPages.Length; i++)
            {
                if (configuredPages[i] != null)
                    validCount++;
            }

            if (validCount == configuredPages.Length)
                return configuredPages;

            if (validCount > 0)
            {
                Sprite[] validPages = new Sprite[validCount];
                int index = 0;
                for (int i = 0; i < configuredPages.Length; i++)
                {
                    if (configuredPages[i] != null)
                        validPages[index++] = configuredPages[i];
                }

                return validPages;
            }
        }

        return null;
    }

    static FormalPlayerActor ActorOf(FormalPlayerActor.ActorRole role)
    {
        FormalPlayerActors actors = FormalPlayerActors.Instance;
        if (actors == null)
            return null;

        return role == FormalPlayerActor.ActorRole.Dog ? actors.Dog : actors.Human;
    }

    bool InRange(FormalPlayerActor actor)
    {
        if (actor == null || !actor.gameObject.activeInHierarchy)
            return false;

        Vector3 delta = actor.transform.position - transform.position;
        if (Mathf.Abs(delta.y) > Mathf.Max(heightTolerance, readRadius))
            return false;

        delta.y = 0f;
        return delta.sqrMagnitude <= readRadius * readRadius;
    }

    float FlatDistanceTo(Vector3 worldPosition)
    {
        Vector3 delta = worldPosition - transform.position;
        delta.y = 0f;
        return delta.magnitude;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.3f, 0.8f, 0.95f, 0.2f);
        Gizmos.DrawSphere(transform.position, readRadius);
        Gizmos.color = new Color(0.3f, 0.8f, 0.95f, 0.9f);
        Gizmos.DrawWireSphere(transform.position, readRadius);
    }
}
