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

    private static float musicVolume = 1f;
    private static float sfxVolume = 1f;
    // 0.5 = 原样，往左变暗，往右变亮
    private static float brightness = 0.5f;
    private static bool initialized = false;

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
    }

    private static void LoadPrefs()
    {
        if (initialized)
            return;

        musicVolume = PlayerPrefs.GetFloat(MusicPref, 1f);
        sfxVolume = PlayerPrefs.GetFloat(SfxPref, 1f);
        brightness = PlayerPrefs.GetFloat(BrightnessPref, 0.5f);
        initialized = true;
    }

    private void Awake()
    {
        LoadPrefs();
        ApplyBrightness();
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
        musicVolume = value;
        PlayerPrefs.SetFloat(MusicPref, value);
        PlayerPrefs.Save();
        ApplyVolumes();
    }

    public void OnSfxChanged(float value)
    {
        sfxVolume = value;
        PlayerPrefs.SetFloat(SfxPref, value);
        PlayerPrefs.Save();
        ApplyVolumes();
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

    private static void ApplyVolumes()
    {
        // BGM 与音效分开：AudioListener 作为总开关保持满音量，
        // “音乐”滑块只驱动 MusicManager 的 BGM，“音效”滑块在播放音效时按 SfxVolume 缩放。
        AudioListener.volume = 1f;
        if (MusicManager.Instance != null)
            MusicManager.Instance.SetMusicVolume(musicVolume);
    }

    /// <summary>统一播放音效入口：自动按“音效”滑块(SfxVolume)缩放。所有 SFX 都应走这里。</summary>
    public static void PlaySfx(AudioSource source, AudioClip clip)
    {
        if (source != null && clip != null)
            source.PlayOneShot(clip, sfxVolume);
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }

    public void Open()
    {
        gameObject.SetActive(true);
    }
}
