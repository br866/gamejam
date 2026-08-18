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

    [Header("Close Button")]
    [SerializeField] private Button closeButton;

    private const string MusicPref = "MusicVolume";
    private const string SfxPref = "SFXVolume";

    private static float musicVolume = 1f;
    private static float sfxVolume = 1f;
    private static bool initialized = false;

    // 记录打开菜单前的时间流速，关闭时恢复（避免和暂停菜单叠加时冲突）
    private float prevTimeScale = 1f;

    public static float MusicVolume => musicVolume;
    public static float SfxVolume => sfxVolume;

    private void Awake()
    {
        if (!initialized)
        {
            musicVolume = PlayerPrefs.GetFloat(MusicPref, 1f);
            sfxVolume = PlayerPrefs.GetFloat(SfxPref, 1f);
            initialized = true;
        }
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
