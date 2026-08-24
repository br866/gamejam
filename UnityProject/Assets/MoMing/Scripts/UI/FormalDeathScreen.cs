using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// 死亡画面。两种死因用两张不同的标题图：
///   Caught  = 被怪物抓到  -> 「你被发现了」
///   Anxiety = 焦虑涨满    -> 「你迷失了」
///
/// 在此之前两种死法都是静默直接 ResetLevel，玩家根本不知道自己为什么被传回去。
///
/// 挂载位置：FormalPersistent 场景的 FormalUI Canvas 上（由 FormalUIBuilder 搭建）。
/// 触发方：FormalAnxietyState.HandleFull() 和 MonsterPatrol.TryCatch()。
/// </summary>
[AddComponentMenu("MoMing/Formal Death Screen")]
public class FormalDeathScreen : MonoBehaviour
{
    public enum DeathCause { Caught, Anxiety }

    public static FormalDeathScreen Instance { get; private set; }

    /// <summary>死亡画面正在显示。暂停菜单/教程要读它，避免多层 UI 抢 ESC。</summary>
    public static bool IsShowing { get; private set; }

    [Header("面板")]
    public GameObject root;
    public CanvasGroup group;
    public Image titleImage;

    [Header("两种死因的标题图")]
    public Sprite caughtTitle;
    public Sprite anxietyTitle;
    [Tooltip("标题图显示宽度，高度按原图比例自动算")]
    public float titleWidth = 880f;

    [Header("选项")]
    public Button restartButton;
    public Button mainMenuButton;
    public Image restartImage;
    public Image mainMenuImage;
    public Color selectedColor = Color.white;
    public Color unselectedColor = new Color(1f, 1f, 1f, 0.45f);

    [Header("节奏")]
    [Tooltip("死亡到画面开始淡入之间的停顿（真实秒）")]
    public float delayBeforeShow = 0.5f;
    public float fadeDuration = 0.6f;

    [Header("回主菜单去哪个场景")]
    public string mainMenuSceneName = "Start";

    private int selection;          // 0 = 重新开始, 1 = 回到主菜单
    private bool interactable;      // 淡入完成前不接受输入，防手滑
    private float prevTimeScale = 1f;
    private Coroutine showRoutine;

    void Awake()
    {
        Instance = this;
        IsShowing = false;

        if (root != null)
            root.SetActive(false);
        if (group != null)
            group.alpha = 0f;
    }

    void OnDestroy()
    {
        if (IsShowing)
            Time.timeScale = 1f;
        IsShowing = false;
        if (Instance == this)
            Instance = null;
    }

    void OnEnable()
    {
        if (restartButton != null) restartButton.onClick.AddListener(Restart);
        if (mainMenuButton != null) mainMenuButton.onClick.AddListener(ReturnToMainMenu);
    }

    void OnDisable()
    {
        if (restartButton != null) restartButton.onClick.RemoveListener(Restart);
        if (mainMenuButton != null) mainMenuButton.onClick.RemoveListener(ReturnToMainMenu);
    }

    /// <summary>触发死亡画面。已经在显示时会被忽略，避免怪物每帧重复触发。</summary>
    public static bool Trigger(DeathCause cause)
    {
        if (Instance == null || IsShowing)
            return IsShowing;

        Instance.Show(cause);
        return true;
    }

    public void Show(DeathCause cause)
    {
        if (IsShowing || root == null)
            return;

        IsShowing = true;
        interactable = false;
        selection = 0;

        prevTimeScale = Time.timeScale > 0f ? Time.timeScale : 1f;
        Time.timeScale = 0f;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        ApplyTitle(cause == DeathCause.Anxiety ? anxietyTitle : caughtTitle);
        RefreshSelection();

        root.SetActive(true);
        if (group != null)
            group.alpha = 0f;

        if (showRoutine != null)
            StopCoroutine(showRoutine);
        showRoutine = StartCoroutine(FadeIn());
    }

    IEnumerator FadeIn()
    {
        // 用真实时间，因为 timeScale 已经是 0
        if (delayBeforeShow > 0f)
            yield return new WaitForSecondsRealtime(delayBeforeShow);

        if (group == null)
        {
            interactable = true;
            yield break;
        }

        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            group.alpha = fadeDuration > 0f ? Mathf.Clamp01(t / fadeDuration) : 1f;
            yield return null;
        }

        group.alpha = 1f;
        interactable = true;
        showRoutine = null;
    }

    void Update()
    {
        if (!IsShowing || !interactable)
            return;

        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W)
            || Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
        {
            // 只有两个选项，任意上下键都是切换
            selection = 1 - selection;
            RefreshSelection();
        }
        else if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)
                 || Input.GetKeyDown(KeyCode.Space))
        {
            Confirm();
        }
    }

    /// <summary>鼠标悬停时同步键盘选中项，两种操作方式不会打架。</summary>
    public void SetSelection(int index)
    {
        if (!IsShowing || !interactable)
            return;

        selection = Mathf.Clamp(index, 0, 1);
        RefreshSelection();
    }

    void RefreshSelection()
    {
        if (restartImage != null)
            restartImage.color = selection == 0 ? selectedColor : unselectedColor;
        if (mainMenuImage != null)
            mainMenuImage.color = selection == 1 ? selectedColor : unselectedColor;
    }

    void Confirm()
    {
        if (selection == 0)
            Restart();
        else
            ReturnToMainMenu();
    }

    /// <summary>标题图两张的长宽比不一样，按固定宽度反推高度，视觉上才统一。</summary>
    void ApplyTitle(Sprite sprite)
    {
        if (titleImage == null)
            return;

        titleImage.sprite = sprite;
        if (sprite == null)
            return;

        var rt = titleImage.rectTransform;
        float ratio = sprite.rect.height / Mathf.Max(1f, sprite.rect.width);
        rt.sizeDelta = new Vector2(titleWidth, titleWidth * ratio);
    }

    public void Restart()
    {
        Hide();

        var flow = FindObjectOfType<FormalGameFlowController>();
        if (flow != null)
            flow.ResetCurrentLevel();
        else
            Debug.LogError("[FormalDeathScreen] 找不到 FormalGameFlowController，重开不了。");
    }

    public void ReturnToMainMenu()
    {
        Hide();

        if (!Application.CanStreamedLevelBeLoaded(mainMenuSceneName))
        {
            Debug.LogError("[FormalDeathScreen] 场景 \"" + mainMenuSceneName + "\" 不在 Build Settings 里。");
            return;
        }

        SceneManager.LoadScene(mainMenuSceneName);
    }

    void Hide()
    {
        if (showRoutine != null)
        {
            StopCoroutine(showRoutine);
            showRoutine = null;
        }

        IsShowing = false;
        interactable = false;
        Time.timeScale = prevTimeScale;

        if (group != null)
            group.alpha = 0f;
        if (root != null)
            root.SetActive(false);
    }
}
