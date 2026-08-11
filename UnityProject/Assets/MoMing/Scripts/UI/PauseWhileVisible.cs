using UnityEngine;

/// <summary>
/// 万能暂停：把本脚本挂到任意"菜单/面板"物体上，该物体一显示(激活)就暂停游戏(Time.timeScale=0)，
/// 隐藏时恢复到打开前的时间流速。不挑是谁把面板显示出来的，接线怎么乱都能生效。
/// 每帧兜底强制，防止被其它脚本改回去。兼容 Unity 2022.3（无 async/await）。
///
/// 用法：选中"打开后会出现的那个菜单面板物体" → Add Component → Pause While Visible。
/// </summary>
[DisallowMultipleComponent]
public class PauseWhileVisible : MonoBehaviour
{
    private float prevTimeScale = 1f;

    void OnEnable()
    {
        // 记录打开前的时间流速；若打开时本就处于暂停(0)，则记为 1，避免关闭后一直卡住
        prevTimeScale = Time.timeScale > 0f ? Time.timeScale : 1f;
        Time.timeScale = 0f;
    }

    void Update()
    {
        // 面板显示期间，若被别的脚本改回去，每帧强制拉回 0
        if (Time.timeScale != 0f)
            Time.timeScale = 0f;
    }

    void OnDisable()
    {
        Time.timeScale = prevTimeScale;
    }
}
