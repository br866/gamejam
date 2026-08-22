using UnityEngine;

/// <summary>
/// 脚印标记：只有操作狗时可见。事件驱动，无每帧轮询。
/// </summary>
public class FootprintMarker : MonoBehaviour
{
    [Header("Visual")]
    public Renderer markerRenderer;
    public bool visibleByDefault = false;

    private bool subscribedToPlayerManager;
    private FormalPlayerControl formalControl;

    void Start()
    {
        if (markerRenderer == null)
            markerRenderer = GetComponent<Renderer>();

        if (PlayerManager.Instance != null)
        {
            PlayerManager.Instance.OnActiveCharacterChanged += SetVisible;
            subscribedToPlayerManager = true;
            SetVisible(PlayerManager.Instance.IsActiveDog);
            return;
        }

        formalControl = FindObjectOfType<FormalPlayerControl>();
        if (formalControl != null)
        {
            formalControl.ActiveRoleChanged += SetVisible;
            SetVisible(formalControl.IsDogActive);
            return;
        }

        SetVisible(visibleByDefault);
    }

    void OnDestroy()
    {
        if (subscribedToPlayerManager)
        {
            if (PlayerManager.Instance != null)
                PlayerManager.Instance.OnActiveCharacterChanged -= SetVisible;
            return;
        }

        if (formalControl != null)
            formalControl.ActiveRoleChanged -= SetVisible;
    }

    void SetVisible(bool dogActive)
    {
        if (markerRenderer != null)
            markerRenderer.enabled = dogActive;
    }
}
