using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 音乐单例管理器：读取 CSV 音乐表，按 Id 点播，带交叉淡入淡出。
/// 还能按游戏状态（在一起 / 分离 / 焦虑高）自动切歌，做到“程序在合适的时机播放”。
///
/// 用法（编辑器里）：
///   1. 场景里建一个空物体，命名 MusicManager，挂上本脚本。
///   2. 把音频文件放进 Assets/MoMing/Resources/Audio/ 下（文件名要和 CSV 里的 ClipName 一致）。
///   3. CSV 放在 Assets/MoMing/Resources/MoMingConfig/music_table.csv。
///   4. 运行即可。原型阶段没有音频文件时，管理器会静默跳过、不报错。
///
/// 代码里手动点播：
///   MusicManager.Instance.PlayMusic("bgm_puzzle");
///   MusicManager.Instance.StopMusic();
///
/// 兼容 Unity 2022.3：淡入淡出用协程，不用 async/await；查找用 FindObjectOfType；用 AudioSource。
/// </summary>
public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance { get; private set; }

    [Header("表格来源（Resources 相对路径，不带扩展名）")]
    [Tooltip("对应 Assets/MoMing/Resources/MoMingConfig/music_table.csv")]
    public string tableResourcePath = "MoMingConfig/music_table";

    [Header("音频文件所在的 Resources 子目录（结尾带 /）")]
    [Tooltip("对应 Assets/MoMing/Resources/Audio/ ；CSV 里的 ClipName 就是这里的文件名")]
    public string clipResourceFolder = "Audio/";

    [Header("交叉淡入淡出")]
    [Tooltip("切歌时默认的淡变时长（秒）。单条配置的 FadeIn/FadeOut 会覆盖它。")]
    public float defaultFade = 1.5f;

    [Header("跨场景保留")]
    [Tooltip("勾选后切场景不打断音乐。若每个场景各自放一个 MusicManager，请不要勾。")]
    public bool dontDestroyOnLoad = false;

    [Header("按游戏状态自动切歌")]
    [Tooltip("勾选后：根据 PlayerManager(在一起/分离) 和 GameManager(焦虑值) 自动选曲")]
    public bool autoDriveByGameState = true;

    [Tooltip("在一起、且焦虑不高时播放的 Id（对应 CSV）")]
    public string idTogether = "bgm_puzzle";

    [Tooltip("刚分离、焦虑还不高时的过渡桥段 Id")]
    public string idSeparatedTransition = "bgm_separated_transition";

    [Tooltip("分离持续、焦虑升高后的分离状态 Id")]
    public string idSeparated = "bgm_separated";

    [Tooltip("焦虑达到阈值后叠加/切换到的焦虑氛围 Id")]
    public string idAnxiety = "bgm_anxiety";

    [Range(0f, 1f)]
    [Tooltip("焦虑归一化值 ≥ 该阈值时，进入分离状态曲")]
    public float separatedAnxietyThreshold = 0.35f;

    [Range(0f, 1f)]
    [Tooltip("焦虑归一化值 ≥ 该阈值时，进入焦虑氛围曲")]
    public float anxietyLayerThreshold = 0.6f;

    private MusicTable _table;
    private AudioSource _sourceA;
    private AudioSource _sourceB;
    private AudioSource _active;   // 当前正在响的那个
    private string _currentId = "";
    private Coroutine _fadeRoutine;
    // 已确认没有音频文件的 Id，记下来避免每帧重复 Resources.Load 和刷日志
    private readonly HashSet<string> _missingClips = new HashSet<string>();

    // “音乐”滑块驱动的 BGM 总音量(0~1)，只影响 BGM，不影响音效
    private float _musicVol = 1f;
    // 当前曲目的基础音量(来自 CSV)。最终音量 = _baseVol * _musicVol
    private float _baseVol = 1f;
    /// <summary>设置 BGM 总音量(0~1)，由 SettingsManager 的“音乐”滑块调用。</summary>
    public void SetMusicVolume(float v) { _musicVol = Mathf.Clamp01(v); }

    void Awake()
    {
        // 简单单例：跨场景保留时，防止重复实例
        if (dontDestroyOnLoad)
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            DontDestroyOnLoad(gameObject);
        }
        Instance = this;

        _sourceA = gameObject.AddComponent<AudioSource>();
        _sourceB = gameObject.AddComponent<AudioSource>();
        ConfigureSource(_sourceA);
        ConfigureSource(_sourceB);
        _active = _sourceA;

        LoadTable();
    }

    void ConfigureSource(AudioSource s)
    {
        s.playOnAwake = false;
        s.loop = true;
        s.volume = 0f;
        s.spatialBlend = 0f; // 2D，全局背景音
    }

    void LoadTable()
    {
        TextAsset csv = Resources.Load<TextAsset>(tableResourcePath);
        if (csv == null)
        {
            Debug.LogWarning("[MusicManager] 找不到音乐表：Resources/" + tableResourcePath +
                             ".csv（原型阶段没建也没关系，之后补上即可）。");
            _table = new MusicTable(); // 空表，避免空引用
            return;
        }
        _table = MusicTable.Parse(csv.text);
    }

    void Start()
    {
        // 读取“音乐”滑块的初始音量
        _musicVol = SettingsManager.MusicVolume;
        // 进场先播一首“在一起”的底噪（手动模式下也要有 BGM）
        PlayMusic(idTogether);
    }

    void Update()
    {
        if (autoDriveByGameState)
            DriveByGameState();

        // 应用“音乐”滑块的总音量到当前 BGM（非淡变期间稳定维持，滑块一动即时生效）
        if (_fadeRoutine == null && _active != null)
            _active.volume = _baseVol * _musicVol;
    }

    /// <summary>根据在一起/分离 + 焦虑值，决定该放哪首，并在需要时切歌。</summary>
    void DriveByGameState()
    {
        string desired = idTogether;

        bool together = PlayerManager.Instance == null || PlayerManager.Instance.IsTogether;
        float anxiety = GameManager.Instance != null ? GameManager.Instance.GetAnxietyNormalized() : 0f;

        if (!together)
        {
            if (anxiety >= anxietyLayerThreshold && _table.Contains(idAnxiety))
                desired = idAnxiety;
            else if (anxiety >= separatedAnxietyThreshold && _table.Contains(idSeparated))
                desired = idSeparated;
            else
                desired = idSeparatedTransition;
        }
        else
        {
            // 在一起，但焦虑还没完全降下来时，仍可用焦虑氛围过渡；否则回到底噪
            if (anxiety >= anxietyLayerThreshold && _table.Contains(idAnxiety))
                desired = idAnxiety;
            else
                desired = idTogether;
        }

        if (desired != _currentId)
            PlayMusic(desired);
    }

    /// <summary>按 Id 点播（带默认淡变）。找不到 Id 或没有音频文件时静默跳过。</summary>
    public void PlayMusic(string id)
    {
        PlayMusic(id, -1f);
    }

    /// <summary>按 Id 点播，指定淡变时长（fade &lt; 0 用配置或默认值）。</summary>
    public void PlayMusic(string id, float fade)
    {
        if (string.IsNullOrEmpty(id)) return;
        if (id == _currentId) return;
        if (_missingClips.Contains(id)) return; // 已知无文件，直接跳过，保持当前曲目继续播放

        MusicTable.Entry e = _table != null ? _table.Get(id) : null;
        if (e == null)
        {
            Debug.LogWarning("[MusicManager] 音乐表里没有 Id：" + id);
            return;
        }

        AudioClip clip = Resources.Load<AudioClip>(clipResourceFolder + e.clipName);
        if (clip == null)
        {
            // 原型阶段常见：还没有音频文件。记进黑名单后静默跳过。
            // 注意：这里故意不写 _currentId，否则“当前曲目”会被一首不存在的歌占住，
            // 导致状态切回来时把同一首 BGM 从头重放、两个 AudioSource 叠在一起。
            Debug.Log("[MusicManager] 暂无音频文件：Resources/" + clipResourceFolder + e.clipName +
                      "（Id=" + id + "），静默跳过。");
            _missingClips.Add(id);
            return;
        }

        _currentId = id;
        float fadeIn = fade >= 0f ? fade : e.fadeIn;
        float fadeOut = fade >= 0f ? fade : defaultFade;

        _baseVol = e.volume; // 记录本曲基础音量，供“音乐”滑块缩放
        AudioSource next = (_active == _sourceA) ? _sourceB : _sourceA;
        next.clip = clip;
        next.loop = e.loop;
        next.volume = 0f;
        next.Play();

        if (_fadeRoutine != null) StopCoroutine(_fadeRoutine);
        _fadeRoutine = StartCoroutine(CrossfadeRoutine(_active, next, e.volume, fadeIn, fadeOut));
        _active = next;
    }

    /// <summary>停止当前音乐（带淡出）。</summary>
    public void StopMusic()
    {
        StopMusic(defaultFade);
    }

    public void StopMusic(float fadeOut)
    {
        _currentId = "";
        if (_fadeRoutine != null) StopCoroutine(_fadeRoutine);
        _fadeRoutine = StartCoroutine(FadeOutRoutine(_active, fadeOut));
    }

    /// <summary>运行时开关自动模式（比如剧情演出时想手动接管）。</summary>
    public void SetAutoDrive(bool on)
    {
        autoDriveByGameState = on;
    }

    IEnumerator CrossfadeRoutine(AudioSource from, AudioSource to, float targetVol, float fadeIn, float fadeOut)
    {
        float fromStart = from != null ? from.volume : 0f;
        float t = 0f;
        float dur = Mathf.Max(fadeIn, fadeOut, 0.0001f);

        while (t < dur)
        {
            t += Time.unscaledDeltaTime; // 用 unscaled，暂停(Time.timeScale=0)时音乐仍能淡变
            float kIn = fadeIn <= 0f ? 1f : Mathf.Clamp01(t / fadeIn);
            float kOut = fadeOut <= 0f ? 1f : Mathf.Clamp01(t / fadeOut);
            if (to != null) to.volume = Mathf.Lerp(0f, targetVol * _musicVol, kIn);
            if (from != null) from.volume = Mathf.Lerp(fromStart, 0f, kOut);
            yield return null;
        }

        if (to != null) to.volume = targetVol * _musicVol;
        if (from != null)
        {
            from.volume = 0f;
            from.Stop();
            from.clip = null;
        }
        _fadeRoutine = null;
    }

    IEnumerator FadeOutRoutine(AudioSource src, float fadeOut)
    {
        if (src == null) yield break;
        float start = src.volume;
        float t = 0f;
        float dur = Mathf.Max(fadeOut, 0.0001f);
        while (t < dur)
        {
            t += Time.unscaledDeltaTime;
            src.volume = Mathf.Lerp(start, 0f, Mathf.Clamp01(t / dur));
            yield return null;
        }
        src.volume = 0f;
        src.Stop();
        src.clip = null;
        _fadeRoutine = null;
    }
}
