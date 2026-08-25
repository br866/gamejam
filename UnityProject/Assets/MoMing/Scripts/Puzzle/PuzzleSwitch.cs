using UnityEngine;

/// <summary>
/// 开关机关：仅人角色按 E 交互后激活，联动 GateController 开门。
/// </summary>
public class PuzzleSwitch : MonoBehaviour
{
    [Header("Linked Gate")]
    public GateController linkedGate;

    [Header("Visual")]
    public Renderer indicatorRenderer;
    public Material inactiveMaterial;
    public Material activeMaterial;

    [Header("Require Item")]
    public bool requiresItem = false;

    private bool isActivated = false;

    void Start()
    {
        if (indicatorRenderer != null && inactiveMaterial != null)
            indicatorRenderer.material = inactiveMaterial;

        if (GameManager.Instance != null)
            GameManager.Instance.OnLevelReset += Reset;
    }

    void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnLevelReset -= Reset;
    }

    public void Interact()
    {
        if (isActivated) return;

        // 需要持有道具的开关
        if (requiresItem)
        {
            var human = PlayerManager.Instance != null ? PlayerManager.Instance.human : null;
            if (human == null || !human.HasItem)
            {
                Debug.Log("[PuzzleSwitch] Requires item to activate!");
                return;
            }
        }

        isActivated = true;

        if (indicatorRenderer != null && activeMaterial != null)
            indicatorRenderer.material = activeMaterial;

        if (linkedGate != null)
            linkedGate.Open();

        Debug.Log("[PuzzleSwitch] " + gameObject.name + " activated!");
    }

    public void Reset()
    {
        isActivated = false;
        if (indicatorRenderer != null && inactiveMaterial != null)
            indicatorRenderer.material = inactiveMaterial;
        if (linkedGate != null)
            linkedGate.Close();
    }

    public bool IsActivated => isActivated;
}
