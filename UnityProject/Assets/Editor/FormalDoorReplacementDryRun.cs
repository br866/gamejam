using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class FormalDoorReplacementDryRun
{
    private const string ReportPath = "Assets/MoMing/FormalLevels/DoorReplacementDryRun.md";
    private const string Door4PrefabPath =
        "Assets/MoMing/FormalLevels/Prefabs/Doors/L01_ExitDoor_ToLevel02.prefab";
    private const string Door5PrefabPath =
        "Assets/MoMing/FormalLevels/Prefabs/Doors/L01_Door_Mechanism.prefab";

    private static readonly string[] ContentPrefabs =
    {
        "Assets/MoMing/FormalLevels/Prefabs/L01_Content.prefab",
        "Assets/MoMing/FormalLevels/Prefabs/L02_Content.prefab",
        "Assets/MoMing/FormalLevels/Prefabs/L03_Content.prefab",
        "Assets/MoMing/FormalLevels/Prefabs/L04_Content.prefab",
        "Assets/MoMing/FormalLevels/Prefabs/L045_Content.prefab",
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

    [MenuItem("Tools/SuperBreadMan/Formal Art/Run Door Replacement Dry Run")]
    public static void Run()
    {
        var report = new StringBuilder();
        report.AppendLine("# Door Replacement Dry Run");
        report.AppendLine();
        report.AppendLine("Read-only report. No Prefabs, scenes, or GameObjects were modified.");
        report.AppendLine();

        int total = 0;
        int matched = 0;
        foreach (string prefabPath in ContentPrefabs)
        {
            report.AppendLine($"## Prefab: `{prefabPath}`");
            report.AppendLine();
            int prefabTotal;
            int prefabMatched;
            ScanPrefab(prefabPath, report, out prefabTotal, out prefabMatched);
            total += prefabTotal;
            matched += prefabMatched;
        }

        foreach (string scenePath in SharedScenes)
        {
            report.AppendLine($"## Scene: `{scenePath}`");
            report.AppendLine();
            int sceneTotal;
            int sceneMatched;
            ScanScene(scenePath, report, out sceneTotal, out sceneMatched);
            total += sceneTotal;
            matched += sceneMatched;
        }

        report.AppendLine("## Summary");
        report.AppendLine();
        report.AppendLine($"- Door-like leaf objects: `{total}`");
        report.AppendLine($"- Objects with existing Door4/Door5 replacement targets: `{matched}`");
        report.AppendLine($"- Objects requiring a new door-type Prefab or manual classification: `{total - matched}`");

        File.WriteAllText(ReportPath, report.ToString(), Encoding.UTF8);
        AssetDatabase.Refresh();
        Debug.Log($"Door replacement dry run written to {ReportPath}. Found {total} door-like objects.");
    }

    static void ScanPrefab(string prefabPath, StringBuilder report, out int total, out int matched)
    {
        total = 0;
        matched = 0;
        GameObject root = PrefabUtility.LoadPrefabContents(prefabPath);
        try
        {
            foreach (Transform transform in root.GetComponentsInChildren<Transform>(true))
            {
                if (!IsDoorLeaf(transform.gameObject))
                    continue;

                total++;
                AppendEntry(report, transform, prefabPath, ref matched);
            }
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }
    }

    static void ScanScene(string scenePath, StringBuilder report, out int total, out int matched)
    {
        total = 0;
        matched = 0;
        Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            foreach (Transform transform in root.GetComponentsInChildren<Transform>(true))
            {
                if (!IsDoorLeaf(transform.gameObject))
                    continue;

                total++;
                AppendEntry(report, transform, scenePath, ref matched);
            }
        }
    }

    static void AppendEntry(StringBuilder report, Transform original, string ownerPath, ref int matched)
    {
        string type = Classify(original.name);
        string targetPath = GetTargetPrefabPath(type);
        report.AppendLine($"### `{original.name}`");
        report.AppendLine();
        report.AppendLine($"- Owner: `{ownerPath}`");
        report.AppendLine($"- Hierarchy: `{GetHierarchyPath(original)}`");
        report.AppendLine($"- Type: `{type}`");
        report.AppendLine($"- Original Position: `{Format(original.position)}`");
        report.AppendLine($"- Original Rotation: `{Format(original.rotation.eulerAngles)}`");
        report.AppendLine($"- Original Lossy Scale: `{Format(original.lossyScale)}`");

        if (string.IsNullOrEmpty(targetPath))
        {
            report.AppendLine("- Replacement: `Manual classification required`");
            report.AppendLine();
            return;
        }

        GameObject targetPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(targetPath);
        Transform targetLeaf = FindDoorLeaf(targetPrefab);
        if (targetLeaf == null)
        {
            report.AppendLine($"- Replacement: `{targetPath}` but its FormalDoor leaf could not be found");
            report.AppendLine();
            return;
        }

        matched++;
        report.AppendLine($"- Replacement: `{targetPath}`");
        report.AppendLine($"- Template DoorLeaf Local Position: `{Format(targetLeaf.localPosition)}`");
        report.AppendLine($"- Template DoorLeaf Local Rotation: `{Format(targetLeaf.localRotation.eulerAngles)}`");
        report.AppendLine($"- Template DoorLeaf Local Scale: `{Format(targetLeaf.localScale)}`");
        report.AppendLine("- Predicted transform result: `Exact matrix alignment required during replacement`");
        report.AppendLine();
    }

    static bool IsDoorLeaf(GameObject gameObject)
    {
        string name = gameObject.name.ToLowerInvariant();
        if (name.Contains("jamb") || name.Contains("doorway") || name.Contains("door pivot"))
            return false;
        return name.Contains("door");
    }

    static string Classify(string name)
    {
        string value = name.ToLowerInvariant();
        if (value.Contains("door4")) return "Door4";
        if (value.Contains("door5")) return "Door5";
        if (value.Contains("door3")) return "Door3";
        if (value.Contains("grille")) return "MetalGrille";
        if (value.Contains("big door")) return "BigDoor";
        return "OtherDoor";
    }

    static string GetTargetPrefabPath(string type)
    {
        if (type == "Door4") return Door4PrefabPath;
        if (type == "Door5") return Door5PrefabPath;
        return string.Empty;
    }

    static Transform FindDoorLeaf(GameObject targetPrefab)
    {
        if (targetPrefab == null)
            return null;

        FormalDoor door = targetPrefab.GetComponentInChildren<FormalDoor>(true);
        if (door == null || door.VisualPivot == null)
            return null;

        MeshRenderer renderer = door.VisualPivot.GetComponentInChildren<MeshRenderer>(true);
        return renderer != null ? renderer.transform : null;
    }

    static string GetHierarchyPath(Transform transform)
    {
        var names = new Stack<string>();
        for (Transform current = transform; current != null; current = current.parent)
            names.Push(current.name);
        return string.Join("/", names);
    }

    static string Format(Vector3 value)
    {
        return string.Format(CultureInfo.InvariantCulture, "{0:F4}, {1:F4}, {2:F4}", value.x, value.y, value.z);
    }
}
