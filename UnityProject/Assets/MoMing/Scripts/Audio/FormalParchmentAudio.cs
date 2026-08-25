using UnityEngine;

/// <summary>
/// Shared Wwise playback point for parchment-style formal UI. Future notice
/// boards can reuse PlayOpen and PlayClose instead of storing Event names.
/// </summary>
[AddComponentMenu("MoMing/Audio/Formal Parchment Audio")]
[RequireComponent(typeof(AkGameObj))]
public sealed class FormalParchmentAudio : MonoBehaviour
{
    [SerializeField] private AK.Wwise.Event openEvent = new AK.Wwise.Event();
    [SerializeField] private AK.Wwise.Event closeEvent = new AK.Wwise.Event();

    public static FormalParchmentAudio Instance { get; private set; }

    private bool warnedMissingOpenEvent;
    private bool warnedMissingCloseEvent;

    void Awake()
    {
        Instance = this;
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public static void PlayOpen()
    {
        if (Instance != null)
            Instance.Post(Instance.openEvent, true);
    }

    public static void PlayClose()
    {
        if (Instance != null)
            Instance.Post(Instance.closeEvent, false);
    }

    void Post(AK.Wwise.Event uiEvent, bool opening)
    {
        if (uiEvent != null && uiEvent.IsValid())
        {
            uiEvent.Post(gameObject);
            return;
        }

        if (opening && !warnedMissingOpenEvent)
        {
            Debug.LogWarning("[FormalParchmentAudio] Play_UI_Parchment_Open is not assigned.", this);
            warnedMissingOpenEvent = true;
        }
        else if (!opening && !warnedMissingCloseEvent)
        {
            Debug.LogWarning("[FormalParchmentAudio] Play_UI_Parchment_Close is not assigned.", this);
            warnedMissingCloseEvent = true;
        }
    }
}
