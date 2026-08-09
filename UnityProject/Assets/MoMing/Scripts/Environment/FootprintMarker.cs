using UnityEngine;

/// <summary>
/// 脚印标记：只有操作狗时可见。
/// </summary>
public class FootprintMarker : MonoBehaviour
{
    [Header("Visual")]
    public Renderer markerRenderer;
    public bool visibleByDefault = false;

    void Start()
    {
        if (markerRenderer == null)
            markerRenderer = GetComponent<Renderer>();

        SetVisible(visibleByDefault);

        if (PlayerManager.Instance != null)
        {
            PlayerManager.Instance.OnActiveCharacterChanged += SetVisible;
            SetVisible(PlayerManager.Instance.IsActiveDog);
        }
    }

    void OnDestroy()
    {
        if (PlayerManager.Instance != null)
            PlayerManager.Instance.OnActiveCharacterChanged -= SetVisible;
    }

    void SetVisible(bool dogActive)
    {
        if (markerRenderer != null)
            markerRenderer.enabled = dogActive;
    }
}
