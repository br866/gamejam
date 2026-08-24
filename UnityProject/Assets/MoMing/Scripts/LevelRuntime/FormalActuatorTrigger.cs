using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
// 加上 IFormalLevelPermanentState，这样踏板可以直接当作
// FormalDoorInteraction 的前置条件被引用（它本来就有 IsComplete）
public class FormalActuatorTrigger : MonoBehaviour, IFormalLevelTemporaryState, IFormalLevelPermanentState
{
    [SerializeField] private FormalTriggerRequirement requirement = FormalTriggerRequirement.EitherPlayer;
    [SerializeField] private MonoBehaviour[] actuators;
    [SerializeField] private bool permanent = true;
    [SerializeField] private bool opensTransitionDoor;
    [SerializeField] private string successorScene;
    [Tooltip("完成后只预加载正式路线的下一关，保持角色位置并等待实体进入目标关入口确认。")]
    [SerializeField] private bool preloadRouteSuccessor;

    private readonly HashSet<Object> occupants = new HashSet<Object>();
    private bool complete;

    public bool IsComplete => complete;
    public string SuccessorScene => successorScene;

    /// <summary>供场景自有的实体门出口绑定在运行时配置，不修改源 Prefab。</summary>
    public void SetPreloadRouteSuccessor(bool enabled)
    {
        preloadRouteSuccessor = enabled;
    }

    void Awake()
    {
        FormalLevelController level = FormalLevelActors.FindLevelController(gameObject.scene);
        if (level != null)
            level.RegisterTemporaryState(this);
    }

    void OnTriggerEnter(Collider other)
    {
        Object occupant = FormalTriggerEligibility.ResolveOccupant(other, requirement);
        if (occupant == null || !occupants.Add(occupant) || complete || !RequirementSatisfied())
            return;

        CompleteTrigger();
    }

    void OnTriggerExit(Collider other)
    {
        Object occupant = FormalTriggerEligibility.ResolveOccupant(other, requirement);
        if (occupant != null)
            occupants.Remove(occupant);
    }

    void LateUpdate()
    {
        occupants.RemoveWhere(occupant => occupant == null);
        TryComplete();
    }

    public void ResetTemporaryState()
    {
        occupants.Clear();
        if (permanent || !complete)
            return;

        complete = false;
        foreach (MonoBehaviour behaviour in actuators)
        {
            IFormalLevelActuator actuator = behaviour as IFormalLevelActuator;
            if (actuator != null)
                actuator.Close();
        }
    }

    bool RequirementSatisfied()
    {
        if (requirement != FormalTriggerRequirement.BothPlayers)
            return occupants.Count > 0;

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

    void TryComplete()
    {
        if (complete || !RequirementSatisfied())
            return;

        CompleteTrigger();
    }

    public void CompleteImmediately(bool triggerRouteOutput = true)
    {
        if (complete)
            return;

        CompleteTrigger(triggerRouteOutput);
    }

    void CompleteTrigger(bool triggerRouteOutput = true)
    {
        complete = true;
        foreach (MonoBehaviour behaviour in actuators)
        {
            IFormalLevelActuator actuator = behaviour as IFormalLevelActuator;
            if (actuator != null)
                actuator.Open();
        }

        if (!triggerRouteOutput)
            return;

        FormalGameFlowController flow = FindObjectOfType<FormalGameFlowController>();
        if (flow == null)
            return;

        // 实体过门优先：加载后继关但不切换当前关、更不摆放角色。
        if (preloadRouteSuccessor)
        {
            Debug.Log(
                $"[PhysicalDoorTransition] exit-trigger trigger='{name}' scene='{gameObject.scene.name}' " +
                $"mode=preload successor='{successorScene}'.",
                this);
            flow.PreloadRouteSuccessor(this, openTransitionDoor: true);
            return;
        }

        bool requestsRouteAdvance = opensTransitionDoor || !string.IsNullOrEmpty(successorScene);
        if (requestsRouteAdvance)
        {
            Debug.LogWarning(
                $"[PhysicalDoorTransition] exit-trigger trigger='{name}' scene='{gameObject.scene.name}' " +
                $"mode=direct successor='{successorScene}' preload={preloadRouteSuccessor}.",
                this);
            flow.RequestRouteAdvance(this);
        }
    }
}
