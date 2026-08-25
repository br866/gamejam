using UnityEngine;

/// <summary>
/// 挂在 Canvas（始终激活）上，提供公开方法供按钮调用以打开/关闭 SettingsPanel。
/// 解决 GameObject.SetActive 持久化调用在目标对象非激活时不生效的问题。
/// </summary>
public class SettingsPanelController : MonoBehaviour
{
    [SerializeField] private GameObject settingsPanel;

    public void ShowSettings()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(true);
            FormalParchmentAudio.PlayOpen();
        }
    }

    public void HideSettings()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
            FormalParchmentAudio.PlayClose();
        }
    }
}
