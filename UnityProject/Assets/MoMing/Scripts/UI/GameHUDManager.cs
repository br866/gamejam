using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// 游戏内 HUD 管理器：房间目标、动态操作提示（随角色切换更新）、暂停菜单、设置面板。
/// </summary>
public class GameHUDManager : MonoBehaviour
{
    [Header("Objective (Top-Left)")]
    [SerializeField] private Text objectiveTitleText;
    [SerializeField] private Text objectiveContentText;
    [SerializeField] private string objectiveTitle = "目标";
    [TextArea(2, 4)]
    [SerializeField] private string objectiveContent = "探索当前区域";

    [Header("Controls Help (Bottom-Right)")]
    [SerializeField] private Text commonControlsText;
    [SerializeField] private Text specialControlsText;
    [SerializeField] private GameObject controlsHelpRoot;

    [Header("Settings Button (Top-Right)")]
    [SerializeField] private Button settingsButton;

    [Header("Pause Menu")]
    [SerializeField] private GameObject pauseMenuRoot;
    [SerializeField] private Button continueButton;
    [SerializeField] private Button settingsButtonInPause;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button returnToMenuButton;
    [SerializeField] private string mainMenuSceneName = "Start";

    [Header("Settings Panel")]
    [SerializeField] private GameObject settingsPanel;

    private bool isPaused = false;
    // 全局暂停标记：供其它脚本（如相机）读取，暂停菜单打开时=true。
    public static bool IsPaused { get; private set; }
    private bool wasBoxAttached = false;

    private void Start()
    {
        UpdateObjectiveText();

        if (settingsButton != null)
            settingsButton.onClick.AddListener(OpenPauseMenu);

        if (pauseMenuRoot != null)
            pauseMenuRoot.SetActive(false);

        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        if (continueButton != null)
            continueButton.onClick.AddListener(ClosePauseMenu);

        if (settingsButtonInPause != null)
            settingsButtonInPause.onClick.AddListener(OpenSettings);

        if (restartButton != null)
            restartButton.onClick.AddListener(RestartLevel);

        if (returnToMenuButton != null)
            returnToMenuButton.onClick.AddListener(ReturnToMainMenu);

        if (PlayerManager.Instance != null)
        {
            PlayerManager.Instance.OnActiveCharacterChanged += OnActiveCharacterChanged;
            PlayerManager.Instance.OnLinkedModeChanged += OnLinkedModeChanged;
        }

        UpdateControlsHelp();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (settingsPanel != null && settingsPanel.activeSelf)
            {
                settingsPanel.SetActive(false);
                return;
            }

            if (isPaused)
                ClosePauseMenu();
            else
                OpenPauseMenu();
        }

        // 挂住/脱离箱子时刷新操作提示
        var pm = PlayerManager.Instance;
        if (pm != null && pm.IsLinkedMode && pm.human != null)
        {
            bool nowAttached = pm.human.IsBoxAttached;
            if (nowAttached != wasBoxAttached)
            {
                wasBoxAttached = nowAttached;
                UpdateControlsHelp();
            }
        }
    }

    private void OnDestroy()
    {
        if (PlayerManager.Instance != null)
        {
            PlayerManager.Instance.OnActiveCharacterChanged -= OnActiveCharacterChanged;
            PlayerManager.Instance.OnLinkedModeChanged -= OnLinkedModeChanged;
        }
    }

    private void UpdateObjectiveText()
    {
        if (objectiveTitleText != null)
            objectiveTitleText.text = objectiveTitle;
        if (objectiveContentText != null)
            objectiveContentText.text = objectiveContent;
    }

    /// <summary>
    /// 运行时设置房间目标文本。
    /// </summary>
    public void SetObjective(string title, string content)
    {
        objectiveTitle = title;
        objectiveContent = content;
        UpdateObjectiveText();
    }

    private void OnActiveCharacterChanged(bool isDog)
    {
        UpdateControlsHelp();
    }

    private void OnLinkedModeChanged(bool isLinked)
    {
        UpdateControlsHelp();
    }

    private void UpdateControlsHelp()
    {
        if (controlsHelpRoot != null)
            controlsHelpRoot.SetActive(true);

        if (commonControlsText != null)
            commonControlsText.text = "WASD 移动\nSpace 跳跃";

        if (specialControlsText == null) return;

        var pm = PlayerManager.Instance;
        if (pm == null) return;

        if (pm.IsLinkedMode)
        {
            var human = pm.human;
            if (human != null && human.IsBoxAttached)
                specialControlsText.text = "WASD 推动箱子\nF 脱离箱子\nQ 退出联动";
            else
                specialControlsText.text = "F 挂住/推动箱子\nQ 退出联动\n双人踏板";
        }
        else if (pm.IsActiveDog)
        {
            specialControlsText.text = "LeftShift 疾跑\nTab 切换角色\nQ 联动模式";
        }
        else
        {
            specialControlsText.text = "E 拾取/丢弃\nF 触发开关\nTab 切换角色\nQ 联动模式";
        }
    }

    private void OpenPauseMenu()
    {
        if (pauseMenuRoot == null) return;

        isPaused = true;
        IsPaused = true;
        pauseMenuRoot.SetActive(true);
        Time.timeScale = 0f;

        var pm = PlayerManager.Instance;
        if (pm != null && pm.ActivePlayer != null)
            pm.ActivePlayer.SetActive(false);
    }

    private void ClosePauseMenu()
    {
        if (pauseMenuRoot == null) return;

        isPaused = false;
        IsPaused = false;
        pauseMenuRoot.SetActive(false);
        Time.timeScale = 1f;

        var pm = PlayerManager.Instance;
        if (pm != null && pm.ActivePlayer != null)
            pm.ActivePlayer.SetActive(true);
    }

    private void OpenSettings()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(true);
    }

    private void RestartLevel()
    {
        IsPaused = false;
        Time.timeScale = 1f;
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }

    private void ReturnToMainMenu()
    {
        IsPaused = false;
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }
}
