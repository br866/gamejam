using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 正式关卡的焦虑屏幕表现：镜头污渍 + 红色暗角。
///
/// 这两个效果原本写在旧的 GameManager 里，而 GameManager 只存在于
/// PlayerSystem.prefab（老的 Level1 / Level2 场景才有），
/// 正式关卡没有它，所以污渍和红晕一直没出现。
/// 这里把那部分逻辑原样搬过来，数据源换成 FormalAnxietyState。
///
/// 挂载位置：FormalPersistent 场景的 FormalUI Canvas 上（或任意常驻物体）。
/// 遮罩会自己建到 targetCanvas 底下，并且插在最前面几个 sibling，
/// 保证压在 HUD 下面、不挡暂停菜单。
///
/// 色差（眩晕）不在这里，那是 AnxietyPostFX 干的活。
/// </summary>
[AddComponentMenu("MoMing/Formal Anxiety Overlay")]
public class FormalAnxietyOverlay : MonoBehaviour
{
    [Header("挂载到哪个 Canvas")]
    [Tooltip("留空就用本物体上的 Canvas，再没有就在场景里找一个")]
    public Canvas targetCanvas;

    [Header("镜头污渍")]
    public bool useDirtOverlay = true;
    [Tooltip("留空则从 Resources 加载下面那个名字")]
    public Texture dirtOverlayTexture;
    public string dirtOverlayResourceName = "AnxietyDirtMask";
    [Tooltip("焦虑低于这个值不出污渍")]
    [Range(0f, 1f)] public float dirtStartThreshold = 0.5f;
    [Range(0f, 2f)] public float dirtOverlayMaxStrength = 0.85f;
    public float dirtSmoothSpeed = 4f;
    [Range(0f, 1f)] public float dirtPulseAmount = 0.22f;
    public float dirtPulseSpeed = 1.6f;
    [Tooltip("按住这个键强制拉满污渍，方便调效果")]
    public KeyCode forceMaxDirtKey = KeyCode.F3;

    [Header("红色暗角")]
    public bool useRedVignette = true;
    [Range(0f, 1f)] public float redVignetteThreshold = 0.6f;
    public float redVignetteEdgeWidth = 0.06f;
    public Color redVignetteColor = new Color(0.55f, 0.08f, 0.08f, 0.5f);
    [Range(0f, 1f)] public float redVignetteNoiseStrength = 0.6f;
    public float redVignetteFadeSpeed = 2f;

    private RawImage dirtOverlay;
    private RawImage redVignette;
    private float currentDirtStrength;
    private float currentRedAlpha;

    void Start()
    {
        if (targetCanvas == null) targetCanvas = GetComponentInParent<Canvas>();
        if (targetCanvas == null) targetCanvas = FindObjectOfType<Canvas>();

        if (targetCanvas == null)
        {
            Debug.LogWarning("[FormalAnxietyOverlay] 找不到 Canvas，污渍和红晕都出不来。", this);
            enabled = false;
            return;
        }

        if (useRedVignette) SetupRedVignette();
        if (useDirtOverlay) SetupDirtOverlay();
    }

    void LateUpdate()
    {
        float anxiety = FormalAnxietyState.Instance != null
            ? Mathf.Clamp01(FormalAnxietyState.Instance.Normalized)
            : 0f;

        UpdateDirtOverlay(anxiety);
        UpdateRedVignette(anxiety);
    }

    // ---------- 污渍 ----------

    RawImage NewFullScreenLayer(string objName, int siblingIndex)
    {
        GameObject go = new GameObject(objName);
        go.transform.SetParent(targetCanvas.transform, false);

        RawImage img = go.AddComponent<RawImage>();
        img.raycastTarget = false;

        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        go.transform.SetSiblingIndex(siblingIndex);
        img.color = new Color(1f, 1f, 1f, 0f);
        return img;
    }

    void SetupDirtOverlay()
    {
        if (dirtOverlayTexture == null && !string.IsNullOrEmpty(dirtOverlayResourceName))
            dirtOverlayTexture = Resources.Load<Texture2D>(dirtOverlayResourceName);

        if (dirtOverlayTexture == null)
        {
            Debug.LogWarning("[FormalAnxietyOverlay] 污渍贴图没找到：Resources/"
                             + dirtOverlayResourceName + " 不存在。", this);
            return;
        }

        dirtOverlay = NewFullScreenLayer("AnxietyDirtOverlay", 1);
        dirtOverlay.texture = dirtOverlayTexture;

        // 污渍要用加色混合，不然会把画面压暗
        Shader additive = Resources.Load<Shader>("MoMingUIAdditive");
        if (additive == null) additive = Shader.Find("MoMing/UI Additive");
        if (additive != null)
            dirtOverlay.material = new Material(additive);
        else
            Debug.LogWarning("[FormalAnxietyOverlay] 找不到 MoMing/UI Additive shader，"
                             + "污渍会走默认 alpha 混合，看起来偏暗。", this);
    }

