using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class FormalArtOverviewTools
{
    private static readonly string[] ArtScenes =
    {
        "Assets/MoMing/FormalLevels/FormalLevel01.unity",
        "Assets/MoMing/FormalLevels/FormalLevel02.unity",
        "Assets/MoMing/FormalLevels/FormalLevel03.unity",
        "Assets/MoMing/FormalLevels/FormalLevel04.unity",
        "Assets/MoMing/FormalLevels/FormalLevel045.unity",
        "Assets/MoMing/FormalLevels/FormalLevel05.unity",
        "Assets/MoMing/FormalLevels/FormalSharedArt_L01_L02.unity",
        "Assets/MoMing/FormalLevels/FormalSharedArt_L02_L03.unity",
        "Assets/MoMing/FormalLevels/FormalSharedArt_L03_L04.unity",
        "Assets/MoMing/FormalLevels/FormalSharedArt_L04_L045.unity",
        "Assets/MoMing/FormalLevels/FormalSharedArt_L045_L05.unity"
    };

    [MenuItem("Tools/SuperBreadMan/Formal Art/Open All Art Scenes Additively")]
    public static void OpenAllArtScenesAdditively()
    {
        EditorSceneManager.OpenScene(ArtScenes[0], OpenSceneMode.Single);
        for (int i = 1; i < ArtScenes.Length; i++)
            EditorSceneManager.OpenScene(ArtScenes[i], OpenSceneMode.Additive);
    }

    [MenuItem("Tools/SuperBreadMan/Formal Art/Close All Art Scenes Except Active Scene")]
    public static void CloseAllArtScenesExceptActiveScene()
    {
        Scene activeScene = SceneManager.GetActiveScene();
        for (int i = SceneManager.sceneCount - 1; i >= 0; i--)
        {
            Scene scene = SceneManager.GetSceneAt(i);
            if (scene != activeScene && scene.isLoaded)
                EditorSceneManager.CloseScene(scene, true);
        }
    }
}
