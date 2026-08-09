using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 场景加载器：静态方法切换场景，供 UI 按钮和关卡完成触发器调用。
/// </summary>
public class SceneLoader : MonoBehaviour
{
    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