    void UpdateDirtOverlay(float anxiety)
    {
        if (dirtOverlay == null) return;

        bool forced = Input.GetKey(forceMaxDirtKey);

        float t = dirtStartThreshold < 1f
            ? Mathf.Clamp01((anxiety - dirtStartThreshold) / (1f - dirtStartThreshold))
            : (anxiety >= 1f ? 1f : 0f);
        if (forced) t = 1f;

        float pulseMul = 1f;
        if (dirtPulseAmount > 0f)
        {
            float phase = Time.time * dirtPulseSpeed * Mathf.PI * 2f;
            float wave = 0.5f + 0.5f * Mathf.Sin(phase);
            pulseMul = Mathf.Lerp(1f - dirtPulseAmount, 1f, wave);
        }

        float target = dirtOverlayMaxStrength * t * pulseMul;
        float k = (forced || dirtSmoothSpeed <= 0f)
            ? 1f
            : 1f - Mathf.Exp(-dirtSmoothSpeed * Time.deltaTime);

        currentDirtStrength = Mathf.Lerp(currentDirtStrength, target, k);
        dirtOverlay.color = new Color(1f, 1f, 1f, currentDirtStrength);
    }

    // ---------- 红色暗角 ----------

    void SetupRedVignette()
    {
        redVignette = NewFullScreenLayer("RedAnxietyVignette", 0);

        const int rW = 512;
        const int rH = 288;

        var redTex = new Texture2D(rW, rH, TextureFormat.RGBA32, false);
        redTex.filterMode = FilterMode.Bilinear;

        // 先生成多倍频噪声，让红边有机、有缺口，而不是一圈均匀的红
        float[] noiseMap = new float[rW * rH];
        for (int y = 0; y < rH; y++)
        {
            for (int x = 0; x < rW; x++)
            {
                float nx = (float)x / rW;
                float ny = (float)y / rH;
                float n = 0f;
                n += HashNoise(nx * 6f, ny * 6f) * 0.5f;
                n += HashNoise(nx * 12f, ny * 12f) * 0.3f;
                n += HashNoise(nx * 24f, ny * 24f) * 0.2f;
                noiseMap[y * rW + x] = n;
            }
        }

        var pixels = new Color32[rW * rH];
        for (int y = 0; y < rH; y++)
        {
            float uy = (float)y / (rH - 1);
            for (int x = 0; x < rW; x++)
            {
                float ux = (float)x / (rW - 1);
                float dx = Mathf.Abs(ux - 0.5f);
                float dy = Mathf.Abs(uy - 0.5f);
                float maxDist = Mathf.Max(dx, dy);

                float edge = 0.5f - redVignetteEdgeWidth;
                float baseT = Mathf.Clamp01((maxDist - edge) / redVignetteEdgeWidth);

                float noise = noiseMap[y * rW + x];
                float patchyT = Mathf.Clamp01(baseT - redVignetteNoiseStrength * (1f - noise) * baseT);

                float alpha = patchyT * patchyT * (3f - 2f * patchyT) * redVignetteColor.a;

                pixels[y * rW + x] = new Color32(
                    (byte)(redVignetteColor.r * 255f),
                    (byte)(redVignetteColor.g * 255f),
                    (byte)(redVignetteColor.b * 255f),
                    (byte)(alpha * 255f));
            }
        }

        redTex.SetPixels32(pixels);
        redTex.Apply(false);
        redVignette.texture = redTex;
    }

    void UpdateRedVignette(float anxiety)
    {
        if (redVignette == null) return;

        float target = 0f;
        if (anxiety >= redVignetteThreshold && redVignetteThreshold < 1f)
        {
            float rawT = Mathf.Clamp01((anxiety - redVignetteThreshold) / (1f - redVignetteThreshold));
            // ×1.5 让 80% 左右就已经很明显，和旧 GameManager 保持一致
            target = Mathf.Clamp01(rawT * 1.5f);
        }

        currentRedAlpha = Mathf.Lerp(currentRedAlpha, target, redVignetteFadeSpeed * Time.deltaTime);
        redVignette.color = new Color(1f, 1f, 1f, currentRedAlpha);
    }

    // ---------- 噪声（照搬旧 GameManager） ----------

    float HashNoise(float x, float y)
    {
        int ix = Mathf.FloorToInt(x);
        int iy = Mathf.FloorToInt(y);
        float fx = x - ix;
        float fy = y - iy;

        float a = Hash2D(ix, iy);
        float b = Hash2D(ix + 1, iy);
        float c = Hash2D(ix, iy + 1);
        float d = Hash2D(ix + 1, iy + 1);

        float ux = fx * fx * (3f - 2f * fx);
        float uy = fy * fy * (3f - 2f * fy);

        return Mathf.Lerp(Mathf.Lerp(a, b, ux), Mathf.Lerp(c, d, ux), uy);
    }

    float Hash2D(int x, int y)
    {
        uint h = (uint)(x * 374761393 + y * 668265263);
        h = (h ^ (h >> 13)) * 1274126177u;
        h = h ^ (h >> 16);
        return (float)(h & 0xFFFFFF) / 0xFFFFFF;
    }
}
