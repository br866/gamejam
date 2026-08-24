using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;

/// <summary>
/// 过场视频播放器：进场景 → 从黑幕淡入 → 播视频 → 播完（或玩家按键跳过）→ 淡出黑幕 → 加载下一个场景。
///
/// 用法：挂在过场场景里那个带 VideoPlayer 的物体上，
/// 把做好的视频文件拖进 VideoPlayer 的 Video Clip 槽位，
/// 再确认 nextSceneName 填的是下一个场景的名字（必须已经在 Build Settings 里）。
///
/// 兼容 Unity 2022.3。
/// </summary>
[RequireComponent(typeof(VideoPlayer))]
public class CutscenePlayer : MonoBehaviour
{
    [Header("播完之后进哪个场景")]
    [Tooltip("填场景名字，不带路径也不带 .unity。这个场景必须已经加进 File → Build Settings。")]
    public string nextSceneName = "superbreadman 1";

    [Header("视频")]
    [Tooltip("留空会自动找本物体上的 VideoPlayer")]
    public VideoPlayer videoPlayer;

    [Header("黑幕（淡入淡出，可以留空）")]
    public Image fadeImage;
    public float fadeInDuration = 0.6f;
    public float fadeOutDuration = 0.8f;

    [Header("跳过")]
    public bool allowSkip = true;
    [Tooltip("「按任意键跳过」那行字")]
    public GameObject skipHint;
    [Tooltip("过几秒之后才把提示显示出来，一上来就显得吵")]
    public float skipHintDelay = 1.5f;

    [Header("保险")]
    [Tooltip("视频没放进来、或者加载失败时，最多卡多少秒就自动进下一个场景。填 0 = 不自动跳。")]
    public float failSafeSeconds = 5f;

    bool leaving;
    float elapsed;

    void Awake()
    {
        // 从暂停菜单或设置面板过来时 timeScale 可能还停在 0，这里强制恢复，
        // 否则协程用 unscaledTime 能跑但视频不动，看起来像卡死。
        Time.timeScale = 1f;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.None;

        if (videoPlayer == null) videoPlayer = GetComponent<VideoPlayer>();
        if (skipHint != null) skipHint.SetActive(false);
    }

    void OnEnable()
    {
        if (videoPlayer == null) return;
        videoPlayer.loopPointReached += OnVideoFinished;
        videoPlayer.errorReceived += OnVideoError;
    }

    void OnDisable()
    {
        if (videoPlayer == null) return;
        videoPlayer.loopPointReached -= OnVideoFinished;
        videoPlayer.errorReceived -= OnVideoError;
    }

    IEnumerator Start()
    {
        if (fadeImage != null)
        {
            fadeImage.gameObject.SetActive(true);
            SetFadeAlpha(1f);
            yield return Fade(1f, 0f, fadeInDuration);
        }

        bool hasVideo = videoPlayer != null
                        && (videoPlayer.clip != null || !string.IsNullOrEmpty(videoPlayer.url));

        if (!hasVideo)
        {
            Debug.LogWarning("[过场] VideoPlayer 上没有视频。把视频文件拖进 Video Clip 槽位。现在直接进下一个场景。");
            Leave();
            yield break;
        }

        videoPlayer.Play();

        if (allowSkip && skipHint != null)
        {
            yield return new WaitForSecondsRealtime(skipHintDelay);
            if (!leaving) skipHint.SetActive(true);
        }
    }

    void Update()
    {
        elapsed += Time.unscaledDeltaTime;

        if (allowSkip && !leaving && Input.anyKeyDown)
        {
            Leave();
            return;
        }

        // 视频一直没播起来的话，别把玩家永远卡在黑屏里
        if (!leaving && failSafeSeconds > 0f && videoPlayer != null
            && !videoPlayer.isPlaying && elapsed > failSafeSeconds)
        {
            Debug.LogWarning("[过场] 视频 " + failSafeSeconds + " 秒内没能播起来，自动进下一个场景。");
            Leave();
        }
    }

    void OnVideoFinished(VideoPlayer vp)
    {
        Leave();
    }

    void OnVideoError(VideoPlayer vp, string message)
    {
        Debug.LogError("[过场] 视频播放出错：" + message);
        Leave();
    }

    /// <summary>也可以挂到「跳过」按钮的 OnClick 上。</summary>
    public void Leave()
    {
        if (leaving) return;
        leaving = true;
        if (skipHint != null) skipHint.SetActive(false);
        StartCoroutine(LeaveRoutine());
    }

    IEnumerator LeaveRoutine()
    {
        if (videoPlayer != null && videoPlayer.isPlaying)
            videoPlayer.Pause();

        if (fadeImage != null)
        {
            fadeImage.gameObject.SetActive(true);
            yield return Fade(fadeImage.color.a, 1f, fadeOutDuration);
        }

        if (string.IsNullOrEmpty(nextSceneName))
        {
            Debug.LogError("[过场] nextSceneName 是空的，不知道该去哪个场景。");
            yield break;
        }

        if (!Application.CanStreamedLevelBeLoaded(nextSceneName))
        {
            Debug.LogError("[过场] 场景 \"" + nextSceneName + "\" 不在 Build Settings 里，加载不了。"
                         + "去 File → Build Settings 把它加进去（或者点 Tools → 面包人 UI → ④ 配置场景流程）。");
            yield break;
        }

        SceneManager.LoadScene(nextSceneName);
    }

    IEnumerator Fade(float from, float to, float duration)
    {
        if (fadeImage == null) yield break;

        if (duration <= 0f)
        {
            SetFadeAlpha(to);
            yield break;
        }

        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            SetFadeAlpha(Mathf.Lerp(from, to, Mathf.Clamp01(t / duration)));
            yield return null;
        }
        SetFadeAlpha(to);
    }

    void SetFadeAlpha(float a)
    {
        if (fadeImage == null) return;
        Color c = fadeImage.color;
        c.a = a;
        fadeImage.color = c;
    }
}
