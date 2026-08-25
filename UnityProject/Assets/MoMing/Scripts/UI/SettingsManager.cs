using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 设置面板管理器：音乐/音效音量滑块实时调节，PlayerPrefs 持久化，跨场景生效。
/// 挂在 SettingsPanel 根节点上。
/// </summary>
public class SettingsManager : MonoBehaviour
{
    [Header("Sliders")]
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private Slider brightnessSlider;

    [Header("Close Button")]
    [SerializeField] private Button closeButton;

    private const string MusicPref = "MusicVolume";
    private const string SfxPref = "SFXVolume";
    private const string BrightnessPref = "Brightness";
    private const float WwiseVolumeMax = 100f;

    private static float musicVolume = 1f;
    private static float sfxVolume = 1f;
    // 0.5 = 原样，往左变暗，往右变亮
    private static float brightness = 0.5f;
    private static bool initialized = false;
    private static WwiseUIFeedbackSettings wwiseAudioSettings;
    private static bool warnedMissingWwiseAudioSettings;
    private static bool warnedMissingMusicVolumeRtpc;
    private static bool warnedMissingSfxVolumeRtpc;

    // 常驻的全屏遮罩，用来做亮度。DontDestroyOnLoad，所有场景都吃这个设置。
    private static Image brightnessOverlay;

    // 记录打开菜单前的时间流速，关闭时恢复（避免和暂停菜单叠加时冲突）
    private float prevTimeScale = 1f;

    public static float MusicVolume => musicVolume;
    public static float SfxVolume => sfxVolume;
    public static float Brightness => brightness;

