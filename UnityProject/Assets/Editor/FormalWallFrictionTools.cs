using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class FormalWallFrictionTools
{
    private const string WallMaterialPath =
        "Assets/MoMing/FormalLevels/Physics/FormalWallNoFriction.physicMaterial";

    private static readonly string[] FormalArtScenes =
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

    [MenuItem("Tools/SuperBreadMan/Formal Physics/Apply Zero Friction To Static Obstacles")]
    public static void ApplyZeroFrictionToWallColliders()
    {
        PhysicMaterial material = AssetDatabase.LoadAssetAtPath<PhysicMaterial>(WallMaterialPath);
        if (material == null)
        {
            Debug.LogError("Missing wall PhysicMaterial: " + WallMaterialPath);
            return;
        }

        SceneSetup[] previousSetup = EditorSceneManager.GetSceneManagerSetup();
        int changed = 0;
        try
        {
            foreach (string scenePath in FormalArtScenes)
            {
                Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                bool sceneChanged = false;
                foreach (GameObject root in scene.GetRootGameObjects())
                {
                    foreach (Collider collider in root.GetComponentsInChildren<Collider>(true))
                    {
                        if (!IsStaticObstacle(collider))
                            continue;

                        if (collider.sharedMaterial == material)
                            continue;

                        Undo.RecordObject(collider, "Assign Wall No-Friction Material");
                        collider.sharedMaterial = material;
                        PrefabUtility.RecordPrefabInstancePropertyModifications(collider);
                        changed++;
                        sceneChanged = true;
                    }
                }

                if (sceneChanged)
                {
                    EditorSceneManager.MarkSceneDirty(scene);
                    EditorSceneManager.SaveScene(scene);
                }
            }
        }
        finally
        {
            if (previousSetup != null && previousSetup.Length > 0)
                EditorSceneManager.RestoreSceneManagerSetup(previousSetup);
        }

        Debug.Log($"Applied no-friction material to {changed} static obstacle colliders.");
    }

    [MenuItem("Tools/SuperBreadMan/Formal Physics/Apply Zero Friction To All Doors")]
    public static void ApplyZeroFrictionToAllDoors()
    {
        PhysicMaterial material = AssetDatabase.LoadAssetAtPath<PhysicMaterial>(WallMaterialPath);
        if (material == null)
        {
            Debug.LogError("Missing wall PhysicMaterial: " + WallMaterialPath);
            return;
        }

        int changed = 0;
        foreach (string scenePath in FormalArtScenes)
        {
            Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            bool sceneChanged = false;
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                foreach (Collider collider in root.GetComponentsInChildren<Collider>(true))
                {
                    if (collider.isTrigger || !IsDoorLike(collider.transform))
                        continue;

                    if (collider.sharedMaterial == material)
                        continue;

                    collider.sharedMaterial = material;
                    PrefabUtility.RecordPrefabInstancePropertyModifications(collider);
                    changed++;
                    sceneChanged = true;
                }
            }

            if (sceneChanged)
            {
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
            }
        }

        Debug.Log($"Applied no-friction material to {changed} door colliders.");
    }

    static bool IsStaticObstacle(Collider collider)
    {
        if (collider.isTrigger || collider.GetComponentInParent<Rigidbody>() != null)
            return false;

        for (Transform current = collider.transform; current != null; current = current.parent)
        {
            string value = current.name.ToLowerInvariant();
            if (value.Contains("floor") ||
                value.Contains("ground") ||
                value.Contains("player") ||
                value.Contains("trigger"))
                return false;
        }

        return true;
    }

    static bool IsDoorLike(Transform transform)
    {
        for (Transform current = transform; current != null; current = current.parent)
        {
            string value = current.name.ToLowerInvariant();
            if (value.Contains("door") || value.Contains("jamb") || value.Contains("doorway"))
                return true;
        }

        return false;
    }
}
