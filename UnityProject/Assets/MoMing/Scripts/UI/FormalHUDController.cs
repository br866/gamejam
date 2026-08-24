using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 正式关卡的常驻 HUD 控制器。
///
/// 为什么要重写一份而不用旧的 GameHUDManager：
/// 旧的绑死在 GameManager + PlayerManager 上，正式关走的是
/// FormalAnxietyState + FormalPlayerActors + FormalPlayerControl，引用对不上。
///
/// 挂载位置：FormalPersistent 场景里的 FormalHUD Canvas 根节点。
/// 关卡是 additive 加载的，HUD 必须待在常驻场景，否则换关就没了。
/// </summary>
// 注意：这里用的是 legacy UI Text，不是 TextMeshPro。
// 项目里唯一的 TMP 字体是 LiberationSans SDF，它没有中文字形，
// 中文全会变成方块。legacy Text 配内置 Arial 在 Windows 上会回退到系统中文字体，
// 主菜单一直是这么干的。等有中文 TMP 字体资源了再换回来。
[AddComponentMenu("MoMing/Formal HUD Controller")]
public class FormalHUDController : MonoBehaviour
{
    public static FormalHUDController Instance { get; private set; }

    [Header("焦虑条")]
    [Tooltip("焦虑进度条。AnxietyPostFX 也可以直接读这个 Slider")]
    public Slider anxietyBar;
    [Tooltip("进度条的 Fill 图片。留空就不做变色")]
    public Image anxietyFill;
    [Tooltip("勾上=用下面的渐变给 Fill 染色。\n" +
             "Fill 换成手绘线条图之后取消勾选，保留原图颜色。")]
    public bool tintAnxietyFill = true;
    [Tooltip("焦虑 0 -> 1 时 Fill 的颜色渐变")]
    public Gradient anxietyGradient = DefaultAnxietyGradient();
    [Tooltip("整条焦虑条的根节点。焦虑为 0 时可以整条淡出")]
    public CanvasGroup anxietyGroup;
    [Tooltip("勾上后焦虑为 0 时焦虑条淡出，不挡画面")]
    public bool hideAnxietyBarWhenCalm = true;
    [Tooltip("焦虑条淡入淡出速度")]
    public float anxietyBarFadeSpeed = 4f;
    [Tooltip("骑在红线尽头、跟着焦虑值往右滑的发光游标。留空就不用")]
    public RectTransform anxietyFillHead;
    [Tooltip("游标在焦虑条本地坐标里的左端点 X（焦虑 0）")]
    public float anxietyFillHeadMinX = -345f;
    [Tooltip("游标在焦虑条本地坐标里的右端点 X（焦虑 1）")]
    public float anxietyFillHeadMaxX = 345f;

    [Header("焦虑状态文字（参考图里进度条下面那行）")]
    [Tooltip("显示\u201c轻度焦虑 / 中度焦虑 / 重度焦虑\u201d的文本。留空就不显示")]
    public Text anxietyStateText;
    [Tooltip("从低到高的状态描述。阈值由 anxietyStateThresholds 决定")]
    public string[] anxietyStateLabels = { "", "轻度焦虑", "中度焦虑", "重度焦虑" };
    [Tooltip("每一档的下限（归一化）。长度要和 anxietyStateLabels 一致，且从小到大")]
    public float[] anxietyStateThresholds = { 0f, 0.05f, 0.45f, 0.75f };

    [Header("目标提示（左上）")]
    public Text objectiveText;
    [TextArea(1, 3)]
    public string defaultObjective = "找到出口";

    [Header("操作提示（右下）")]
    public Text controlsText;
    [TextArea(2, 5)]
    public string humanControls = "WASD 移动   Space 跳跃   LeftShift 疾跑\nE 互动   F 推箱子   Tab 切换到小狗";
    [TextArea(2, 5)]
    public string dogControls = "WASD 移动   Space 跳跃   Tab 切换到人";

    [Header("场景内文字提示（居中偏下）")]
    [Tooltip("提示条的 CanvasGroup，用来淡入淡出")]
    public CanvasGroup hintGroup;
    public Text hintText;
    [Tooltip("默认停留时长（秒）")]
    public float defaultHintDuration = 3f;
    [Tooltip("淡入淡出时长（秒）")]
    public float hintFadeDuration = 0.25f;

    private FormalAnxietyState anxiety;
    private FormalPlayerControl playerControl;
    private Coroutine hintRoutine;
    private float anxietyGroupTargetAlpha;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (hintGroup != null)
            hintGroup.alpha = 0f;

