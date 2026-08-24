using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 「走过这道门就进下一关」的触发器。
///
/// 摆放位置：出口门**后面**（关卡纵深那一侧）紧挨着门的一小块区域。
/// 玩家穿过门走进来 -> RequestRouteAdvance()，下一关开始加载。
///
/// 配上 requiredOpenDoor 之后，门没开时走进来不会有任何反应，
/// 所以「先拿钥匙开门，再走过去」这个顺序是强制的。
///
/// Collider 记得勾 Is Trigger。
/// </summary>
[RequireComponent(typeof(Collider))]
[AddComponentMenu("MoMing/Formal Route Advance Trigger")]
public class FormalRouteAdvanceTrigger : MonoBehaviour
{
    [Header("谁能触发")]
    [SerializeField] private FormalTriggerRequirement requirement = FormalTriggerRequirement.EitherPlayer;

    [Header("前置条件")]
    [Tooltip("这扇门必须是打开状态才算数。留空 = 不检查门")]
    [SerializeField] private FormalDoor requiredOpenDoor;
    [Tooltip("门不在本场景里的时候用（过关门是摆在 SharedArt 场景里的）。\n" +
             "填名字片段，比如 ToLevel02。填了但运行时找不到门，会当成门没开，不放行。")]
    [SerializeField] private string requiredOpenDoorNameToken;
    [Tooltip("这些机关必须全部完成。留空 = 不检查")]
    [SerializeField] private FormalMechanismState[] prerequisites;

    [Header("调试")]
    [SerializeField] private bool logWhenFired;

    private readonly HashSet<Object> occupants = new HashSet<Object>();
    private bool fired;
    private FormalDoor resolvedRequiredDoor;

    /// <summary>没配门 = 不检查；配了门但找不到 = 当成没开，宁可卡住也别放人过去。</summary>
    bool RequiredDoorOpen
    {
        get
        {
            if (requiredOpenDoor == null && string.IsNullOrEmpty(requiredOpenDoorNameToken))
                return true;

            if (requiredOpenDoor != null)
                return requiredOpenDoor.IsOpen;

            if (resolvedRequiredDoor == null)
                resolvedRequiredDoor = FormalDoor.FindByNameToken(requiredOpenDoorNameToken);

            return resolvedRequiredDoor != null && resolvedRequiredDoor.IsOpen;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (fired)
            return;

        Object occupant = FormalTriggerEligibility.ResolveOccupant(other, requirement);
        if (occupant == null || !occupants.Add(occupant))
            return;

        TryAdvance();
    }

    void OnTriggerExit(Collider other)
    {
        Object occupant = FormalTriggerEligibility.ResolveOccupant(other, requirement);
        if (occupant != null)
            occupants.Remove(occupant);
    }

    void TryAdvance()
    {
        if (fired)
            return;

        if (!RequiredDoorOpen)
            return;

        if (prerequisites != null)
        {
            foreach (FormalMechanismState prerequisite in prerequisites)
                if (prerequisite == null || !prerequisite.IsComplete)
                    return;
        }

        // BothPlayers 时要等两个都进来
        if (requirement == FormalTriggerRequirement.BothPlayers && !HasBothPlayers())
            return;

        FormalGameFlowController flow = FindObjectOfType<FormalGameFlowController>();
        if (flow == null)
            return;

        if (flow.CurrentLevelScene != gameObject.scene.name)
            return;

        fired = true;
        flow.RequestRouteAdvance();

        if (logWhenFired)
            Debug.Log($"[FormalRouteAdvanceTrigger] {gameObject.scene.name} 出口触发，加载下一关。", this);
    }

    bool HasBothPlayers()
    {
        bool hasHuman = false;
        bool hasDog = false;
        foreach (Object occupant in occupants)
        {
            FormalPlayerActor player = occupant as FormalPlayerActor;
            if (player == null)
                continue;

            hasHuman |= player.Role == FormalPlayerActor.ActorRole.Human;
            hasDog |= player.Role == FormalPlayerActor.ActorRole.Dog;
        }

        return hasHuman && hasDog;
    }

    void LateUpdate()
    {
        occupants.RemoveWhere(occupant => occupant == null);
    }

    void OnDrawGizmos()
    {
        Collider col = GetComponent<Collider>();
        if (col == null)
            return;

        Gizmos.color = new Color(0.95f, 0.8f, 0.3f, 0.25f);
        Gizmos.DrawCube(col.bounds.center, col.bounds.size);
        Gizmos.color = new Color(0.95f, 0.8f, 0.3f, 0.9f);
        Gizmos.DrawWireCube(col.bounds.center, col.bounds.size);
    }
}
