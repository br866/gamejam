using UnityEngine;

public class FormalCheckpoint : MonoBehaviour, IFormalLevelPermanentState
{
    [SerializeField] private FormalLevelController level;
    [SerializeField] private Transform humanRespawnAnchor;
    [SerializeField] private Transform dogRespawnAnchor;
    [SerializeField] private FormalMechanismState[] prerequisites;
    [SerializeField] private bool successorRegistrationPoint;
    [Tooltip("过关存档点：踩到时把没跟上来的那一个直接拽到门口。\n" +
             "默认关闭——现在靠新关卡入口的 FormalLevelEntrySeal 来要求玩家自己把同伴带进来，" +
             "自动传送会让那个设计失去意义。只在某一关想放宽时才勾。")]
    [SerializeField] private bool bringPartnerAlong = false;

    public bool IsComplete { get; private set; }

    public void ActivateCheckpoint()
    {
        if (level == null)
            level = FormalLevelActors.FindLevelController(gameObject.scene);

        if (level == null || !PrerequisitesComplete())
            return;

        IsComplete = true;
        level.SetCheckpoint(humanRespawnAnchor, dogRespawnAnchor);
    }

    void OnTriggerEnter(Collider other)
    {
        if (IsComplete || !FormalLevelActors.IsPlayer(other) || !PrerequisitesComplete())
            return;

        FormalPlayerActor trigger = FormalLevelActors.ResolvePlayer(other);

        ActivateCheckpoint();

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
