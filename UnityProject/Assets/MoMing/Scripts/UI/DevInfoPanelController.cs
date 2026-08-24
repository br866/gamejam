using UnityEngine;

/// <summary>
/// 开发者信息面板开关。挂在始终激活的 Canvas 根物体上（和 SettingsPanelController 一样），
/// 因为面板自己在关闭状态下拿不到按钮回调。
/// 主菜单「开发者」按钮 OnClick -> ShowDevInfo；面板本身是一个全屏按钮，点一下 -> HideDevInfo。
/// </summary>
public class DevInfoPanelController : MonoBehaviour
{
    [SerializeField] private GameObject devInfoPanel;

    public void ShowDevInfo()
    {
        if (devInfoPanel != null)
            devInfoPanel.SetActive(true);
    }

    public void HideDevInfo()
    {
        if (devInfoPanel != null)
            devInfoPanel.SetActive(false);
    }

    public void ToggleDevInfo()
    {
        if (devInfoPanel != null)
            devInfoPanel.SetActive(!devInfoPanel.activeSelf);
    }
}
