using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// 正式关卡的暂停菜单。挂在 FormalPersistent 场景的 FormalUI Canvas 上。
///
/// 为什么不用旧的 GameHUDManager：那个绑死在 PlayerSystem.prefab 上，
/// 而正式关卡根本没有那个 prefab，所以 ESC 一直没人接。
///
/// 面板层级：设置面板 &gt; 暂停菜单 &gt; HUD。ESC 一次关一层。
/// </summary>
[AddComponentMenu("MoMing/Formal Pause Menu")]
public class FormalPauseMenu : MonoBehaviour
{
    [Header("面板")]
    public GameObject pauseRoot;
    public GameObject settingsPanel;
    [Tooltip("暂停时要不要把 HUD 藏起来")]
    public GameObject hudRoot;
    public bool hideHudWhilePaused = true;

    [Header("按钮")]
    public Button continueButton;
    public Button settingsButton;
    public Button restartButton;
    public Button mainMenuButton;

    [Header("主菜单场景名")]
    public string mainMenuSceneName = "Start";

    /// <summary>给相机等脚本读的全局暂停标记</summary>
    public static bool IsPaused { get; private set; }

    private float prevTimeScale = 1f;
    private bool cursorWasVisible;
    private CursorLockMode prevCursorLock;

    void Awake()
    {
        IsPaused = false;

        if (pauseRoot != null)
            pauseRoot.SetActive(false);
        if (settingsPanel != null)
            settingsPanel.SetActive(false);
    }

    void OnEnable()
    {
        if (continueButton != null) continueButton.onClick.AddListener(Resume);
        if (settingsButton != null) settingsButton.onClick.AddListener(OpenSettings);
        if (restartButton != null) restartButton.onClick.AddListener(RestartLevel);
        if (mainMenuButton != null) mainMenuButton.onClick.AddListener(ReturnToMainMenu);
    }

    void OnDisable()
    {
        if (continueButton != null) continueButton.onClick.RemoveListener(Resume);
        if (settingsButton != null) settingsButton.onClick.RemoveListener(OpenSettings);
        if (restartButton != null) restartButton.onClick.RemoveListener(RestartLevel);
        if (mainMenuButton != null) mainMenuButton.onClick.RemoveListener(ReturnToMainMenu);
    }

    void OnDestroy()
    {
        // 别把 timeScale=0 带进下一个场景
        if (IsPaused)
            Time.timeScale = 1f;
        IsPaused = false;
    }

    void Update()
    {
        // 开局玩法介绍开着的时候，ESC 归它管（它自己会翻页/跳过），
        // 否则一按 ESC 会同时弹出暂停菜单，两层叠在一起。
        if (FormalTutorialPopup.IsShowing || FormalDeathScreen.IsShowing)
            return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // 设置面板开着 -> ESC 只关设置，回到暂停菜单
            if (settingsPanel != null && settingsPanel.activeSelf)
            {
                settingsPanel.SetActive(false);
            }
            else if (IsPaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }

        // 设置面板是自己管自己关闭的（返回按钮走 SettingsManager.Close），
        // 所以这里每帧同步一次：设置关了就把暂停菜单显示回来。
        if (IsPaused && pauseRoot != null && settingsPanel != null)
        {
            bool showPause = !settingsPanel.activeSelf;
            if (pauseRoot.activeSelf != showPause)
                pauseRoot.SetActive(showPause);
        }
    }

    public void Pause()
    {
        if (IsPaused || pauseRoot == null)
            return;

        IsPaused = true;
        prevTimeScale = Time.timeScale > 0f ? Time.timeScale : 1f;
        Time.timeScale = 0f;

        cursorWasVisible = Cursor.visible;
        prevCursorLock = Cursor.lockState;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        pauseRoot.SetActive(true);
        if (hideHudWhilePaused && hudRoot != null)
            hudRoot.SetActive(false);
    }

    public void Resume()
    {
        if (!IsPaused)
            return;

        IsPaused = false;
        Time.timeScale = prevTimeScale;

        Cursor.visible = cursorWasVisible;
        Cursor.lockState = prevCursorLock;

        if (pauseRoot != null) pauseRoot.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (hudRoot != null) hudRoot.SetActive(true);
    }

    public void OpenSettings()
    {
        if (settingsPanel == null)
            return;

        settingsPanel.SetActive(true);
        // 两层 UI 别叠在一起
        if (pauseRoot != null) pauseRoot.SetActive(false);
    }

    /// <summary>重开当前这一关（不是从头开始整条流程）</summary>
    public void RestartLevel()
    {
        var flow = FindObjectOfType<FormalGameFlowController>();
        if (flow == null)
        {
            Debug.LogError("[FormalPauseMenu] 找不到 FormalGameFlowController，重开不了。");
            return;
        }

        Resume();
        flow.ResetCurrentLevel();
    }

    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        IsPaused = false;

        if (!Application.CanStreamedLevelBeLoaded(mainMenuSceneName))
        {
            Debug.LogError("[FormalPauseMenu] 场景 \"" + mainMenuSceneName + "\" 不在 Build Settings 里。");
            return;
        }

        SceneManager.LoadScene(mainMenuSceneName);
    }
}
