using System.Collections.Generic;
using UnityEngine;

public class FormalCheckpoint : MonoBehaviour, IFormalLevelPermanentState
{
    [SerializeField] private FormalLevelController level;
    [SerializeField] private string owningLevelScene;
    [SerializeField] private Transform humanRespawnAnchor;
    [SerializeField] private Transform dogRespawnAnchor;
    [SerializeField] private FormalMechanismState[] prerequisites;
    [SerializeField] private bool successorRegistrationPoint;
    [Tooltip("过关存档点：踩到时把没跟上来的那一个直接拽到门口。\n" +
             "默认关闭——现在靠新关卡入口的 FormalLevelEntrySeal 来要求玩家自己把同伴带进来，" +
             "自动传送会让那个设计失去意义。只在某一关想放宽时才勾。")]
    [SerializeField] private bool bringPartnerAlong = false;
    [Tooltip("踩到这个存档点时弹一次“存档地毯”介绍图。\n" +
             "图配在 FormalPersistent 的 FormalUI / Formal Tutorial Popup 的 Checkpoint Pages 上，" +
             "看过一次就永久不再弹。只在第二关那块地毯上勾。")]
    [SerializeField] private bool showSaveAreaTutorial = false;
    [Tooltip("踩到这个存档点时弹一次\u201c本关介绍\u201d图（FormalTutorialPopup 的 Level Intro Pages）。\n" +
             "和 4.5 关入口封关区是同一份、只弹一次，谁先碰到算谁的。只在 4.5 关那个存档点勾。")]
    [SerializeField] private bool showLevelIntroTutorial = false;

    private static readonly HashSet<FormalCheckpoint> RegisteredCheckpoints =
        new HashSet<FormalCheckpoint>();
    private bool registeredWithOwningLevel;

    public bool IsComplete { get; private set; }
    public string OwningLevelScene => owningLevelScene;
    public bool IsRegisteredWithOwningLevel => registeredWithOwningLevel;

    void Awake()
    {
        if (!string.IsNullOrEmpty(owningLevelScene))
            RegisterWithOwningLevel();
    }

    void Start()
    {
        if (!registeredWithOwningLevel)
            RegisterWithOwningLevel();
    }

    void OnDestroy()
    {
        RegisteredCheckpoints.Remove(this);
        registeredWithOwningLevel = false;
    }

    /// <summary>Returns true only for this checkpoint's first registration in its loaded level.</summary>
    public bool RegisterWithOwningLevel()
    {
        if (registeredWithOwningLevel)
            return false;

        if (string.IsNullOrEmpty(owningLevelScene))
        {
            Debug.LogError($"[FormalCheckpoint] {name} has no owning level scene configured.", this);
            return false;
        }

        if (owningLevelScene != gameObject.scene.name)
        {
            Debug.LogError(
                $"[FormalCheckpoint] {name} belongs to '{owningLevelScene}' but is placed in '{gameObject.scene.name}'.",
                this);
            return false;
        }

        if (!RegisteredCheckpoints.Add(this))
            return false;

        registeredWithOwningLevel = true;
        return true;
    }

    public void ActivateCheckpoint()
    {
        if (level == null)
            level = FormalLevelActors.FindLevelController(gameObject.scene);

        if (level == null || !PrerequisitesComplete())
            return;

        IsComplete = true;
        // 传 null 时 FormalLevelController 会退回关卡自带的复活锚点
        level.SetCheckpoint(humanRespawnAnchor, dogRespawnAnchor);
    }

    void OnTriggerEnter(Collider other)
    {
        if (IsComplete || !FormalLevelActors.IsPlayer(other) || !PrerequisitesComplete())
            return;

        FormalPlayerActor trigger = FormalLevelActors.ResolvePlayer(other);

        ActivateCheckpoint();

        // 存档地毯介绍：只在勾了的那个存档点弹，而且全流程只弹一次
        if (IsComplete && showSaveAreaTutorial && FormalTutorialPopup.Instance != null)
            FormalTutorialPopup.Instance.ShowCheckpointTutorial();

        // 本关介绍（4.5 关长廊）：踩到这个存档点也算触发，和入口区共用同一个"只弹一次"
        if (showLevelIntroTutorial)
        {
            FormalTutorialPopup.Trace("存档点被踩到（" + gameObject.scene.name + "），IsComplete=" + IsComplete);
            if (IsComplete && FormalTutorialPopup.Instance != null)
                FormalTutorialPopup.Instance.ShowLevelIntroTutorial();
        }

        FormalGameFlowController flow = FindObjectOfType<FormalGameFlowController>();
        if (flow == null)
            return;

        flow.NotifyCheckpointActivated(gameObject.scene.name);

        if (!successorRegistrationPoint)
            return;

        // 过关点：把落在上一关的那一个一起拽进来。
        // 不然它留在旧关卡里，旧场景就没法卸载，玩家会看到上一关的东西穿帮。
        if (bringPartnerAlong)
            BringPartnerAlong(trigger);

        flow.NotifySuccessorCheckpointActivated(gameObject.scene.name);
    }

    /// <summary>把没触发存档点的那个角色传送到它自己的锚点上。</summary>
    void BringPartnerAlong(FormalPlayerActor trigger)
    {
        FormalPlayerActors actors = FormalPlayerActors.Instance;
        if (actors == null || trigger == null)
            return;

        bool triggeredByHuman = trigger == actors.Human;
        FormalPlayerActor partner = triggeredByHuman ? actors.Dog : actors.Human;
        Transform anchor = triggeredByHuman ? dogRespawnAnchor : humanRespawnAnchor;

        if (partner == null || anchor == null)
            return;

        // 用脚底 pivot 约定，和 FormalLevelController.MovePlayer 保持一致
        partner.SetPositionAndRotation(anchor.position - partner.MoverAttachOffset, anchor.rotation);
    }

    bool PrerequisitesComplete()
    {
        if (prerequisites == null)
            return true;

        foreach (FormalMechanismState prerequisite in prerequisites)
        {
            if (prerequisite == null || !prerequisite.IsComplete)
                return false;
        }

        return true;
    }
}
