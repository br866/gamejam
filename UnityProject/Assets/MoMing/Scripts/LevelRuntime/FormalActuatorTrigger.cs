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

    [Header("Wwise Audio")]
    [Tooltip("Optional successful-completion Event for visible pressure plates.")]
    [SerializeField] private AK.Wwise.Event completionEvent = new AK.Wwise.Event();
    [Tooltip("Enable only on standalone pressure plates that should use the common completion sound.")]
    [SerializeField] private bool playCompletionAudio;

    [Header("调试")]
    [Tooltip("勾上之后：谁踩上来、谁离开、当前占用者是谁、条件为什么没满足，都会打到 Console。\n" +
             "查「踩了没反应」的时候勾上跑一次，查完记得取消。")]
    [SerializeField] private bool logDiagnostics;

    [Tooltip("在 Scene 视图里画出触发区的实际范围（不选中也画）。\n" +
             "用来确认盒子是不是太小、悬空、或者根本没盖住玩家站的位置。")]
    [SerializeField] private bool alwaysDrawTriggerBounds;

    private readonly HashSet<Object> occupants = new HashSet<Object>();
    private bool complete;
    private float nextDiagnosticTime;

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

        if (logDiagnostics)
            Debug.Log($"[踏板 {name}] 进入: collider='{other.name}' 解析出的角色=" +
                      $"{(occupant != null ? occupant.name : "不合格(不是本踏板认的角色)")}", this);

        if (occupant == null || !occupants.Add(occupant) || complete || !RequirementSatisfied())
        {
            if (logDiagnostics && occupant != null)
                Debug.Log($"[踏板 {name}] 还不能触发：{DescribeState()}", this);
            return;
        }

        CompleteTrigger();
    }

    void OnTriggerExit(Collider other)
    {
        Object occupant = FormalTriggerEligibility.ResolveOccupant(other, requirement);
        if (occupant != null)
            occupants.Remove(occupant);

        if (logDiagnostics && occupant != null)
            Debug.Log($"[踏板 {name}] 离开: {occupant.name}，{DescribeState()}", this);
    }

    void LateUpdate()
    {
        occupants.RemoveWhere(occupant => occupant == null);
        TryComplete();

        // 有人站在上面但一直没触发的时候，每秒说一次卡在哪儿
        if (logDiagnostics && !complete && occupants.Count > 0 && Time.time >= nextDiagnosticTime)
        {
            nextDiagnosticTime = Time.time + 1f;
            Debug.Log($"[踏板 {name}] 有人在上面但没触发：{DescribeState()}", this);
        }
    }

    /// <summary>当前占用者和条件判定的一句话描述，只在调试开关打开时用。</summary>
    string DescribeState()
    {
        List<string> names = new List<string>();
        foreach (Object occupant in occupants)
            names.Add(occupant != null ? occupant.name : "<已销毁>");

        return $"requirement={requirement} 当前占用者=[{string.Join(", ", names)}] " +
               $"满足条件={RequirementSatisfied()} complete={complete}";
    }

    void OnDrawGizmos()
    {
        if (!alwaysDrawTriggerBounds)
            return;

        DrawTriggerBounds();
    }

    void OnDrawGizmosSelected()
    {
        if (alwaysDrawTriggerBounds)
            return;

        DrawTriggerBounds();
    }

    void DrawTriggerBounds()
    {
        Collider col = GetComponent<Collider>();
        if (col == null)
            return;

        Bounds bounds = col.bounds;
        Gizmos.color = new Color(0.3f, 0.9f, 0.4f, 0.2f);
        Gizmos.DrawCube(bounds.center, bounds.size);
        Gizmos.color = new Color(0.3f, 0.9f, 0.4f, 0.95f);
        Gizmos.DrawWireCube(bounds.center, bounds.size);
    }

    public void ResetTemporaryState()
    {
        occupants.Clear();
        if (permanent)
            return;

        complete = false;

        // 不看之前记的 complete，一律把机关关回去。
        // 关一扇本来就关着的门是空操作，代价为零；
        // 反过来漏关一扇开着的门，玩家重开之后就能直接走过去，谜题白送。
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

        if (playCompletionAudio && completionEvent != null && completionEvent.IsValid())
            completionEvent.Post(gameObject);

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
