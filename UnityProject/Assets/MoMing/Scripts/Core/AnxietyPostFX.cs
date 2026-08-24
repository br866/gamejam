using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

/// <summary>
/// 根据焦虑值驱动 URP 的 Chromatic Aberration（色差）。
/// 焦虑值达到 caStartThreshold（默认 0.75，即进度条 3/4）之后，
/// Intensity 从 0 平滑上升到 caMaxIntensity（默认 1）。
///
/// 用法：
/// 1. 把这个脚本挂到场景里的 Light/Global Volume 上（或任意常驻物体）。
/// 2. Target Volume 留空会自动找场景里优先级最高的 Global Volume。
/// 3. Volume Profile 里必须已经 Add Override 了 Chromatic Aberration；
///    脚本只负责改数值，不会自动添加 Override。
///
/// 注意：运行时访问的是 volume.profile（Unity 会自动克隆一份运行时副本），
/// 所以退出 Play 模式后不会把 Profile 资源改脏。
///
/// 镜头污渍（Bloom Lens Dirt / Dirt Overlay）由 GameManager 负责，这里不碰。
/// </summary>
[AddComponentMenu("MoMing/Anxiety Post FX")]
public class AnxietyPostFX : MonoBehaviour
{
    [Header("Volume")]
    [Tooltip("留空则自动查找场景中优先级最高的 Global Volume")]
    public Volume targetVolume;

    [Header("Chromatic Aberration 色差")]
    [Tooltip("焦虑归一化值超过这个阈值后才开始出现色差。0.75 = 进度条 3/4")]
    [Range(0f, 1f)] public float caStartThreshold = 0.75f;
    [Tooltip("焦虑 100% 时的色差强度")]
    [Range(0f, 1f)] public float caMaxIntensity = 1f;
    [Tooltip("阈值→满值之间的变化曲线，默认线性；改成 EaseInOut 会让后段更猛")]
    public AnimationCurve caCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

    [Header("Pulse 心跳式脉动（可选）")]
    [Tooltip("让色差像心跳一样轻微起伏，比死值更有压迫感。默认关闭 = 严格 0→1")]
    public bool pulse = false;
    [Tooltip("每秒脉动次数")]
    public float pulseSpeed = 1.6f;
    [Tooltip("脉动幅度：0.25 = 在 75%~100% 之间起伏")]
    [Range(0f, 1f)] public float pulseAmount = 0.25f;

    [Header("Smoothing 平滑")]
    [Tooltip("数值追赶速度，越大越跟手；0 = 不平滑")]
    public float smoothSpeed = 4f;
    [Tooltip("使用 unscaledDeltaTime，暂停 / 慢动作时也能正常过渡")]
    public bool useUnscaledTime = true;

    [Header("Fallback / Debug")]
    [Tooltip("找不到 GameManager 时，从这个 Slider 读取焦虑值（0~1）")]
    public Slider anxietyBarSlider;
    [Tooltip("勾上后用下面的 debugValue 代替真实焦虑值，方便在 Play 模式里拖着看效果")]
    public bool useDebugValue = false;
    [Range(0f, 1f)] public float debugValue = 0f;

    private ChromaticAberration _ca;
    private float _caCurrent;

    void OnEnable()
    {
        ResolveVolume();
        CacheOverrides();
        _caCurrent = 0f;
    }

    void ResolveVolume()
    {
        if (targetVolume != null) return;

        Volume[] volumes = FindObjectsOfType<Volume>();
        float bestPriority = float.NegativeInfinity;
        foreach (Volume v in volumes)
        {
            if (v == null || !v.isGlobal || v.sharedProfile == null) continue;
            if (v.priority > bestPriority)
            {
                bestPriority = v.priority;
                targetVolume = v;
            }
        }

        if (targetVolume == null)
            Debug.LogWarning("[AnxietyPostFX] 场景里没找到 Global Volume，脚本不会生效。", this);
    }

    void CacheOverrides()
    {
        _ca = null;
        if (targetVolume == null) return;

        // volume.profile 会自动克隆一份运行时实例，不会污染 Profile 资源
        VolumeProfile profile = targetVolume.profile;
        if (profile == null) return;

        if (!profile.TryGet(out _ca))
            Debug.LogWarning("[AnxietyPostFX] Profile 里没有 Chromatic Aberration，请先 Add Override。", this);
    }

    /// <summary>取当前焦虑值（0~1）。</summary>
    public float GetAnxiety01()
    {
        if (useDebugValue) return debugValue;
        // 正式关卡优先：FormalAnxietyState 是正式关的焦虑数据源
        if (FormalAnxietyState.Instance != null) return Mathf.Clamp01(FormalAnxietyState.Instance.Normalized);
        if (GameManager.Instance != null) return Mathf.Clamp01(GameManager.Instance.GetAnxietyNormalized());
        if (anxietyBarSlider != null) return Mathf.Clamp01(anxietyBarSlider.normalizedValue);
        return 0f;
    }

    void LateUpdate()
    {
        if (_ca == null) return;

        float anxiety = GetAnxiety01();

        // 阈值以下为 0，阈值→1 之间映射到 0→1
        float t = caStartThreshold < 1f
            ? Mathf.Clamp01((anxiety - caStartThreshold) / (1f - caStartThreshold))
            : (anxiety >= 1f ? 1f : 0f);

        float shaped = Mathf.Clamp01(caCurve.Evaluate(t));

        // 脉动
        float pulseMul = 1f;
        if (pulse && pulseAmount > 0f)
        {
            float phase = (useUnscaledTime ? Time.unscaledTime : Time.time) * pulseSpeed * Mathf.PI * 2f;
            float wave = 0.5f + 0.5f * Mathf.Sin(phase);          // 0~1
            pulseMul = Mathf.Lerp(1f - pulseAmount, 1f, wave);
        }

        float dt = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
        float target = Mathf.Clamp01(shaped * caMaxIntensity * pulseMul);

        _caCurrent = smoothSpeed > 0f
            ? Mathf.Lerp(_caCurrent, target, 1f - Mathf.Exp(-smoothSpeed * dt))
            : target;

        _ca.active = true;
        _ca.intensity.overrideState = true;
        _ca.intensity.value = _caCurrent;
    }
}
