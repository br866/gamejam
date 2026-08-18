using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 音效路径常量。对应 Assets/Audio/Resources/SFX/ 下的文件（不带扩展名）。
/// 改了素材文件名，只要同步改这里就行，调用方不用动。
/// </summary>
public static class Sfx
{
    // --- 交互物 ---
    public const string KeyPickup      = "SFX/InteractiveObj/Keys/key_pickup";
    public const string KeyThrow       = "SFX/InteractiveObj/Keys/key_throw";
    public const string CratePushStart = "SFX/InteractiveObj/WoodenCrate&Stool/WoodenCrate_Push_Start";
    public const string CratePushLoop  = "SFX/InteractiveObj/WoodenCrate&Stool/WoodenCrate_Push_Loop";
    public const string CratePushStop  = "SFX/InteractiveObj/WoodenCrate&Stool/WoodenCrate_Push_Stop";
    public const string Scroll         = "SFX/InteractiveObj/ParchmentScroll-SFX";

    // --- UI ---
    public const string SettingsOpen   = "SFX/UI/Settings_Open_Click_Arp";
    public const string SettingsClose  = "SFX/UI/Settings_Close_Click_Arp";

    // --- 脚步（多变体，用 PlayRandom + 前缀）---
    public const string FootstepFolder = "SFX/Footsteps";
    public const string HumanCarpet    = "Human_Footstep_SoftBoot_LowPileCarpet";
    public const string HumanTile      = "Human_Footstep_SoftBoot_WaxedTile";
    public const string DogCarpet      = "Dog_Footstep_Claws_LowPileCarpet";
    public const string DogTile        = "Dog_Footstep_Claws_WaxedTile";
    public const string MonsterStep    = "Brutedoc_Footstep";
}

/// <summary>
/// 音效单例：按路径从 Resources 加载并播放，不需要在 Inspector 里拖任何引用。
///
/// 用法：
///   SfxManager.Play(Sfx.SettingsOpen);                              // 2D，UI 用
///   SfxManager.PlayAt(Sfx.KeyPickup, transform.position);           // 3D，有距离衰减
///   SfxManager.PlayRandom(Sfx.FootstepFolder, Sfx.HumanTile, pos);  // 多变体随机挑一个
///
/// 素材必须放在某个 Resources 文件夹下，当前是 Assets/Audio/Resources/SFX/。
/// 找不到文件时静默跳过并只打一次日志，不会报错、不会刷屏。
/// 兼容 Unity 2022.3。
/// </summary>
public class SfxManager : MonoBehaviour
{
    public static SfxManager Instance { get; private set; }

    [Tooltip("同时最多能叠几个音效")]
    public int poolSize = 12;

    [Tooltip("3D 音效的最大可闻距离")]
    public float max3DDistance = 25f;

    private AudioSource[] _pool;
    private int _next;

    private readonly Dictionary<string, AudioClip> _clips = new Dictionary<string, AudioClip>();
    private readonly Dictionary<string, AudioClip[]> _groups = new Dictionary<string, AudioClip[]>();
    private readonly HashSet<string> _missing = new HashSet<string>();

    /// <summary>没有场景手动挂载时自动生成一个，跨场景保留。跟 MusicManager 同一套路。</summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void AutoSpawn()
    {
        if (Instance != null) return;
        if (FindObjectOfType<SfxManager>() != null) return;

        GameObject go = new GameObject("SfxManager (Auto)");
        go.AddComponent<SfxManager>();
        DontDestroyOnLoad(go);
    }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (poolSize < 1) poolSize = 1;
        _pool = new AudioSource[poolSize];
        for (int i = 0; i < poolSize; i++)
        {
            GameObject child = new GameObject("SfxSource_" + i);
            child.transform.SetParent(transform, false);
            AudioSource s = child.AddComponent<AudioSource>();
            s.playOnAwake = false;
            s.loop = false;
            s.rolloffMode = AudioRolloffMode.Linear;
            s.maxDistance = max3DDistance;
            _pool[i] = s;
        }
    }

    // ---------- 对外 API ----------

    /// <summary>2D 播放（UI、全局提示音）。</summary>
    public static void Play(string path, float volume = 1f)
    {
        if (Instance == null) return;
        Instance.PlayInternal(Instance.Load(path), Vector3.zero, false, volume);
    }

    /// <summary>3D 播放，带距离衰减。</summary>
    public static void PlayAt(string path, Vector3 position, float volume = 1f)
    {
        if (Instance == null) return;
        Instance.PlayInternal(Instance.Load(path), position, true, volume);
    }

    /// <summary>从某文件夹里挑一个以 prefix 开头的音频随机播放（脚步声这种多变体素材）。</summary>
    public static void PlayRandom(string folder, string prefix, Vector3 position, float volume = 1f)
    {
        if (Instance == null) return;
        AudioClip[] group = Instance.LoadGroup(folder, prefix);
        if (group == null || group.Length == 0) return;
        AudioClip clip = group[Random.Range(0, group.Length)];
        Instance.PlayInternal(clip, position, true, volume);
    }

    /// <summary>直接播放一个已有的 AudioClip（Inspector 里拖好的素材）。</summary>
    public static void PlayClipAt(AudioClip clip, Vector3 position, float volume = 1f)
    {
        if (Instance == null || clip == null) return;
        Instance.PlayInternal(clip, position, true, volume);
    }

    /// <summary>取一个 clip（需要自己接 AudioSource 循环播放时用，比如推箱子的 Loop）。</summary>
    public static AudioClip GetClip(string path)
    {
        return Instance == null ? null : Instance.Load(path);
    }

    // ---------- 内部 ----------

    void PlayInternal(AudioClip clip, Vector3 pos, bool spatial, float volume)
    {
        if (clip == null || _pool == null) return;

        AudioSource s = _pool[_next];
        _next = (_next + 1) % _pool.Length;

        s.transform.position = spatial ? pos : Vector3.zero;
        s.spatialBlend = spatial ? 1f : 0f;
        s.volume = Mathf.Clamp01(volume);
        s.clip = clip;
        s.Play();
    }

    AudioClip Load(string path)
    {
        if (string.IsNullOrEmpty(path)) return null;
        if (_missing.Contains(path)) return null;

        AudioClip cached;
        if (_clips.TryGetValue(path, out cached)) return cached;

        AudioClip clip = Resources.Load<AudioClip>(path);
        if (clip == null)
        {
            Debug.Log("[SfxManager] 暂无音效文件：Resources/" + path + "，静默跳过。");
            _missing.Add(path);
            return null;
        }
        _clips[path] = clip;
        return clip;
    }

    AudioClip[] LoadGroup(string folder, string prefix)
    {
        string key = folder + "|" + prefix;
        AudioClip[] cached;
        if (_groups.TryGetValue(key, out cached)) return cached;

        AudioClip[] all = Resources.LoadAll<AudioClip>(folder);
        List<AudioClip> hits = new List<AudioClip>();
        for (int i = 0; i < all.Length; i++)
        {
            if (all[i] != null && all[i].name.StartsWith(prefix))
                hits.Add(all[i]);
        }

        if (hits.Count == 0)
            Debug.Log("[SfxManager] Resources/" + folder + " 下没有以 " + prefix + " 开头的音效，静默跳过。");

        AudioClip[] arr = hits.ToArray();
        _groups[key] = arr;
        return arr;
    }
}
