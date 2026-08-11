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
        // 简易方案：用 AudioListener.volume 做总音量，后续接入 AudioMixer 时替换
        // 音乐和音效取平均值作为主音量（原型阶段，后续可拆分 AudioMixer Group）
        AudioListener.volume = (musicVolume + sfxVolume) * 0.5f;
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
