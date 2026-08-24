using UnityEngine;

/// <summary>
/// 正式关卡的焦虑值状态机（从旧的 GameManager 里剥离出来的纯数据层）。
///
/// 和旧 GameManager 的区别：
/// - 只管"焦虑值本身"，不碰灯光、污渍、红晕、存档点、场景切换。
/// - 数据源是 FormalPlayerActors（正式关的人/狗），不是老的 PlayerManager。
/// - 挂在 FormalPersistent 常驻场景上，关卡 additive 加载/卸载都不会丢。
///
/// 用法：
/// 1. 在 FormalPersistent 场景里新建空物体 "FormalAnxiety"，挂本脚本。
/// 2. 表现层（进度条、后处理、音效）自己订阅 OnNormalizedChanged，或者轮询 Normalized。
/// </summary>
[AddComponentMenu("MoMing/Formal Anxiety State")]
public class FormalAnxietyState : MonoBehaviour
{
    public enum FullBehaviour
    {
        /// <summary>只发事件，由别人决定怎么处理</summary>
        None,
        /// <summary>调用当前关卡的 FormalLevelController.ResetLevel()</summary>
        ResetLevel,
    }

    public static FormalAnxietyState Instance { get; private set; }

    [Header("数值")]
    [Tooltip("焦虑值上限。分离后从 0 涨满需要 max / increaseRate 秒")]
    public float maxAnxiety = 100f;
    [Tooltip("分离时每秒上涨多少。maxAnxiety / increaseRate = 涨满所需秒数。4 = 25 秒")]
    public float increaseRate = 4f;
    [Tooltip("重新靠拢时每秒下降多少")]
    public float decreaseRate = 6f;

    [Header("分离判定（带回滞，避免边界抖动）")]
    [Tooltip("距离超过这个值算分离，焦虑开始上涨")]
    public float separationRadius = 8f;
    [Tooltip("距离小于这个值算重新靠拢。必须小于 separationRadius")]
    public float togetherRadius = 5f;

    [Header("行为")]
    [Tooltip("焦虑涨满时做什么")]
    public FullBehaviour onFull = FullBehaviour.ResetLevel;
    [Tooltip("换关卡时自动把焦虑清零")]
    public bool resetOnLevelChange = true;
    [Tooltip("找不到人或狗时（关卡加载中）暂停计算，而不是当成分离")]
    public bool freezeWhenActorsMissing = true;

    [Header("调试")]
    [Tooltip("按住这个键强制把焦虑拉满，用来验证表现")]
    public KeyCode forceMaxKey = KeyCode.F4;
    [Tooltip("按这个键把焦虑清零")]
    public KeyCode clearKey = KeyCode.F5;
    [SerializeField] private bool logStateChanges = false;

    /// <summary>当前焦虑值（0 ~ maxAnxiety）</summary>
    public float Current { get; private set; }

    /// <summary>归一化焦虑值（0 ~ 1）。表现层基本只需要这个。</summary>
    public float Normalized => maxAnxiety > 0.0001f ? Mathf.Clamp01(Current / maxAnxiety) : 0f;

    /// <summary>人和狗当前是否处于分离状态</summary>
    public bool IsSeparated { get; private set; }

    /// <summary>人狗当前距离。找不到角色时为 -1。</summary>
    public float ActorDistance { get; private set; } = -1f;

    /// <summary>焦虑归一化值变化时触发（每帧最多一次，值有变化才发）</summary>
    public event System.Action<float> OnNormalizedChanged;

    /// <summary>分离状态翻转时触发。参数 = 是否分离。</summary>
    public event System.Action<bool> OnSeparationChanged;

    /// <summary>焦虑涨满时触发一次</summary>
    public event System.Action OnFull;

    private FormalPlayerActors cachedActors;
    private float lastBroadcastNormalized = -1f;
    private bool suspended;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    void Update()
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        if (forceMaxKey != KeyCode.None && Input.GetKeyDown(forceMaxKey))
            SetNormalized(1f);
        if (clearKey != KeyCode.None && Input.GetKeyDown(clearKey))
            ResetAnxiety();
#endif

