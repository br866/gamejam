using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// One persistent 2D Wwise emitter for every Unity UI Button. It discovers
/// inactive menus on scene load and periodically covers runtime-created Buttons.
/// </summary>
[AddComponentMenu("MoMing/Audio/Wwise UI Feedback Router")]
[DisallowMultipleComponent]
[RequireComponent(typeof(AkGameObj))]
public sealed class WwiseUIFeedbackRouter : MonoBehaviour
{
    private const float RefreshIntervalSeconds = 0.5f;

    private static WwiseUIFeedbackRouter instance;

    [SerializeField] private WwiseUIFeedbackSettings settings;

    private float nextRefreshTime;
    private bool warnedMissingSettings;
    private bool warnedMissingHover;
    private bool warnedMissingClick;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void AutoSpawn()
    {
        if (instance != null || FindObjectOfType<WwiseUIFeedbackRouter>() != null)
            return;

        var host = new GameObject("Wwise UI Feedback (Auto)");
        host.AddComponent<WwiseUIFeedbackRouter>();
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        if (settings == null)
            settings = Resources.Load<WwiseUIFeedbackSettings>(WwiseUIFeedbackSettings.ResourcesPath);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        if (instance == this)
            instance = null;
    }

    private void Start()
    {
        int installed = InstallOnAllLoadedScenes();
        Debug.Log(
            "[WwiseUIFeedbackRouter] Installed feedback on " + installed + " UI Button(s).", this);
        nextRefreshTime = Time.unscaledTime + RefreshIntervalSeconds;
    }

    private void Update()
    {
        if (Time.unscaledTime < nextRefreshTime)
            return;

        nextRefreshTime = Time.unscaledTime + RefreshIntervalSeconds;
        InstallOnAllLoadedScenes();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        InstallInScene(scene);
    }

    private int InstallOnAllLoadedScenes()
    {
        int installed = 0;
        for (int i = 0; i < SceneManager.sceneCount; i++)
            installed += InstallInScene(SceneManager.GetSceneAt(i));
        return installed;
    }

    private int InstallInScene(Scene scene)
    {
        if (!scene.IsValid() || !scene.isLoaded)
            return 0;

        int installed = 0;
        GameObject[] roots = scene.GetRootGameObjects();
        for (int rootIndex = 0; rootIndex < roots.Length; rootIndex++)
        {
            Button[] buttons = roots[rootIndex].GetComponentsInChildren<Button>(true);
            for (int buttonIndex = 0; buttonIndex < buttons.Length; buttonIndex++)
                if (Install(buttons[buttonIndex]))
                    installed++;
        }
        return installed;
    }

    private bool Install(Button button)
    {
        if (button == null)
            return false;

        WwiseUIButtonFeedback feedback = button.GetComponent<WwiseUIButtonFeedback>();
        bool wasAdded = feedback == null;
        if (feedback == null)
            feedback = button.gameObject.AddComponent<WwiseUIButtonFeedback>();

        feedback.Initialize(this, button);
        return wasAdded;
    }

    internal void PostHover()
    {
        if (!TryGetSettings())
            return;

        if (!settings.HasValidHoverEvent)
        {
            if (!warnedMissingHover)
            {
                Debug.LogWarning("[WwiseUIFeedbackRouter] Play_UI_Hover is not assigned; Hover feedback is disabled.");
                warnedMissingHover = true;
            }
            return;
        }

        settings.HoverEvent.Post(gameObject);
    }

    internal void PostClick()
    {
        if (!TryGetSettings())
            return;

        if (!settings.HasValidClickEvent)
        {
            if (!warnedMissingClick)
            {
                Debug.LogWarning("[WwiseUIFeedbackRouter] Play_UI_Click is not assigned; Click feedback is disabled.");
                warnedMissingClick = true;
            }
            return;
        }

        settings.ClickEvent.Post(gameObject);
    }

    private bool TryGetSettings()
    {
        if (settings != null)
            return true;

        if (!warnedMissingSettings)
        {
            Debug.LogError(
                "[WwiseUIFeedbackRouter] Missing Resources/MoMing/WwiseUIFeedbackSettings. " +
                "Use MoMing > Audio > Refresh Wwise UI Feedback Settings in the Unity Editor.");
            warnedMissingSettings = true;
        }
        return false;
    }
}
