using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class FormalDoorPrefabReplacementTools
{
    private const string Door4PrefabPath =
        "Assets/MoMing/FormalLevels/Prefabs/Doors/L01_ExitDoor_ToLevel02.prefab";
    private const string Door5PrefabPath =
        "Assets/MoMing/FormalLevels/Prefabs/Doors/L01_Door_Mechanism.prefab";

    private static readonly string[] ContentPrefabs =
    {
        "Assets/MoMing/FormalLevels/Prefabs/L03_Content.prefab",
        "Assets/MoMing/FormalLevels/Prefabs/L04_Content.prefab",
        "Assets/MoMing/FormalLevels/Prefabs/L05_Content.prefab"
    };

    private static readonly string[] SharedScenes =
    {
        "Assets/MoMing/FormalLevels/FormalSharedArt_L01_L02.unity",
        "Assets/MoMing/FormalLevels/FormalSharedArt_L02_L03.unity",
        "Assets/MoMing/FormalLevels/FormalSharedArt_L03_L04.unity",
        "Assets/MoMing/FormalLevels/FormalSharedArt_L04_L045.unity",
        "Assets/MoMing/FormalLevels/FormalSharedArt_L045_L05.unity"
    };

    [MenuItem("Tools/SuperBreadMan/Formal Art/Apply Verified Door4 And Door5 Replacements")]
    public static void ApplyVerifiedReplacements()
    {
        int replaced = 0;
        foreach (string prefabPath in ContentPrefabs)
            replaced += ReplaceInPrefab(prefabPath);

        foreach (string scenePath in SharedScenes)
            replaced += ReplaceInScene(scenePath);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"Replaced {replaced} verified Door4/Door5 leaf objects.");
    }

    static int ReplaceInPrefab(string prefabPath)
    {
        GameObject root = PrefabUtility.LoadPrefabContents(prefabPath);
        int replaced = ReplaceUnderRoot(root);
        PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
        PrefabUtility.UnloadPrefabContents(root);
        return replaced;
    }

    static int ReplaceInScene(string scenePath)
    {
        Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
        int replaced = 0;
        foreach (GameObject root in scene.GetRootGameObjects())
            replaced += ReplaceUnderRoot(root);

        if (replaced > 0)
        {
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        return replaced;
    }

    static int ReplaceUnderRoot(GameObject root)
    {
        Transform[] transforms = root.GetComponentsInChildren<Transform>(true);
        int replaced = 0;
        foreach (Transform transform in transforms)
        {
            if (!TryGetDoorPrefabPath(transform.gameObject.name, out string prefabPath))
                continue;

            ReplaceLeaf(transform, prefabPath);
            replaced++;
        }

        return replaced;
    }

    static void ReplaceLeaf(Transform original, string prefabPath)
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (prefab == null)
            throw new System.InvalidOperationException("Missing door prefab: " + prefabPath);

        Transform parent = original.parent;
        Matrix4x4 originalLocalMatrix = Matrix4x4.TRS(
            original.localPosition,
            original.localRotation,
            original.localScale);
        GameObject replacement = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        replacement.name = original.name;
        replacement.transform.SetParent(parent, false);

        Transform replacementLeaf = FindDoorLeaf(replacement);
        if (replacementLeaf == null)
            throw new System.InvalidOperationException("Door prefab has no FormalDoor leaf: " + prefabPath);

        Matrix4x4 leafLocalToRoot = replacement.transform.worldToLocalMatrix * replacementLeaf.localToWorldMatrix;
        ApplyLocalMatrix(replacement.transform, originalLocalMatrix * leafLocalToRoot.inverse);
        Object.DestroyImmediate(original.gameObject);
    }

    static Transform FindDoorLeaf(GameObject root)
    {
        FormalDoor door = root.GetComponentInChildren<FormalDoor>(true);
        if (door == null || door.VisualPivot == null)
            return null;

        MeshRenderer renderer = door.VisualPivot.GetComponentInChildren<MeshRenderer>(true);
        return renderer != null ? renderer.transform : null;
    }

    static void ApplyLocalMatrix(Transform transform, Matrix4x4 matrix)
    {
        Vector3 right = matrix.GetColumn(0);
        Vector3 up = matrix.GetColumn(1);
        Vector3 forward = matrix.GetColumn(2);
        Vector3 scale = new Vector3(right.magnitude, up.magnitude, forward.magnitude);
        if (Vector3.Dot(Vector3.Cross(right, up), forward) < 0f)
            scale.x = -scale.x;

        transform.localPosition = matrix.GetColumn(3);
        transform.localRotation = Quaternion.LookRotation(forward / scale.z, up / scale.y);
        transform.localScale = scale;
    }

    static bool TryGetDoorPrefabPath(string name, out string prefabPath)
    {
        string value = name.ToLowerInvariant();
        if (value.Contains("jamb") || value.Contains("doorway"))
        {
            prefabPath = null;
            return false;
        }

        if (value.Contains("door4"))
        {
            prefabPath = Door4PrefabPath;
            return true;
        }

        if (value.Contains("door5"))
        {
            prefabPath = Door5PrefabPath;
            return true;
        }

        prefabPath = null;
        return false;
    }
}
