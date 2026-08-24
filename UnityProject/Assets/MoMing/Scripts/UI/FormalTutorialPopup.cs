using System.Collections;
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
    public const string CheckpointPrefKeyDefault = "MoMing.SaveAreaTutorialShown";
    public const string Level045PrefKeyDefault = "MoMing.Level045TutorialShown";

    [Header("面板")]
    public GameObject root;
    public Image pageImage;
    public Text pageLabel;
    public Button prevButton;
    public Button nextButton;

    [Header("内容")]
    public Sprite[] pages;

    [Header("存档地毯介绍（第二关踩到存档点时弹）")]
    [Tooltip("踩到存档点时弹的图。留空就不弹")]
    public Sprite[] checkpointPages;
    [Tooltip("记\u201c看过了\u201d用的 PlayerPrefs 键，和开局介绍分开记")]
    public string checkpointPrefKey = CheckpointPrefKeyDefault;
    [Tooltip("踩上去之后等几秒再弹，给玩家一点“我踩到了什么”的反应时间")]
    public float checkpointDelaySeconds = 1f;

    [Header("第四点五关长廊介绍（进 4.5 关时弹）")]
    [Tooltip("进到 levelIntroScene 这一关之后弹的图。留空就不弹")]
    public Sprite[] levelIntroPages;
    [Tooltip("哪一关进去之后弹")]
    public string levelIntroScene = "FormalLevel045";
    [Tooltip("记“看过了”用的 PlayerPrefs 键，和别的介绍分开记")]
    public string levelIntroPrefKey = Level045PrefKeyDefault;
    [Tooltip("进关之后等几秒再弹，别一进去就糊玩家一脸")]
    public float levelIntroDelaySeconds = 1f;

    [Header("触发")]
    [Tooltip("哪一关加载完之后弹。留空 = 任意关卡加载完就弹")]
    public string triggerLevelScene = "FormalLevel01";
    [Tooltip("关掉可以让它每次都弹，方便调试")]
    public bool rememberAcrossRuns = true;

    /// <summary>教程正在显示时为 true。暂停菜单要读它，避免 ESC 两边同时响应。</summary>
    public static bool IsShowing { get; private set; }

    /// <summary>常驻场景里唯一那一个。存档点要靠它弹图。</summary>
    public static FormalTutorialPopup Instance { get; private set; }

    private int index;
    private bool checkpointQueued;
    private bool levelIntroQueued;
    private bool levelIntroDone;
    private string tracedLevelScene = "<还没开始>";
    private bool finished;
    private Sprite[] activePages;
    private string activePrefKey;
    private bool activeIsIntro;
    private float prevTimeScale = 1f;
    private bool prevCursorVisible;
    private CursorLockMode prevCursorLock;

    void Awake()
    {
        Instance = this;
        IsShowing = false;
        if (root != null)
            root.SetActive(false);

        if (rememberAcrossRuns && PlayerPrefs.GetInt(PrefKey, 0) == 1)
            finished = true;

        // 开局把三组图的挂载情况和“看过了”状态打出来，省得每次靠猜
        Debug.Log("[FormalTutorialPopup] 教程图配置：" +
                  "开局 " + Count(pages) + " 张(已看=" + Seen(PrefKey) + ")、" +
                  "存档地毯 " + Count(checkpointPages) + " 张(已看=" + Seen(checkpointPrefKey) + ")、" +
                  "本关介绍/" + levelIntroScene + " " + Count(levelIntroPages) + " 张(已看=" + Seen(levelIntroPrefKey) + ")。" +
                  " 小键盘 3 = 强制弹地毯介绍，小键盘 9 = 强制弹本关介绍。", this);

        Trace("========== 新的一跑 ==========");
        Trace("图配置：开局 " + Count(pages) + " 张(已看=" + Seen(PrefKey) + ")、" +
              "地毯 " + Count(checkpointPages) + " 张(已看=" + Seen(checkpointPrefKey) + ")、" +
              "本关介绍/" + levelIntroScene + " " + Count(levelIntroPages) + " 张(已看=" + Seen(levelIntroPrefKey) + ")");
    }

    /// <summary>把诊断写到工程根目录的 tutorial-debug.log，方便离线看这一跑到底发生了什么。</summary>
    public static void Trace(string line)
    {
        try
        {
            string path = Application.dataPath + "/../tutorial-debug.log";
            System.IO.File.AppendAllText(path,
                System.DateTime.Now.ToString("HH:mm:ss") + "  " + line + "\n");
        }
        catch (System.Exception)
        {
            // 写不了就算了，不影响游戏
        }
    }

    static int Count(Sprite[] arr)
    {
        return arr == null ? 0 : arr.Length;
    }

    static string Seen(string prefKey)
    {
        return string.IsNullOrEmpty(prefKey)
            ? "无键"
            : (PlayerPrefs.GetInt(prefKey, 0) == 1 ? "是" : "否");
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
        if (Instance == this)
            Instance = null;
    }

    void Update()
    {
        TraceLevelChange();

        if (!IsShowing)
        {
            // GM：不用真跑到那一关也能验证图对不对
            if (Input.GetKeyDown(KeyCode.Keypad3))
            {
                Debug.Log("[FormalTutorialPopup] GM 强制弹存档地毯介绍（" + Count(checkpointPages) + " 张）。", this);
                ForceShow(checkpointPages, checkpointPrefKey);
                return;
            }

            if (Input.GetKeyDown(KeyCode.Keypad9))
            {
                Debug.Log("[FormalTutorialPopup] GM 强制弹本关介绍（" + Count(levelIntroPages) + " 张）。", this);
                ForceShow(levelIntroPages, levelIntroPrefKey);
                return;
            }

            if (!finished && ShouldTrigger())
                Show();
            else
                TryTriggerLevelIntro();
            return;
        }

        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
            Next();
        else if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
            Prev();
        else if (Input.GetKeyDown(KeyCode.Escape))
            Close();
    }

    void TraceLevelChange()
    {
        var flow = FindObjectOfType<FormalGameFlowController>();
        string now = flow != null ? flow.CurrentLevelScene : "<没有 flow>";
        if (now == tracedLevelScene)
            return;

        tracedLevelScene = now;
        var actors = FormalPlayerActors.Instance;
        string human = actors != null && actors.Human != null
            ? actors.Human.transform.position.ToString("F2")
            : "<没有人>";
        Trace("当前关卡 -> " + now + "，人在 " + human +
              "，本关介绍已弹过=" + levelIntroDone + "，图=" + Count(levelIntroPages) + " 张");
    }

    bool ShouldTrigger()
    {
        if (pages == null || pages.Length == 0)
            return false;

        return LevelReady(triggerLevelScene);
    }

    /// <summary>角色就位，而且当前就在这一关 = 可以弹了。scene 留空表示任意关卡。</summary>
    bool LevelReady(string scene, bool requireDog = true)
    {
        // 角色就位 = 关卡真的能玩了，这时候弹才不会打断加载。
        // 4.5 关那段是人自己走的，狗不一定还在，所以那边不要求狗
        var actors = FormalPlayerActors.Instance;
        if (actors == null || actors.Human == null)
            return false;

        if (requireDog && actors.Dog == null)
            return false;

        if (string.IsNullOrEmpty(scene))
            return true;

        var flow = FindObjectOfType<FormalGameFlowController>();
        return flow != null && flow.CurrentLevelScene == scene;
    }

    /// <summary>
    /// 进到 4.5 关（levelIntroScene）之后，等一会儿弹长廊怪物介绍，全流程只弹一次。
    /// 这条是兜底轮询：走关卡入口封关区（FormalLevelEntrySeal）那条路更早、更准。
    /// </summary>
    void TryTriggerLevelIntro()
    {
        if (!LevelIntroPending())
            return;

        // 这一段是人单独走的，别要求狗也在
        if (!LevelReady(levelIntroScene, false))
            return;

        Trace("轮询命中：当前关卡已经是 " + levelIntroScene);
        BeginLevelIntro("当前关卡已经是 " + levelIntroScene);
    }

    /// <summary>
    /// 本关介绍图。由关卡入口的 FormalLevelEntrySeal（勾了 Show Level Intro Tutorial 的那个）调，
    /// 人一踏进入口区就算数，不用等关卡切换正式提交。全流程只弹一次。
    /// </summary>
    public bool ShowLevelIntroTutorial()
    {
        if (!LevelIntroPending())
            return false;

        Trace("外部触发命中（入口区/存档点/出生点触发区）");
        BeginLevelIntro("入口区触发");
        return true;
    }

    bool LevelIntroPending()
    {
        if (levelIntroDone || levelIntroQueued)
            return false;

        if (rememberAcrossRuns && !string.IsNullOrEmpty(levelIntroPrefKey) &&
            PlayerPrefs.GetInt(levelIntroPrefKey, 0) == 1)
        {
            levelIntroDone = true;
            return false;
        }

        return true;
    }

    void BeginLevelIntro(string reason)
    {
        levelIntroDone = true;

        if (levelIntroPages == null || levelIntroPages.Length == 0)
        {
            Debug.LogWarning("[FormalTutorialPopup] 该弹本关介绍了（" + reason +
                             "），但 Level Intro Pages 是空的，这次不弹。" +
                             "去 FormalPersistent 场景跑一下 Tools/默名/挂上额外教程图（地毯 + 4.5 关），然后 Ctrl+S。", this);
            Trace("!! 该弹本关介绍了（" + reason + "），但 Level Intro Pages 是空的");
            return;
        }

        Debug.Log("[FormalTutorialPopup] " + reason + "，" + levelIntroDelaySeconds + " 秒后弹本关介绍。", this);

        if (levelIntroDelaySeconds <= 0f)
        {
            ShowOnce(levelIntroPages, levelIntroPrefKey);
            return;
        }

        levelIntroQueued = true;
        StartCoroutine(ShowDelayed(levelIntroPages, levelIntroPrefKey, levelIntroDelaySeconds,
            () => levelIntroQueued = false));
    }

    public void Show()
    {
        ShowPages(pages, PrefKey, true);
    }

    /// <summary>GM 用：无视“看过了”直接弹，没配图就报警告。</summary>
    void ForceShow(Sprite[] content, string prefKey)
    {
        if (content == null || content.Length == 0)
        {
            Debug.LogWarning("[FormalTutorialPopup] 这组图没配，弹不出来。" +
                             "去 FormalPersistent 场景跑 Tools/默名/挂上额外教程图（地毯 + 4.5 关），然后 Ctrl+S。", this);
            return;
        }

        ShowPages(content, prefKey, false);
    }

    /// <summary>
    /// 弹一组额外的教程图，用自己的 prefKey 单独记\u201c看过了\u201d。
    /// 已经看过、正在弹别的、或者没配图，都直接不弹并返回 false。
    /// </summary>
    public bool ShowOnce(Sprite[] content, string prefKey)
    {
        if (IsShowing || content == null || content.Length == 0)
            return false;

        if (rememberAcrossRuns && !string.IsNullOrEmpty(prefKey) && PlayerPrefs.GetInt(prefKey, 0) == 1)
            return false;

        ShowPages(content, prefKey, false);
        return IsShowing;
    }

    /// <summary>
    /// 存档地毯介绍。第二关踩到存档点时由 FormalCheckpoint 调，全流程只弹一次。
    /// 会等 checkpointDelaySeconds 秒再弹，让玩家先看清自己踩到了地毯。
    /// </summary>
    public bool ShowCheckpointTutorial()
    {
        if (checkpointPages == null || checkpointPages.Length == 0)
        {
            Debug.LogWarning("[FormalTutorialPopup] 存档地毯介绍没配图（Checkpoint Pages 是空的），这次不弹。" +
                             "去 FormalPersistent 场景跑一下 Tools/默名/挂上额外教程图（地毯 + 4.5 关）。", this);
            return false;
        }

        if (checkpointQueued || IsShowing)
            return false;

        if (rememberAcrossRuns && !string.IsNullOrEmpty(checkpointPrefKey) &&
            PlayerPrefs.GetInt(checkpointPrefKey, 0) == 1)
            return false;

        if (checkpointDelaySeconds <= 0f)
            return ShowOnce(checkpointPages, checkpointPrefKey);

        checkpointQueued = true;
        StartCoroutine(ShowDelayed(checkpointPages, checkpointPrefKey, checkpointDelaySeconds,
            () => checkpointQueued = false));
        return true;
    }

    /// <summary>等 delay 秒（真实时间）再弹。要是那会儿正在弹别的，就排队等它关掉。</summary>
    IEnumerator ShowDelayed(Sprite[] content, string prefKey, float delay, System.Action onDone)
    {
        // 用 unscaled，别的东西把 timeScale 调慢了也照样按真实时间等
        float waited = 0f;
        while (waited < delay)
        {
            waited += Time.unscaledDeltaTime;
            yield return null;
        }

        // 这会儿要是正好在弹别的（比如开局介绍没关），排队等它关掉再弹
        while (IsShowing)
            yield return null;

        if (onDone != null)
            onDone();

        ShowOnce(content, prefKey);
    }

    void ShowPages(Sprite[] content, string prefKey, bool isIntro)
    {
        if (root == null || content == null || content.Length == 0)
        {
            Trace("!! ShowPages 被叫了但弹不出来：root=" + (root != null) + " 张数=" + Count(content));
            return;
        }

        Trace("弹出来了：" + Count(content) + " 张，key=" + prefKey);

        activePages = content;
        activePrefKey = prefKey;
        activeIsIntro = isIntro;

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

        if (index >= activePages.Length - 1)
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

        // 只有开局那组看完才算 finished，不然存档点这组会把开局介绍顶掉
        if (activeIsIntro)
            finished = true;

        if (rememberAcrossRuns && !string.IsNullOrEmpty(activePrefKey))
        {
            PlayerPrefs.SetInt(activePrefKey, 1);
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
        if (activePages == null || activePages.Length == 0)
            return;

        index = Mathf.Clamp(index, 0, activePages.Length - 1);

        if (pageImage != null)
            pageImage.sprite = activePages[index];

        // 只有一张的时候不显示 "1 / 1"
        if (pageLabel != null)
            pageLabel.text = activePages.Length > 1 ? (index + 1) + " / " + activePages.Length : string.Empty;

        // 第一张没有“上一张”；最后一张的“下一张”变成关闭，所以一直可点
        if (prevButton != null)
            prevButton.gameObject.SetActive(index > 0);
    }
}
