using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// 全局单例：焦虑值管理、UI 更新、关卡重置、关卡完成切换场景。
/// 焦虑系统：分离时 10s 倒计时填满；焦虑 ≥70% 时场景灯光熄灭，角色头顶聚光灯亮起（真实3D光影+阴影）。
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Players")]
    public Transform humanPlayer;
    public Transform dogPlayer;

    [Header("Anxiety")]
    public float maxAnxiety = 100f;
    public float anxietyIncreaseRate = 10f;
    public float anxietyDecreaseRate = 15f;

    [Header("Vignette (Permanent Camera Display Range)")]
    [Range(0f, 0.5f)] public float vignetteRadius = 0.35f;
    public float vignetteSoftness = 0.08f;
    public RawImage vignetteMask;

    [Header("Critical Darkness (3D Spotlights)")]
    [Range(0f, 1f)] public float criticalThreshold = 0.7f;
    public float spotlightTransitionSpeed = 3f;
    public RawImage spotlightMask; // legacy, hidden when critical
    public float spotlightHeight = 8f;
    public float spotlightRange = 6f;
    public float spotlightAngle = 20f;
    public float spotlightIntensity = 100f;
    public Color spotlightColor = new Color(1f, 0.95f, 0.8f, 1f);
    [Range(0f, 1f)] public float normalAmbientIntensity = 1f;
    [Range(0.01f, 0.3f)] public float fullDarknessRange = 0.1f;

    [Header("Red Anxiety Vignette")]
    [Range(0f, 1f)] public float redVignetteThreshold = 0.6f;
    public RawImage redVignette;
    public float redVignetteEdgeWidth = 0.06f;
    public Color redVignetteColor = new Color(0.55f, 0.08f, 0.08f, 0.5f);
    [Range(0f, 1f)] public float redVignetteNoiseStrength = 0.6f;

    [Header("UI References")]
    public Slider anxietyBarSlider;
    public Image darknessOverlay;

    [Header("Checkpoints")]
    public Vector3 levelStartHuman = new Vector3(0f, 1f, -3f);
    public Vector3 levelStartDog = new Vector3(1.5f, 1f, -3f);

    [Header("Scene Transition")]
    public string nextSceneName = "";

    [Header("Debug Commands")]
    public KeyCode teleportToLevel2Key = KeyCode.F2;
    public Vector3 debugLevel2HumanPosition = new Vector3(15.7f, 12.6f, -5.98f);
    public Vector3 debugLevel2DogPosition = new Vector3(17.2f, 12.6f, -5.98f);

    private float currentAnxiety = 0f;
    private bool isSeparated = false;
    private bool levelComplete = false;
    private Camera mainCam;
    private float currentOverlayAlpha = 0f;
    private float currentSpotlightIntensity = 0f;
    private float currentVignetteAlpha = 0f;
    private float currentRedVignetteAlpha = 0f;
    private bool hasCheckpoint = false;
    private Vector3 checkpointHuman;
    private Vector3 checkpointDog;
    private Light humanSpotlight;
    private Light dogSpotlight;
    private Light directionalLight;
    private float originalDirectionalIntensity;
    private float originalAmbientIntensity;

    public System.Action OnLevelReset;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        mainCam = Camera.main;

        // Permanently set camera to solid black background so the void outside rooms is always black
        if (mainCam != null)
        {
            mainCam.clearFlags = CameraClearFlags.SolidColor;
            mainCam.backgroundColor = Color.black;
        }
        // Permanently use Flat ambient mode so skybox doesn't leak light into the scene
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.8f, 0.8f, 0.85f, 1f);

        directionalLight = RenderSettings.sun;
        if (directionalLight == null)
        {
            var dirLights = FindObjectsOfType<Light>();
            foreach (var l in dirLights)
            {
                if (l.type == LightType.Directional)
                {
                    directionalLight = l;
                    break;
                }
            }
        }
        if (directionalLight != null)
            originalDirectionalIntensity = directionalLight.intensity;
        originalAmbientIntensity = normalAmbientIntensity;
        RenderSettings.ambientIntensity = normalAmbientIntensity;

        // Create 3D spotlights that follow each character
        humanSpotlight = CreateSpotlight("HumanSpotlight");
        dogSpotlight = CreateSpotlight("DogSpotlight");

        // Hide legacy 2D spotlight mask
        if (spotlightMask != null)
            spotlightMask.color = new Color(1, 1, 1, 0);

        SetupVignette();
        SetupRedVignette();
    }

    Light CreateSpotlight(string name)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(transform, false);
        Light light = go.AddComponent<Light>();
        light.type = LightType.Spot;
        light.range = spotlightRange;
        light.spotAngle = spotlightAngle;
        light.innerSpotAngle = 0f;
        light.intensity = 0f;
        light.color = spotlightColor;
        light.shadows = LightShadows.Soft;
        light.enabled = true;
        return light;
    }

    void SetupVignette()
    {
        if (vignetteMask == null)
        {
            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas == null) canvas = FindObjectOfType<Canvas>();
            if (canvas != null)
            {
                GameObject vigObj = new GameObject("VignetteMask");
                vigObj.transform.SetParent(canvas.transform, false);
                vignetteMask = vigObj.AddComponent<RawImage>();
                RectTransform rt = vigObj.GetComponent<RectTransform>();
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;
                vigObj.transform.SetSiblingIndex(0);
            }
        }

        int vigW = 512;
        int vigH = 288;
        Texture2D vigTex = new Texture2D(vigW, vigH, TextureFormat.RGBA32, false);
        vigTex.filterMode = FilterMode.Bilinear;
        Color32[] pixels = new Color32[vigW * vigH];

        for (int y = 0; y < vigH; y++)
        {
            float uy = (float)y / (vigH - 1);
            for (int x = 0; x < vigW; x++)
            {
                float ux = (float)x / (vigW - 1);
                // Rounded-rectangle vignette: 70% visible in both dimensions
                float dx = Mathf.Abs(ux - 0.5f) / vignetteRadius;
                float dy = Mathf.Abs(uy - 0.5f) / vignetteRadius;
                // Superellipse (power 4) for rounded rectangle shape
                float dist = Mathf.Pow(Mathf.Pow(dx, 4f) + Mathf.Pow(dy, 4f), 0.25f);

                float t = Mathf.Clamp01((dist - 1f) / vignetteSoftness);
                float lightFactor = t * t * (3f - 2f * t);
                byte alpha = (byte)(255 * lightFactor);

                int idx = y * vigW + x;
                pixels[idx] = new Color32(0, 0, 0, alpha);
            }
        }

        vigTex.SetPixels32(pixels);
        vigTex.Apply(false);

        if (vignetteMask != null)
        {
            vignetteMask.texture = vigTex;
            vignetteMask.color = new Color(1, 1, 1, 1);
        }
    }

    void SetupRedVignette()
    {
        if (redVignette == null)
        {
            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas == null) canvas = FindObjectOfType<Canvas>();
            if (canvas != null)
            {
                GameObject redObj = new GameObject("RedAnxietyVignette");
                redObj.transform.SetParent(canvas.transform, false);
                redVignette = redObj.AddComponent<RawImage>();
                RectTransform rt = redObj.GetComponent<RectTransform>();
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;
                // Place above darknessOverlay but below gameplay UI
                redObj.transform.SetSiblingIndex(1);
            }
        }

        int rW = 512;
        int rH = 288;
        Texture2D redTex = new Texture2D(rW, rH, TextureFormat.RGBA32, false);
        redTex.filterMode = FilterMode.Bilinear;
        Color32[] pixels = new Color32[rW * rH];

        // Pre-generate a noise lookup using simple hash-based pseudo-random
        // Multiple octaves for organic look
        float[] noiseMap = new float[rW * rH];
        for (int y = 0; y < rH; y++)
        {
            for (int x = 0; x < rW; x++)
            {
                float nx = (float)x / rW;
                float ny = (float)y / rH;
                // Multi-octave value noise via hash
                float n = 0f;
                n += HashNoise(nx * 6f, ny * 6f) * 0.5f;
                n += HashNoise(nx * 12f, ny * 12f) * 0.3f;
                n += HashNoise(nx * 24f, ny * 24f) * 0.2f;
                noiseMap[y * rW + x] = n;
            }
        }

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

                // Use noise to create patchy, organic coverage with actual gaps
                float noise = noiseMap[y * rW + x];
                // Threshold-based: noise creates holes in the red zone
                float patchyT = baseT - redVignetteNoiseStrength * (1f - noise) * baseT;
                patchyT = Mathf.Clamp01(patchyT);

                // Smoothstep
                float alpha = patchyT * patchyT * (3f - 2f * patchyT);
                alpha *= redVignetteColor.a;

                int idx = y * rW + x;
                pixels[idx] = new Color32(
                    (byte)(redVignetteColor.r * 255),
                    (byte)(redVignetteColor.g * 255),
                    (byte)(redVignetteColor.b * 255),
                    (byte)(alpha * 255)
                );
            }
        }

        redTex.SetPixels32(pixels);
        redTex.Apply(false);

        if (redVignette != null)
        {
            redVignette.texture = redTex;
            redVignette.color = new Color(1, 1, 1, 0);
        }
    }

    // Simple hash-based value noise for organic red vignette texture
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

    void Update()
    {
        if (levelComplete) return;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        if (Input.GetKeyDown(teleportToLevel2Key))
            TeleportPlayersToLevel2ForDebug();
#endif

        if (isSeparated)
            currentAnxiety += anxietyIncreaseRate * Time.deltaTime;
        else
            currentAnxiety -= anxietyDecreaseRate * Time.deltaTime;

        currentAnxiety = Mathf.Clamp(currentAnxiety, 0f, maxAnxiety);
        UpdateAnxietyUI();
        UpdateDarkness();

        if (currentAnxiety >= maxAnxiety)
            ResetLevel();
    }

    void UpdateAnxietyUI()
    {
        if (anxietyBarSlider != null)
            anxietyBarSlider.value = currentAnxiety / maxAnxiety;
    }

    void UpdateDarkness()
    {
        if (mainCam == null) return;

        float normalizedAnxiety = currentAnxiety / maxAnxiety;
        bool isCritical = normalizedAnxiety >= criticalThreshold;

        float targetOverlayAlpha;
        float targetSpotlightIntensity;
        float targetVignetteAlpha;

        if (isCritical)
        {
            // Critical: everything goes pitch black except spotlight pools on characters
            float darkAmount = Mathf.Clamp01((normalizedAnxiety - criticalThreshold) / fullDarknessRange);

            targetOverlayAlpha = 0f;
            targetSpotlightIntensity = spotlightIntensity;
            targetVignetteAlpha = 0f;

            // Drive ambient and directional to 0 — force exactly 0 when fully dark
            float effectiveAmbient = darkAmount >= 1f ? 0f : Mathf.Lerp(normalAmbientIntensity, 0f, darkAmount);
            RenderSettings.ambientIntensity = effectiveAmbient;
            RenderSettings.ambientLight = effectiveAmbient <= 0.01f ? Color.black : new Color(0.8f, 0.8f, 0.85f, 1f) * effectiveAmbient;
            RenderSettings.reflectionIntensity = effectiveAmbient <= 0.01f ? 0f : Mathf.Lerp(1f, 0f, darkAmount);
            if (directionalLight != null)
            {
                directionalLight.intensity = darkAmount >= 1f ? 0f : Mathf.Lerp(originalDirectionalIntensity, 0f, darkAmount);
                if (darkAmount >= 1f) directionalLight.enabled = false;
            }

            UpdateSpotlightPositions();
        }
        else
        {
            // Normal: rooms lit by directional light, void is black from camera background
            targetOverlayAlpha = normalizedAnxiety * 0.5f;
            targetSpotlightIntensity = 0f;
            targetVignetteAlpha = 0f;

            RenderSettings.ambientIntensity = normalAmbientIntensity;
            RenderSettings.ambientLight = new Color(0.8f, 0.8f, 0.85f, 1f);
            RenderSettings.reflectionIntensity = 1f;
            if (directionalLight != null)
            {
                directionalLight.intensity = originalDirectionalIntensity;
                directionalLight.enabled = true;
            }
        }

        currentOverlayAlpha = Mathf.Lerp(currentOverlayAlpha, targetOverlayAlpha, spotlightTransitionSpeed * Time.deltaTime);
        currentSpotlightIntensity = Mathf.Lerp(currentSpotlightIntensity, targetSpotlightIntensity, spotlightTransitionSpeed * Time.deltaTime);
        currentVignetteAlpha = Mathf.Lerp(currentVignetteAlpha, targetVignetteAlpha, spotlightTransitionSpeed * Time.deltaTime);

        if (darknessOverlay != null)
            darknessOverlay.color = new Color(0f, 0f, 0f, currentOverlayAlpha);

        if (spotlightMask != null)
            spotlightMask.color = new Color(1, 1, 1, 0f);

        if (vignetteMask != null)
            vignetteMask.color = new Color(1, 1, 1, currentVignetteAlpha);

        // Red anxiety vignette: appears above 60% anxiety, intensifies toward 100%
        float targetRedAlpha = 0f;
        if (normalizedAnxiety >= redVignetteThreshold)
        {
            // Map 60%→0, 100%→1, but reach ~0.8 by 80% for strong visual feedback
            float rawT = Mathf.Clamp01((normalizedAnxiety - redVignetteThreshold) / (1f - redVignetteThreshold));
            targetRedAlpha = Mathf.Clamp01(rawT * 1.5f);
        }
        currentRedVignetteAlpha = Mathf.Lerp(currentRedVignetteAlpha, targetRedAlpha, spotlightTransitionSpeed * Time.deltaTime);
        if (redVignette != null)
            redVignette.color = new Color(1f, 1f, 1f, currentRedVignetteAlpha);

        if (humanSpotlight != null)
            humanSpotlight.intensity = currentSpotlightIntensity;
        if (dogSpotlight != null)
            dogSpotlight.intensity = currentSpotlightIntensity;
    }

    void UpdateSpotlightPositions()
    {
        // Use camera forward direction for spotlight angle (matches camera's viewing angle)
        Vector3 lightDir = mainCam != null ? mainCam.transform.forward : Vector3.down;
        lightDir.y = -Mathf.Abs(lightDir.y); // ensure pointing downward

        if (humanSpotlight != null && humanPlayer != null)
        {
            humanSpotlight.transform.position = humanPlayer.position - lightDir * spotlightHeight;
            humanSpotlight.transform.rotation = Quaternion.LookRotation(lightDir);
        }
        if (dogSpotlight != null && dogPlayer != null)
        {
            dogSpotlight.transform.position = dogPlayer.position - lightDir * spotlightHeight;
            dogSpotlight.transform.rotation = Quaternion.LookRotation(lightDir);
        }
    }

    public void SetSeparated(bool separated)
    {
        isSeparated = separated;
    }

    public void ResetLevel()
    {
        currentAnxiety = 0f;
        currentSpotlightIntensity = 0f;
        currentOverlayAlpha = 0f;
        currentVignetteAlpha = 0f;
        currentRedVignetteAlpha = 0f;
        if (darknessOverlay != null)
            darknessOverlay.color = new Color(0, 0, 0, 0);
        if (spotlightMask != null)
            spotlightMask.color = new Color(1, 1, 1, 0);
        if (vignetteMask != null)
            vignetteMask.color = new Color(1, 1, 1, 0);
        if (redVignette != null)
            redVignette.color = new Color(1, 1, 1, 0);
        RenderSettings.ambientIntensity = normalAmbientIntensity;
        RenderSettings.ambientLight = new Color(0.8f, 0.8f, 0.85f, 1f);
        RenderSettings.reflectionIntensity = 1f;
        if (directionalLight != null)
        {
            directionalLight.intensity = originalDirectionalIntensity;
            directionalLight.enabled = true;
        }
        if (humanSpotlight != null)
            humanSpotlight.intensity = 0f;
        if (dogSpotlight != null)
            dogSpotlight.intensity = 0f;
        Vector3 respawnHuman = hasCheckpoint ? checkpointHuman : levelStartHuman;
        Vector3 respawnDog = hasCheckpoint ? checkpointDog : levelStartDog;
        ResetPlayerPositions(respawnHuman, respawnDog);
        OnLevelReset?.Invoke();
        Debug.Log("[GameManager] Level reset.");
    }

    public void OnPlayerCaught()
    {
        Vector3 respawnHuman = hasCheckpoint ? checkpointHuman : levelStartHuman;
        Vector3 respawnDog = hasCheckpoint ? checkpointDog : levelStartDog;
        ResetPlayerPositions(respawnHuman, respawnDog);
        currentAnxiety = Mathf.Min(currentAnxiety, maxAnxiety * 0.5f);
        Debug.Log("[GameManager] Player caught! Reset to start.");
    }

    public void OnLevelComplete()
    {
        if (levelComplete) return;
        levelComplete = true;
        Debug.Log("[GameManager] === Level Complete! ===");

        if (!string.IsNullOrEmpty(nextSceneName))
        {
            Debug.Log("[GameManager] Loading next scene: " + nextSceneName);
            SceneManager.LoadScene(nextSceneName);
        }
    }

    [ContextMenu("Teleport Players To Level2 (Debug)")]
    public void TeleportPlayersToLevel2ForDebug()
    {
        if (humanPlayer == null || dogPlayer == null)
        {
            Debug.LogWarning("[GameManager] Debug teleport requires both player references.");
            return;
        }

        ResetPlayerPositions(debugLevel2HumanPosition, debugLevel2DogPosition);
        currentAnxiety = 0f;
        isSeparated = false;
        Debug.Log("[GameManager] Debug teleport: Human and Dog moved to Level2.");
    }

    void ResetPlayerPositions(Vector3 humanPos, Vector3 dogPos)
    {
        if (humanPlayer != null)
        {
            humanPlayer.position = humanPos;
            var rb = humanPlayer.GetComponent<Rigidbody>();
            if (rb != null) { rb.velocity = Vector3.zero; rb.angularVelocity = Vector3.zero; }
        }
        if (dogPlayer != null)
        {
            dogPlayer.position = dogPos;
            var rb = dogPlayer.GetComponent<Rigidbody>();
            if (rb != null) { rb.velocity = Vector3.zero; rb.angularVelocity = Vector3.zero; }
        }
    }

    public float GetAnxietyNormalized()
    {
        return currentAnxiety / maxAnxiety;
    }

    /// <summary>
    /// 设置存档点复活位置。由 Checkpoint 脚本调用。
    /// </summary>
    public void SetCheckpoint(Vector3 humanPos, Vector3 dogPos)
    {
        checkpointHuman = humanPos;
        checkpointDog = dogPos;
        hasCheckpoint = true;
        Debug.Log("[GameManager] Checkpoint set. Human=" + humanPos + " Dog=" + dogPos);
    }

    public bool HasCheckpoint => hasCheckpoint;
}