    /// <summary>
    /// 面板默认是关闭的，Awake 不会跑，所以亮度得有个独立的启动入口，
    /// 否则玩家不进设置界面就永远是默认亮度。
    /// </summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        LoadPrefs();
        ApplyBrightness();
        SubscribeToWwiseInitialization();
        ApplyWwiseVolumes();
    }

    private static void LoadPrefs()
    {
        if (initialized)
            return;

        musicVolume = Mathf.Clamp01(PlayerPrefs.GetFloat(MusicPref, 1f));
        sfxVolume = Mathf.Clamp01(PlayerPrefs.GetFloat(SfxPref, 1f));
        brightness = PlayerPrefs.GetFloat(BrightnessPref, 0.5f);
        initialized = true;
    }

    private void Awake()
    {
        LoadPrefs();
        ApplyBrightness();
        ApplyWwiseVolumes();
    }

    private void OnEnable()
    {
        // 打开菜单 = 暂停游戏（记录原时间流速，关闭时恢复）
        prevTimeScale = Time.timeScale;
        Time.timeScale = 0f;

        if (musicSlider != null)
        {
            musicSlider.value = musicVolume;
            musicSlider.onValueChanged.AddListener(OnMusicChanged);
        }

        if (sfxSlider != null)
        {
            sfxSlider.value = sfxVolume;
            sfxSlider.onValueChanged.AddListener(OnSfxChanged);
        }

        if (brightnessSlider != null)
        {
            brightnessSlider.value = brightness;
            brightnessSlider.onValueChanged.AddListener(OnBrightnessChanged);
        }

        if (closeButton != null)
            closeButton.onClick.AddListener(Close);
    }

    private void OnDisable()
    {
        // 关闭菜单 = 恢复到打开前的时间流速
        Time.timeScale = prevTimeScale;

        if (musicSlider != null)
            musicSlider.onValueChanged.RemoveListener(OnMusicChanged);
        if (sfxSlider != null)
            sfxSlider.onValueChanged.RemoveListener(OnSfxChanged);
        if (brightnessSlider != null)
            brightnessSlider.onValueChanged.RemoveListener(OnBrightnessChanged);
        if (closeButton != null)
            closeButton.onClick.RemoveListener(Close);
    }

    public void OnMusicChanged(float value)
    {
        musicVolume = Mathf.Clamp01(value);
        PlayerPrefs.SetFloat(MusicPref, musicVolume);
        PlayerPrefs.Save();
        ApplyWwiseVolumes();
    }

    public void OnSfxChanged(float value)
    {
        sfxVolume = Mathf.Clamp01(value);
        PlayerPrefs.SetFloat(SfxPref, sfxVolume);
        PlayerPrefs.Save();
        ApplyWwiseVolumes();
    }

    public void OnBrightnessChanged(float value)
    {
        brightness = value;
        PlayerPrefs.SetFloat(BrightnessPref, value);
        PlayerPrefs.Save();
        ApplyBrightness();
    }

    /// <summary>把亮度值写到全屏遮罩上。0.5 = 不动，往左压黑，往右提白。</summary>
    private static void ApplyBrightness()
    {
        if (brightnessOverlay == null)
            BuildBrightnessOverlay();
        if (brightnessOverlay == null)
            return;

        if (brightness < 0.5f)
            brightnessOverlay.color = new Color(0f, 0f, 0f, (0.5f - brightness) * 1.4f);
        else
            brightnessOverlay.color = new Color(1f, 1f, 1f, (brightness - 0.5f) * 0.5f);
    }

    private static void BuildBrightnessOverlay()
    {
        if (!Application.isPlaying)
            return;

        var root = new GameObject("~BrightnessOverlay");
        Object.DontDestroyOnLoad(root);

        var canvas = root.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        // 压在所有 UI 上面，包括暂停菜单
        canvas.sortingOrder = 32760;

        var go = new GameObject("Overlay", typeof(RectTransform));
        go.transform.SetParent(root.transform, false);

        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        brightnessOverlay = go.AddComponent<Image>();
        // 千万不能吃点击，否则整个游戏都点不动了
        brightnessOverlay.raycastTarget = false;
        brightnessOverlay.color = new Color(0f, 0f, 0f, 0f);
    }

    private static void SubscribeToWwiseInitialization()
    {
        AkUnitySoundEngineInitialization initialization =
            AkUnitySoundEngineInitialization.Instance;

        initialization.initializationDelegate -= ApplyWwiseVolumes;
        initialization.initializationDelegate += ApplyWwiseVolumes;
        initialization.reInitializationDelegate -= ApplyWwiseVolumes;
        initialization.reInitializationDelegate += ApplyWwiseVolumes;
    }

    private static void ApplyWwiseVolumes()
    {
        if (!AkUnitySoundEngine.IsInitialized())
            return;

        if (wwiseAudioSettings == null)
        {
            wwiseAudioSettings = Resources.Load<WwiseUIFeedbackSettings>(
                WwiseUIFeedbackSettings.ResourcesPath);
        }

        if (wwiseAudioSettings == null)
        {
            if (!warnedMissingWwiseAudioSettings)
            {
                Debug.LogError(
                    "[SettingsManager] Missing Wwise UI audio settings; volume RTPCs were not applied.");
                warnedMissingWwiseAudioSettings = true;
            }
            return;
        }

        if (wwiseAudioSettings.HasValidMusicVolumeRtpc)
        {
            wwiseAudioSettings.MusicVolumeRtpc.SetGlobalValue(musicVolume * WwiseVolumeMax);
        }
        else if (!warnedMissingMusicVolumeRtpc)
        {
            Debug.LogWarning("[SettingsManager] MusicVolume RTPC is not configured.");
            warnedMissingMusicVolumeRtpc = true;
        }

        if (wwiseAudioSettings.HasValidSfxVolumeRtpc)
        {
            wwiseAudioSettings.SfxVolumeRtpc.SetGlobalValue(sfxVolume * WwiseVolumeMax);
        }
        else if (!warnedMissingSfxVolumeRtpc)
        {
            Debug.LogWarning("[SettingsManager] SFXVolume RTPC is not configured.");
            warnedMissingSfxVolumeRtpc = true;
        }
    }

    public void Close()
    {
        gameObject.SetActive(false);
        FormalParchmentAudio.PlayClose();
    }

    public void Open()
    {
        gameObject.SetActive(true);
        FormalParchmentAudio.PlayOpen();
    }
}
