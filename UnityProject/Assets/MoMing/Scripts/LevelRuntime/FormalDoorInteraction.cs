using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 门前的交互区：站进来按 E 开门。
///
/// 配上 requiredKey 之后，没拿钥匙时按 E 只会提示"锁着"，
/// 所以"拿钥匙 → 走到门口 → 按 E"这个顺序是强制的。
///
/// 摆放位置：门前面（玩家会走到的那一侧）一小块区域。
/// Collider 记得勾 Is Trigger。
/// </summary>
[RequireComponent(typeof(Collider))]
[AddComponentMenu("MoMing/Formal Door Interaction")]
public class FormalDoorInteraction : MonoBehaviour
{
    [Header("开哪扇门")]
    [SerializeField] private FormalDoor door;
    [Tooltip("门不在本场景里的时候用（过关门是摆在 SharedArt 场景里的，Inspector 拖不过来）。\n" +
             "填名字片段，比如 ToLevel02，运行时会在所有已加载场景里找。\n" +
             "上面的 Door 拖了东西就以拖的为准。")]
    [SerializeField] private string doorNameToken;

    [Header("前置条件（比如踏板）")]
    [Tooltip("这些机关必须先完成，门才认钥匙。\n" +
             "可以拖 FormalActuatorTrigger（踏板）、FormalMechanismState、FormalDoor 等任何带 IsComplete 的组件。")]
    [SerializeField] private MonoBehaviour[] prerequisites;

    [Header("需要的钥匙")]
    [Tooltip("留空 = 不需要钥匙，站进来按 E 就能开")]
    [SerializeField] private FormalHumanKey requiredKey;

    [Header("交互")]
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [Tooltip("默认只有人形角色能开门。小狗开不了门。")]
    [SerializeField] private FormalTriggerRequirement requirement = FormalTriggerRequirement.HumanOnly;
    [Tooltip("开过之后就不再提示，也不能再按")]
    [SerializeField] private bool onceOnly = true;

    [Header("提示文字（留空=不提示）")]
    [SerializeField] private string promptReady = "按 E 开门";
    [SerializeField] private string promptLocked = "门锁着，得先找到钥匙";
    [SerializeField] private string promptPrerequisite = "门纹丝不动，好像还缺点什么";
    [SerializeField] private string promptOpened = "门开了";

    private readonly HashSet<Object> occupants = new HashSet<Object>();
    private bool opened;
    private bool lastCanOpen;
    private bool promptShown;
    private FormalDoor resolvedDoor;

    /// <summary>Inspector 拖的门优先；没拖就按名字片段跨场景找一次并记住。</summary>
    FormalDoor ResolvedDoor
    {
        get
        {
            if (door != null)
                return door;

            if (resolvedDoor == null)
                resolvedDoor = FormalDoor.FindByNameToken(doorNameToken);

            return resolvedDoor;
        }
    }

    bool HasKey => requiredKey == null || requiredKey.IsCollected;
    bool CanOpen => PrerequisitesComplete && HasKey;
    bool PlayerInside => occupants.Count > 0;

    void OnTriggerEnter(Collider other)
    {
        Object occupant = FormalTriggerEligibility.ResolveOccupant(other, requirement);
        if (occupant == null || !occupants.Add(occupant))
            return;

        promptShown = false;
        RefreshPrompt();
    }

    void OnTriggerExit(Collider other)
    {
        Object occupant = FormalTriggerEligibility.ResolveOccupant(other, requirement);
        if (occupant == null || !occupants.Remove(occupant))
            return;

        if (occupants.Count == 0)
            promptShown = false;
    }

    bool PrerequisitesComplete
    {
        get
        {
            if (prerequisites == null)
                return true;

            foreach (MonoBehaviour behaviour in prerequisites)
            {
                IFormalLevelPermanentState state = behaviour as IFormalLevelPermanentState;
                if (state == null || !state.IsComplete)
                    return false;
            }

            return true;
        }
    }

    void Update()
    {
        occupants.RemoveWhere(occupant => occupant == null);

        if (!PlayerInside || (onceOnly && opened))
            return;

        // 站在门口的时候条件变了（拿到钥匙 / 狗踩上踏板），提示要跟着换一次
        if (CanOpen != lastCanOpen)
        {
            promptShown = false;
            RefreshPrompt();
        }

        if (!Input.GetKeyDown(interactKey))
            return;

        if (!PrerequisitesComplete)
        {
            ShowHint(promptPrerequisite);
            return;
        }

        if (!HasKey)
        {
            ShowHint(promptLocked);
            return;
        }

        FormalDoor target = ResolvedDoor;
        if (target == null)
        {
            Debug.LogWarning("[FormalDoorInteraction] 没有连门，按 E 没用。", this);
            return;
        }

        target.OpenPermanently();
        opened = true;
        ShowHint(promptOpened);
    }

    void RefreshPrompt()
    {
        lastCanOpen = CanOpen;

        if (promptShown || (onceOnly && opened))
            return;

        promptShown = true;
        ShowHint(!PrerequisitesComplete ? promptPrerequisite
               : !HasKey ? promptLocked
               : promptReady);
    }

    void ShowHint(string message)
    {
        if (string.IsNullOrEmpty(message))
            return;

        if (FormalHUDController.Instance != null)
            FormalHUDController.Instance.ShowHint(message);
    }

    void OnDrawGizmos()
    {
        Collider col = GetComponent<Collider>();
        if (col == null)
            return;

        Gizmos.color = new Color(0.4f, 0.7f, 1f, 0.25f);
        Gizmos.DrawCube(col.bounds.center, col.bounds.size);
        Gizmos.color = new Color(0.4f, 0.7f, 1f, 0.9f);
        Gizmos.DrawWireCube(col.bounds.center, col.bounds.size);
    }
}