        if (anxietyGroup != null)
            anxietyGroup.alpha = hideAnxietyBarWhenCalm ? 0f : 1f;
    }

    void Start()
    {
        SetObjective(defaultObjective);
        ApplyAnxiety(0f);
        RefreshControlsText();
    }

    void OnDestroy()
    {
        UnsubscribeAnxiety();
        UnsubscribeControl();
        if (Instance == this)
            Instance = null;
    }

    void Update()
    {
        // 焦虑源和角色控制器都在关卡场景里，换关会换新的，所以每帧确认一次引用。
        EnsureAnxietySubscription();
        EnsureControlSubscription();
        UpdateAnxietyGroupAlpha();
    }

    // ---------- 焦虑条 ----------

    private void EnsureAnxietySubscription()
    {
        FormalAnxietyState current = FormalAnxietyState.Instance;
        if (current == anxiety)
            return;

        UnsubscribeAnxiety();
        anxiety = current;

        if (anxiety != null)
        {
            anxiety.OnNormalizedChanged += ApplyAnxiety;
            ApplyAnxiety(anxiety.Normalized);
        }
    }

    private void UnsubscribeAnxiety()
    {
        if (anxiety != null)
            anxiety.OnNormalizedChanged -= ApplyAnxiety;
        anxiety = null;
    }

    private void ApplyAnxiety(float normalized)
    {
        if (anxietyBar != null)
        {
            anxietyBar.minValue = 0f;
            anxietyBar.maxValue = 1f;
            anxietyBar.value = normalized;
        }

        if (anxietyFill != null)
        {
            if (tintAnxietyFill && anxietyGradient != null)
                anxietyFill.color = anxietyGradient.Evaluate(normalized);

            // Fill 用手绘线条图时把 Image 设成 Filled/Horizontal，由这里驱动 fillAmount：
            // 线是从左往右一段段显出来，而不是被 Slider 横向压扁。
            if (anxietyFill.type == Image.Type.Filled)
                anxietyFill.fillAmount = normalized;
        }

        if (anxietyFillHead != null)
        {
            Vector2 head = anxietyFillHead.anchoredPosition;
            head.x = Mathf.Lerp(anxietyFillHeadMinX, anxietyFillHeadMaxX, Mathf.Clamp01(normalized));
            anxietyFillHead.anchoredPosition = head;
        }

        UpdateAnxietyStateText(normalized);

        anxietyGroupTargetAlpha = (!hideAnxietyBarWhenCalm || normalized > 0.001f) ? 1f : 0f;
    }

    /// <summary>
    /// 按归一化焦虑值挑一档状态文字。取最后一个"阈值 <= 当前值"的档。
    /// </summary>
    private void UpdateAnxietyStateText(float normalized)
    {
        if (anxietyStateText == null || anxietyStateLabels == null || anxietyStateLabels.Length == 0)
            return;

        int index = 0;
        int count = Mathf.Min(anxietyStateLabels.Length, anxietyStateThresholds != null ? anxietyStateThresholds.Length : 0);

        for (int i = 0; i < count; i++)
        {
            if (normalized >= anxietyStateThresholds[i])
                index = i;
        }

        string label = anxietyStateLabels[Mathf.Clamp(index, 0, anxietyStateLabels.Length - 1)];
        if (anxietyStateText.text != label)
            anxietyStateText.text = label;
    }

    private void UpdateAnxietyGroupAlpha()
    {
        if (anxietyGroup == null)
            return;

        anxietyGroup.alpha = Mathf.MoveTowards(
            anxietyGroup.alpha,
            anxietyGroupTargetAlpha,
            anxietyBarFadeSpeed * Time.unscaledDeltaTime);
    }

    // ---------- 目标 ----------

    /// <summary>设置左上角的关卡目标文本</summary>
    public void SetObjective(string content)
    {
        if (objectiveText != null)
            objectiveText.text = content;
    }

    // ---------- 操作提示 ----------

    private void EnsureControlSubscription()
    {
        if (playerControl != null)
            return;

        playerControl = FindObjectOfType<FormalPlayerControl>();
        if (playerControl != null)
        {
            playerControl.ActiveRoleChanged += OnActiveRoleChanged;
            RefreshControlsText();
        }
    }

    private void UnsubscribeControl()
    {
        if (playerControl != null)
            playerControl.ActiveRoleChanged -= OnActiveRoleChanged;
        playerControl = null;
    }

    private void OnActiveRoleChanged(bool isDog)
    {
        RefreshControlsText();
    }

    private void RefreshControlsText()
    {
        if (controlsText == null)
            return;

        bool isDog = playerControl != null && playerControl.IsDogActive;
        controlsText.text = isDog ? dogControls : humanControls;
    }

    // ---------- 场景内文字提示 ----------

    /// <summary>弹一条居中提示，用默认时长</summary>
    public void ShowHint(string message)
    {
        ShowHint(message, defaultHintDuration);
    }

    /// <summary>弹一条居中提示，指定停留时长（秒）</summary>
    public void ShowHint(string message, float duration)
    {
        if (hintText == null || hintGroup == null)
        {
            Debug.LogWarning("[FormalHUDController] hintText / hintGroup 没有赋值，提示无法显示：" + message);
            return;
        }

        if (hintRoutine != null)
            StopCoroutine(hintRoutine);

        hintRoutine = StartCoroutine(HintRoutine(message, duration));
    }

    /// <summary>立刻收起当前提示</summary>
    public void ClearHint()
    {
        if (hintRoutine != null)
        {
            StopCoroutine(hintRoutine);
            hintRoutine = null;
        }

        if (hintGroup != null)
            hintGroup.alpha = 0f;
    }

    private IEnumerator HintRoutine(string message, float duration)
    {
        hintText.text = message;

        yield return FadeHint(hintGroup.alpha, 1f, hintFadeDuration);

        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            yield return null;
        }

        yield return FadeHint(1f, 0f, hintFadeDuration);
        hintRoutine = null;
    }

    private IEnumerator FadeHint(float from, float to, float duration)
    {
        if (duration <= 0f)
        {
            hintGroup.alpha = to;
            yield break;
        }

        float t = 0f;
        while (t < duration)
        {
            // 用 unscaledDeltaTime，暂停时提示也能正常淡出
            t += Time.unscaledDeltaTime;
            hintGroup.alpha = Mathf.Lerp(from, to, t / duration);
            yield return null;
        }

        hintGroup.alpha = to;
    }

    // ---------- 工具 ----------

    private static Gradient DefaultAnxietyGradient()
    {
        var g = new Gradient();
        g.SetKeys(
            new[]
            {
                new GradientColorKey(new Color(0.55f, 0.75f, 0.62f), 0f),
                new GradientColorKey(new Color(0.85f, 0.72f, 0.35f), 0.6f),
                new GradientColorKey(new Color(0.75f, 0.15f, 0.15f), 1f),
            },
            new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1f) });
        return g;
    }
}