        if (suspended)
            return;

        bool actorsReady = RefreshActors();

        if (!actorsReady)
        {
            if (freezeWhenActorsMissing)
            {
                Broadcast();
                return;
            }
        }
        else
        {
            EvaluateSeparation();
        }

        float delta = IsSeparated ? increaseRate : -decreaseRate;
        Current = Mathf.Clamp(Current + delta * Time.deltaTime, 0f, maxAnxiety);

        Broadcast();

        if (Current >= maxAnxiety)
            HandleFull();
    }

    /// <summary>
    /// 找到当前关卡里的 FormalPlayerActors。关卡是 additive 加载的，
    /// 每次换关 Instance 都会换成新的一份，这里顺便做换关重置。
    /// </summary>
    private bool RefreshActors()
    {
        FormalPlayerActors actors = FormalPlayerActors.Instance;

        if (actors != cachedActors)
        {
            cachedActors = actors;
            if (resetOnLevelChange && actors != null)
            {
                ResetAnxiety();
                if (logStateChanges)
                    Debug.Log("[FormalAnxietyState] 检测到新关卡的角色，焦虑已重置。");
            }
        }

        if (actors == null || actors.Human == null || actors.Dog == null)
        {
            ActorDistance = -1f;
            return false;
        }

        return true;
    }

    private void EvaluateSeparation()
    {
        Vector3 humanPos = cachedActors.Human.transform.position;
        Vector3 dogPos = cachedActors.Dog.transform.position;
        ActorDistance = Vector3.Distance(humanPos, dogPos);

        // 回滞：分离用大半径，重聚用小半径，中间地带保持原状态。
        float exitRadius = Mathf.Max(separationRadius, togetherRadius + 0.01f);
        float enterRadius = Mathf.Min(togetherRadius, exitRadius - 0.01f);

        bool wasSeparated = IsSeparated;

        if (!IsSeparated && ActorDistance > exitRadius)
            IsSeparated = true;
        else if (IsSeparated && ActorDistance < enterRadius)
            IsSeparated = false;

        if (wasSeparated != IsSeparated)
        {
            if (logStateChanges)
                Debug.Log($"[FormalAnxietyState] 分离状态 -> {IsSeparated}（距离 {ActorDistance:F2}）");
            OnSeparationChanged?.Invoke(IsSeparated);
        }
    }

    private void Broadcast()
    {
        float n = Normalized;
        if (Mathf.Approximately(n, lastBroadcastNormalized))
            return;

        lastBroadcastNormalized = n;
        OnNormalizedChanged?.Invoke(n);
    }

    private void HandleFull()
    {
        OnFull?.Invoke();

        switch (onFull)
        {
            case FullBehaviour.ResetLevel:
                var level = FindObjectOfType<FormalLevelController>();
                if (level != null)
                    level.ResetLevel();
                else
                    Debug.LogWarning("[FormalAnxietyState] 焦虑涨满，但场景里找不到 FormalLevelController。");
                ResetAnxiety();
                break;

            case FullBehaviour.None:
            default:
                break;
        }
    }

    // ---------- 对外接口 ----------

    /// <summary>焦虑清零</summary>
    public void ResetAnxiety()
    {
        Current = 0f;
        IsSeparated = false;
        Broadcast();
    }

    /// <summary>直接设定归一化焦虑值（0~1），调试或剧情用</summary>
    public void SetNormalized(float normalized)
    {
        Current = Mathf.Clamp01(normalized) * maxAnxiety;
        Broadcast();
    }

    /// <summary>加一笔焦虑（可为负），例如被怪物看到时瞬间 +20</summary>
    public void AddAnxiety(float amount)
    {
        Current = Mathf.Clamp(Current + amount, 0f, maxAnxiety);
        Broadcast();
    }

    /// <summary>暂停/恢复焦虑计算。过场动画、结算界面用。</summary>
    public void SetSuspended(bool value)
    {
        suspended = value;
    }
}
