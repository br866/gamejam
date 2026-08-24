using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 关卡入口的「封关」触发器。
///
/// 摆放位置：**新关卡自己的场景里**，罩住进门后的那一小片区域。
/// 人和狗都进到这个范围里之后 -> 关掉两关之间的门 + 卸载上一关。
///
/// 为什么需要它：过关点（FormalCheckpoint）是摆在【上一关】场景里的，
/// 触发那一刻玩家还踩在上一关的地板上，当场卸载会直接掉下去。
/// 所以「加载下一关」和「卸载上一关」必须拆成两个时机，这是后一个。
///
/// Collider 记得勾 Is Trigger。
/// </summary>
[RequireComponent(typeof(Collider))]
[AddComponentMenu("MoMing/Formal Level Entry Seal")]
public class FormalLevelEntrySeal : MonoBehaviour
{
    [Header("条件")]
    [Tooltip("勾上=人和狗都进来才封关；取消=任意一个进来就封")]
    [SerializeField] private bool requireBothActors = true;

    [Header("封关做什么")]
    [SerializeField] private bool closeTransitionDoor = true;
    [SerializeField] private bool unloadPredecessor = true;

    [Header("生效延迟")]
    [Tooltip("关卡刚加载完的这几秒内不封关。\n" +
             "防止玩家还站在两关交界处、新关卡一加载触发器就把上一关拆了。")]
    [SerializeField] private float armDelay = 1.5f;

    [Header("教程")]
    [Tooltip("人一踏进这个入口区，就弹一次本关介绍图。\n" +
             "图配在 FormalPersistent 的 FormalUI / Formal Tutorial Popup 的 Level Intro Pages 上，" +
             "看过一次就永久不再弹。只在 4.5 关入口勾。")]
    [SerializeField] private bool showLevelIntroTutorial = false;

    [Header("调试")]
    [SerializeField] private bool logWhenSealed;

    private readonly HashSet<FormalPlayerActor> inside = new HashSet<FormalPlayerActor>();
    private bool sealDone;
    private float armTime;

    void OnEnable()
    {
        armTime = Time.unscaledTime + Mathf.Max(0f, armDelay);
    }

    void OnTriggerEnter(Collider other)
    {
        FormalPlayerActor actor = FormalLevelActors.ResolvePlayer(other);
        if (actor == null)
            return;

        // 本关介绍：踏进入口区就弹，不用等封关条件（4.5 关是人先进、狗后到）
        if (showLevelIntroTutorial)
        {
            FormalTutorialPopup.Trace("入口封关区被踩到（" + gameObject.scene.name + "），popup=" +
                                      (FormalTutorialPopup.Instance != null));
            if (FormalTutorialPopup.Instance != null)
                FormalTutorialPopup.Instance.ShowLevelIntroTutorial();
        }

        if (sealDone || !inside.Add(actor))
            return;

        TrySeal();
    }

    void OnTriggerExit(Collider other)
    {
        if (sealDone)
            return;

        FormalPlayerActor actor = FormalLevelActors.ResolvePlayer(other);
        if (actor != null)
            inside.Remove(actor);
    }

    void Update()
    {
        // 延迟结束的那一刻补判一次：可能两个人在延迟期间就已经进来了
        if (!sealDone && Time.unscaledTime >= armTime && inside.Count > 0)
            TrySeal();
    }

    void TrySeal()
    {
        if (sealDone || Time.unscaledTime < armTime || !ConditionMet())
            return;

        FormalGameFlowController flow = FindObjectOfType<FormalGameFlowController>();
        if (flow == null)
            return;

        // L2 的实体过门会先预加载 L3；两人真正进入本入口区时才提交关卡切换。
        if (flow.ConfirmPreloadedPhysicalArrival(gameObject.scene.name, this))
        {
            if (!flow.SealPredecessorLevel(closeTransitionDoor, unloadPredecessor))
                return;

            sealDone = true;
            if (logWhenSealed)
                Debug.Log($"[FormalLevelEntrySeal] {gameObject.scene.name} 实体进关已确认，前关已按本关策略处理。", this);
            return;
        }

        // 触发场景必须是当前关，否则说明这个触发器所在的关卡已经不是"最新的一关"了
        if (flow.CurrentLevelScene != gameObject.scene.name)
            return;

        if (!flow.SealPredecessorLevel(closeTransitionDoor, unloadPredecessor))
            return;   // 还没轮到（比如上一关已经卸载过了），下次有人进来再试

        sealDone = true;

        if (logWhenSealed)
            Debug.Log($"[FormalLevelEntrySeal] {gameObject.scene.name} 入口已封，上一关卸载。", this);
    }

    bool ConditionMet()
    {
        inside.RemoveWhere(actor => actor == null);

        if (!requireBothActors)
            return inside.Count > 0;

        bool hasHuman = false;
        bool hasDog = false;
        foreach (FormalPlayerActor actor in inside)
        {
            hasHuman |= actor.Role == FormalPlayerActor.ActorRole.Human;
            hasDog |= actor.Role == FormalPlayerActor.ActorRole.Dog;
        }

        return hasHuman && hasDog;
    }

    void OnDrawGizmos()
    {
        Collider col = GetComponent<Collider>();
        if (col == null)
            return;

        Gizmos.color = new Color(0.3f, 0.9f, 0.6f, 0.25f);
        Gizmos.DrawCube(col.bounds.center, col.bounds.size);
        Gizmos.color = new Color(0.3f, 0.9f, 0.6f, 0.9f);
        Gizmos.DrawWireCube(col.bounds.center, col.bounds.size);
    }
}
