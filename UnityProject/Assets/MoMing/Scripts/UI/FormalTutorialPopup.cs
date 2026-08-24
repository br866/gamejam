using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 开局玩法介绍：进第一关时弹三张图，看完就再也不弹。
///
/// 挂载位置：FormalPersistent 场景的 FormalUI Canvas 上（由 FormalUIBuilder 自动搭建）。
/// 触发时机：triggerLevelScene 这一关加载完、人和狗都就位的那一刻。
/// 记忆方式：PlayerPrefs，永久记住。想再看一遍走
/// 菜单 Tools / 默名 / 重置玩法介绍。
/// </summary>
[AddComponentMenu("MoMing/Formal Tutorial Popup")]
public class FormalTutorialPopup : MonoBehaviour
{
    public const string PrefKey = "MoMing.TutorialShown";

    [Header("面板")]
    public GameObject root;
    public Image pageImage;
    public Text pageLabel;
    public Button prevButton;
    public Button nextButton;

    [Header("内容")]
    public Sprite[] pages;

    [Header("触发")]
    [Tooltip("哪一关加载完之后弹。留空 = 任意关卡加载完就弹")]
    public string triggerLevelScene = "FormalLevel01";
    [Tooltip("关掉可以让它每次都弹，方便调试")]
    public bool rememberAcrossRuns = true;

    /// <summary>教程正在显示时为 true。暂停菜单要读它，避免 ESC 两边同时响应。</summary>
    public static bool IsShowing { get; private set; }

    private int index;
    private bool finished;
    private float prevTimeScale = 1f;
    private bool prevCursorVisible;
    private CursorLockMode prevCursorLock;

    void Awake()
    {
        IsShowing = false;
        if (root != null)
            root.SetActive(false);

        if (rememberAcrossRuns && PlayerPrefs.GetInt(PrefKey, 0) == 1)
            finished = true;
    }

    void OnEnable()
    {
        if (prevButton != null) prevButton.onClick.AddListener(Prev);
        if (nextButton != null) nextButton.onClick.AddListener(Next);
    }

    void OnDisable()
    {
        if (prevButton != null) prevButton.onClick.RemoveListener(Prev);
        if (nextButton != null) nextButton.onClick.RemoveListener(Next);
    }

    void OnDestroy()
    {
        if (IsShowing)
            Time.timeScale = 1f;
        IsShowing = false;
    }

    void Update()
    {
        if (!IsShowing)
        {
            if (!finished && ShouldTrigger())
                Show();
            return;
        }

        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
            Next();
        else if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
            Prev();
        else if (Input.GetKeyDown(KeyCode.Escape))
            Close();
    }

    bool ShouldTrigger()
    {
        if (pages == null || pages.Length == 0)
            return false;

        // 人和狗都就位 = 关卡真的能玩了，这时候弹才不会打断加载
        var actors = FormalPlayerActors.Instance;
        if (actors == null || actors.Human == null || actors.Dog == null)
            return false;

        if (string.IsNullOrEmpty(triggerLevelScene))
            return true;

        var flow = FindObjectOfType<FormalGameFlowController>();
        return flow != null && flow.CurrentLevelScene == triggerLevelScene;
    }

    public void Show()
    {
        if (root == null || pages == null || pages.Length == 0)
            return;

        IsShowing = true;
        index = 0;

        prevTimeScale = Time.timeScale > 0f ? Time.timeScale : 1f;
        Time.timeScale = 0f;

        prevCursorVisible = Cursor.visible;
        prevCursorLock = Cursor.lockState;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        root.SetActive(true);
        Refresh();
    }

    public void Next()
    {
        if (!IsShowing) return;

        if (index >= pages.Length - 1)
        {
            Close();
            return;
        }

        index++;
        Refresh();
    }

    public void Prev()
    {
        if (!IsShowing || index <= 0)
            return;

        index--;
        Refresh();
    }

    public void Close()
    {
        if (!IsShowing) return;

        IsShowing = false;
        finished = true;

        if (rememberAcrossRuns)
        {
            PlayerPrefs.SetInt(PrefKey, 1);
            PlayerPrefs.Save();
        }

        Time.timeScale = prevTimeScale;
        Cursor.visible = prevCursorVisible;
        Cursor.lockState = prevCursorLock;

        if (root != null)
            root.SetActive(false);
    }

    void Refresh()
    {
        index = Mathf.Clamp(index, 0, pages.Length - 1);

        if (pageImage != null)
            pageImage.sprite = pages[index];

        if (pageLabel != null)
            pageLabel.text = (index + 1) + " / " + pages.Length;

        // 第一张没有“上一张”；最后一张的“下一张”变成关闭，所以一直可点
        if (prevButton != null)
            prevButton.gameObject.SetActive(index > 0);
    }
}
