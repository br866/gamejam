using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class FormalModelPrefabTools
{
    const string PrefabFolder = "Assets/MoMing/FormalLevels/Prefabs/SharedModels";
    const string ReportPath = "Assets/MoMing/FormalLevels/SharedModelExtractionReport.md";

    static readonly string[] ContentPrefabs =
    {
        "Assets/MoMing/FormalLevels/Prefabs/L01_Content.prefab",
        "Assets/MoMing/FormalLevels/Prefabs/L02_Content.prefab",
        "Assets/MoMing/FormalLevels/Prefabs/L03_Content.prefab",
        "Assets/MoMing/FormalLevels/Prefabs/L04_Content.prefab",
        "Assets/MoMing/FormalLevels/Prefabs/L045_Content.prefab",
        "Assets/MoMing/FormalLevels/Prefabs/L05_Content.prefab"
    };

    static readonly string[] SharedArtScenes =
    {
        "Assets/MoMing/FormalLevels/FormalSharedArt_L01_L02.unity",
        "Assets/MoMing/FormalLevels/FormalSharedArt_L02_L03.unity",
        "Assets/MoMing/FormalLevels/FormalSharedArt_L03_L04.unity",
        "Assets/MoMing/FormalLevels/FormalSharedArt_L04_L045.unity",
        "Assets/MoMing/FormalLevels/FormalSharedArt_L045_L05.unity"
    };

    [MenuItem("Tools/SuperBreadMan/Formal Art/Audit Shared Model Prefab Extraction")]
    public static void Audit()
    {
        var lines = new List<string>
        {
            "# Shared Model Prefab Extraction Report",
            "",
            "A model is a GameObject with a MeshFilter or SkinnedMeshRenderer. Existing prefab-source objects are retained; non-model objects are excluded. Candidate roots are the highest modeled transforms in a hierarchy.",
            ""
        };

        int modeled = 0;
        int existingPrefab = 0;
        int extractable = 0;

        lines.Add("## Content Prefabs");
        foreach (string path in ContentPrefabs)
        {
            var root = PrefabUtility.LoadPrefabContents(path);
            var candidates = FindModelRoots(root.transform);
            int existing = candidates.Count(transform => HasPrefabSource(transform.gameObject));
            modeled += candidates.Count;
            existingPrefab += existing;
            extractable += candidates.Count - existing;
            lines.Add($"- `{path}`: {candidates.Count} model roots, {existing} existing prefab sources, {candidates.Count - existing} extractable.");
            PrefabUtility.UnloadPrefabContents(root);
        }

        lines.Add("");
        lines.Add("## Shared Art Scenes");
        foreach (string path in SharedArtScenes)
        {
            Scene scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
            var candidates = FindModelRoots(scene);
            int existing = candidates.Count(transform => HasPrefabSource(transform.gameObject));
            modeled += candidates.Count;
            existingPrefab += existing;
            extractable += candidates.Count - existing;
            lines.Add($"- `{path}`: {candidates.Count} model roots, {existing} existing prefab sources, {candidates.Count - existing} extractable.");
            EditorSceneManager.CloseScene(scene, false);
        }

        lines.Add("");
        lines.Add("## Totals");
        lines.Add($"- Model roots: {modeled}");
        lines.Add($"- Existing prefab sources retained: {existingPrefab}");
        lines.Add($"- Extractable model roots: {extractable}");
        lines.Add("- Non-model objects: excluded by rule.");

        File.WriteAllLines(ReportPath, lines);
        AssetDatabase.ImportAsset(ReportPath);
        Debug.Log($"[FormalModelPrefabTools] Wrote {ReportPath}.");
    }

    [MenuItem("Tools/SuperBreadMan/Formal Art/Extract All Non-Prefab Modeled Objects")]
    public static void ExtractAll()
    {
        EnsureFolder(PrefabFolder);

        foreach (string path in ContentPrefabs)
        {
            var root = PrefabUtility.LoadPrefabContents(path);
            ExtractModelRoots(root.transform, Path.GetFileNameWithoutExtension(path));
            PrefabUtility.SaveAsPrefabAsset(root, path);
            PrefabUtility.UnloadPrefabContents(root);
        }

        foreach (string path in SharedArtScenes)
        {
            Scene scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
            foreach (GameObject root in scene.GetRootGameObjects())
                ExtractModelRoots(root.transform, Path.GetFileNameWithoutExtension(path));
            EditorSceneManager.SaveScene(scene);
            EditorSceneManager.CloseScene(scene, false);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Audit();
    }

    [MenuItem("Tools/SuperBreadMan/Formal Art/Validate Shared Model Prefab Extraction")]
    public static void ValidateExtraction()
    {
        var failures = new List<string>();

        foreach (string path in ContentPrefabs)
        {
            var root = PrefabUtility.LoadPrefabContents(path);
            CollectUnprefabbedModels(root.transform, path, failures);
            PrefabUtility.UnloadPrefabContents(root);
        }

        foreach (string path in SharedArtScenes)
        {
            Scene scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
            foreach (GameObject root in scene.GetRootGameObjects())
                CollectUnprefabbedModels(root.transform, path, failures);
            EditorSceneManager.CloseScene(scene, false);
        }

        if (failures.Count > 0)
        {
            foreach (string failure in failures)
                Debug.LogError("[FormalModelPrefabTools] " + failure);
            throw new System.InvalidOperationException($"Found {failures.Count} modeled objects without prefab ownership.");
        }

        Debug.Log("[FormalModelPrefabTools] All formal modeled objects have prefab ownership.");
    }

    static void ExtractModelRoots(Transform root, string owner)
    {
        foreach (Transform candidate in FindModelRoots(root).OrderByDescending(GetDepth))
        {
            if (HasPrefabSource(candidate.gameObject))
                continue;

            Transform parent = candidate.parent;
            int siblingIndex = candidate.GetSiblingIndex();
            Vector3 localPosition = candidate.localPosition;
            Quaternion localRotation = candidate.localRotation;
            Vector3 localScale = candidate.localScale;
            string assetPath = GetAssetPath(owner, candidate.gameObject);

            GameObject copy = Object.Instantiate(candidate.gameObject);
            copy.name = candidate.name;
            copy.transform.SetParent(null, true);
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(copy, assetPath);
            Object.DestroyImmediate(copy);

            GameObject replacement = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            replacement.name = candidate.name;
            replacement.transform.SetParent(parent, false);
            replacement.transform.localPosition = localPosition;
            replacement.transform.localRotation = localRotation;
            replacement.transform.localScale = localScale;
            replacement.transform.SetSiblingIndex(siblingIndex);
            Object.DestroyImmediate(candidate.gameObject);
        }
    }

    static List<Transform> FindModelRoots(Scene scene)
    {
        var result = new List<Transform>();
        foreach (GameObject root in scene.GetRootGameObjects())
            result.AddRange(FindModelRoots(root.transform));
        return result;
    }

    static List<Transform> FindModelRoots(Transform root)
    {
        return root.GetComponentsInChildren<Transform>(true)
            .Where(IsModelRoot)
            .ToList();
    }

    static void CollectUnprefabbedModels(Transform root, string owner, List<string> failures)
    {
        foreach (Transform candidate in FindModelRoots(root))
        {
            if (!HasPrefabSource(candidate.gameObject))
                failures.Add($"{owner}: {candidate.name} has a model but no prefab source.");
        }
    }

    static bool IsModelRoot(Transform transform)
    {
        if (!HasModel(transform.gameObject))
            return false;

        for (Transform parent = transform.parent; parent != null; parent = parent.parent)
        {
            if (HasModel(parent.gameObject))
                return false;
        }

        return true;
    }

    static bool HasModel(GameObject gameObject)
    {
        MeshFilter filter = gameObject.GetComponent<MeshFilter>();
        if (filter != null && filter.sharedMesh != null)
            return true;

        SkinnedMeshRenderer skinned = gameObject.GetComponent<SkinnedMeshRenderer>();
        return skinned != null && skinned.sharedMesh != null;
    }

    static bool HasPrefabSource(GameObject gameObject)
    {
        return PrefabUtility.GetCorrespondingObjectFromSource(gameObject) != null;
    }

    static int GetDepth(Transform transform)
    {
        int depth = 0;
        for (Transform current = transform.parent; current != null; current = current.parent)
            depth++;
        return depth;
    }

    static string GetAssetPath(string owner, GameObject gameObject)
    {
        string meshIdentity = "NoMesh";
        MeshFilter filter = gameObject.GetComponent<MeshFilter>();
        if (filter != null && filter.sharedMesh != null && AssetDatabase.TryGetGUIDAndLocalFileIdentifier(filter.sharedMesh, out string guid, out long localId))
            meshIdentity = guid.Substring(0, 8) + "_" + localId;

        string safeName = string.Concat(gameObject.name.Select(character => Path.GetInvalidFileNameChars().Contains(character) ? '_' : character));
        return $"{PrefabFolder}/{owner}_{safeName}_{meshIdentity}.prefab";
    }

    static void EnsureFolder(string folder)
    {
        if (AssetDatabase.IsValidFolder(folder))
            return;

        string parent = Path.GetDirectoryName(folder).Replace('\\', '/');
        string name = Path.GetFileName(folder);
        AssetDatabase.CreateFolder(parent, name);
    }
}
