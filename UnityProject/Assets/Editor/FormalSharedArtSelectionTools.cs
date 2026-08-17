using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class FormalSharedArtSelectionTools
{
    private const string ReportPath = "Assets/MoMing/FormalLevels/SharedArtSelectionReport.md";
    private const string L01PrefabPath = "Assets/MoMing/FormalLevels/Prefabs/L01_Content.prefab";
    private const string L02PrefabPath = "Assets/MoMing/FormalLevels/Prefabs/L02_Content.prefab";
    private const string L03PrefabPath = "Assets/MoMing/FormalLevels/Prefabs/L03_Content.prefab";
    private const string L04PrefabPath = "Assets/MoMing/FormalLevels/Prefabs/L04_Content.prefab";
    private const string L045PrefabPath = "Assets/MoMing/FormalLevels/Prefabs/L045_Content.prefab";
    private const string L05PrefabPath = "Assets/MoMing/FormalLevels/Prefabs/L05_Content.prefab";
    private const string SharedScenePath = "Assets/MoMing/FormalLevels/FormalSharedArt_L01_L02.unity";
    private const string SharedL02L03ScenePath = "Assets/MoMing/FormalLevels/FormalSharedArt_L02_L03.unity";
    private const string SharedL03L04ScenePath = "Assets/MoMing/FormalLevels/FormalSharedArt_L03_L04.unity";
    private const string SharedL04L045ScenePath = "Assets/MoMing/FormalLevels/FormalSharedArt_L04_L045.unity";
    private const string SharedL045L05ScenePath = "Assets/MoMing/FormalLevels/FormalSharedArt_L045_L05.unity";
    private const float PositionTolerance = 0.03f;
    private const float ScaleTolerance = 0.03f;

    private static readonly ExtractionSpec[] L01L02SharedSpecs =
    {
        new ExtractionSpec("door4 (1)", "98ba8db753cdc824fbbf7e0c13d6c03b", 7158796146257112550, new Vector3(20.1218f, 13.4601f, -6.0496f)),
        new ExtractionSpec("floor2 (2)", "cab5280519badce489b3cfd05aef5937", 9043464486913355637, new Vector3(25.4548f, 8.9901f, -5.0896f)),
        new ExtractionSpec("wall5 (8)", "37f78a18ce42f2b499bff224c23c1f12", 4858151225365440216, new Vector3(19.6148f, 13.8201f, -0.0796f)),
        new ExtractionSpec("wall5 (7)", "37f78a18ce42f2b499bff224c23c1f12", 4858151225365440216, new Vector3(19.9448f, 13.9301f, -14.2296f)),
        new ExtractionSpec("door4  jamb (1)", "6e7dc99f5f2c175488485e5ca7bbff07", 7158796146257112550, new Vector3(20.1148f, 13.7081f, -6.1996f)),
        new ExtractionSpec("wall3 (1)", "000d9cdf2db00b148bc1cf01fcc66658", 5703254614228647771, new Vector3(23.1848f, 13.8301f, 5.9104f))
    };

    private static readonly ExtractionSpec L02OwnedCarpet =
        new ExtractionSpec("Carpet2 (1)", "dbca526c2aa919c46a24b62c20cb2895", 7158796146257112550, new Vector3(17.084751f, 9.770076f, -6.2296104f));

    private static readonly RecoverySpec[] L04L045RecoverySpecs =
    {
        new RecoverySpec("door4 (4)", "98ba8db753cdc824fbbf7e0c13d6c03b", 7158796146257112550, "353d29627cd2f014eac4eba30327787f", new Vector3(-125.6953f, 13.6601f, -0.0596f), new Vector3(794.1466f, 1380.805f, 771.0737f), new Vector3(270f, 90f, 0f)),
        new RecoverySpec("door4  jamb (4)", "6e7dc99f5f2c175488485e5ca7bbff07", 7158796146257112550, "cf1d1c62f323a6441913b13b9ef8cfe5", new Vector3(-125.7023f, 13.9081f, -0.2096f), new Vector3(996.9408f, 803.1295f, 794.2629f), new Vector3(270f, 90f, 0f)),
        new RecoverySpec("wall5 (43)", "37f78a18ce42f2b499bff224c23c1f12", 4858151225365440216, "35136df790c14304fbf65c151d084d4f", new Vector3(-125.7852f, 13.9301f, 8.1204f), new Vector3(1118.414f, 852.6958f, 1088.791f), new Vector3(270f, 90f, 0f)),
        new RecoverySpec("wall5 (42)", "37f78a18ce42f2b499bff224c23c1f12", 4858151225365440216, "35136df790c14304fbf65c151d084d4f", new Vector3(-125.7852f, 13.9301f, -6.6296f), new Vector3(736.9451f, 852.6958f, 1088.791f), new Vector3(270f, 90f, 0f))
    };

    [MenuItem("Tools/SuperBreadMan/Formal Art/Export Current Shared-Art Selection Report")]
    public static void ExportCurrentSelectionReport()
    {
        SelectionEntry[] entries = CollectSelectionEntries();
        DuplicateMatch[] l02Matches = FindMatches(entries, L02PrefabPath, "L02");
        DuplicateMatch[] l03Matches = FindMatches(entries, L03PrefabPath, "L03");
        DuplicateMatch[] l04Matches = FindMatches(entries, L04PrefabPath, "L04");
        DuplicateMatch[] l045Matches = FindMatches(entries, L045PrefabPath, "L045");
        DuplicateMatch[] l05Matches = FindMatches(entries, L05PrefabPath, "L05");

        var builder = new StringBuilder();
        builder.AppendLine("# Shared Art Selection Report");
        builder.AppendLine();
        builder.AppendLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        builder.AppendLine();
        builder.AppendLine("## Selected Objects");
        builder.AppendLine();

        if (entries.Length == 0)
        {
            builder.AppendLine("No scene objects are selected.");
        }
        else
        {
            foreach (SelectionEntry entry in entries)
            {
                builder.AppendLine($"- `{entry.ScenePath}`");
                builder.AppendLine($"  - Overview root: `{entry.OverviewRoot}`");
                builder.AppendLine($"  - Source prefab: `{entry.SourcePrefabPath}`");
                builder.AppendLine($"  - Source object: `{entry.SourceObjectName}`");
                builder.AppendLine($"  - World position: `{Format(entry.Position)}`");
                builder.AppendLine($"  - World rotation: `{Format(entry.Rotation.eulerAngles)}`");
                builder.AppendLine($"  - World scale: `{Format(entry.Scale)}`");
                builder.AppendLine($"  - Mesh: `{entry.MeshGuid}:{entry.MeshLocalId}`");
                builder.AppendLine($"  - Materials: `{string.Join(", ", entry.MaterialGuids)}`");
            }
        }

        builder.AppendLine();
        builder.AppendLine("## L02 Duplicate Candidates");
        builder.AppendLine();

        if (l02Matches.Length == 0)
        {
            builder.AppendLine("No same-position L02 duplicate candidates found. Matches from the selected source prefab are omitted.");
        }
        else
        {
            foreach (DuplicateMatch match in l02Matches)
            {
                builder.AppendLine($"- Selected `{match.Selected.ScenePath}` appears to duplicate L02 object `{match.L02Path}`");
                builder.AppendLine($"  - Distance: `{match.PositionDistance.ToString("0.####", CultureInfo.InvariantCulture)}`");
                builder.AppendLine($"  - Scale delta: `{match.ScaleDistance.ToString("0.####", CultureInfo.InvariantCulture)}`");
                builder.AppendLine($"  - Mesh: `{match.Selected.MeshGuid}:{match.Selected.MeshLocalId}`");
            }
        }

        builder.AppendLine();
        builder.AppendLine("## L03 Duplicate Candidates");
        builder.AppendLine();

        if (l03Matches.Length == 0)
        {
            builder.AppendLine("No same-position L03 duplicate candidates found for the current selection.");
        }
        else
        {
            foreach (DuplicateMatch match in l03Matches)
            {
                builder.AppendLine($"- Selected `{match.Selected.ScenePath}` appears to duplicate L03 object `{match.L02Path}`");
                builder.AppendLine($"  - Distance: `{match.PositionDistance.ToString("0.####", CultureInfo.InvariantCulture)}`");
                builder.AppendLine($"  - Scale delta: `{match.ScaleDistance.ToString("0.####", CultureInfo.InvariantCulture)}`");
                builder.AppendLine($"  - Mesh: `{match.Selected.MeshGuid}:{match.Selected.MeshLocalId}`");
            }
        }

        builder.AppendLine();
        builder.AppendLine("## L04 Duplicate Candidates");
        builder.AppendLine();

        if (l04Matches.Length == 0)
        {
            builder.AppendLine("No same-position L04 duplicate candidates found.");
        }
        else
        {
            foreach (DuplicateMatch match in l04Matches)
            {
                builder.AppendLine($"- Selected `{match.Selected.ScenePath}` appears to duplicate L04 object `{match.L02Path}`");
                builder.AppendLine($"  - Distance: `{match.PositionDistance.ToString("0.####", CultureInfo.InvariantCulture)}`");
                builder.AppendLine($"  - Scale delta: `{match.ScaleDistance.ToString("0.####", CultureInfo.InvariantCulture)}`");
                builder.AppendLine($"  - Mesh: `{match.Selected.MeshGuid}:{match.Selected.MeshLocalId}`");
            }
        }

        builder.AppendLine();
        builder.AppendLine("## L045 Duplicate Candidates");
        builder.AppendLine();

        if (l045Matches.Length == 0)
        {
            builder.AppendLine("No same-position L045 duplicate candidates found.");
        }
        else
        {
            foreach (DuplicateMatch match in l045Matches)
            {
                builder.AppendLine($"- Selected `{match.Selected.ScenePath}` appears to duplicate L045 object `{match.L02Path}`");
                builder.AppendLine($"  - Distance: `{match.PositionDistance.ToString("0.####", CultureInfo.InvariantCulture)}`");
                builder.AppendLine($"  - Scale delta: `{match.ScaleDistance.ToString("0.####", CultureInfo.InvariantCulture)}`");
                builder.AppendLine($"  - Mesh: `{match.Selected.MeshGuid}:{match.Selected.MeshLocalId}`");
            }
        }

        builder.AppendLine();
        builder.AppendLine("## L05 Duplicate Candidates");
        builder.AppendLine();

        if (l05Matches.Length == 0)
        {
            builder.AppendLine("No same-position L05 duplicate candidates found.");
        }
        else
        {
            foreach (DuplicateMatch match in l05Matches)
            {
                builder.AppendLine($"- Selected `{match.Selected.ScenePath}` appears to duplicate L05 object `{match.L02Path}`");
                builder.AppendLine($"  - Distance: `{match.PositionDistance.ToString("0.####", CultureInfo.InvariantCulture)}`");
                builder.AppendLine($"  - Scale delta: `{match.ScaleDistance.ToString("0.####", CultureInfo.InvariantCulture)}`");
                builder.AppendLine($"  - Mesh: `{match.Selected.MeshGuid}:{match.Selected.MeshLocalId}`");
            }
        }

        builder.AppendLine();
        builder.AppendLine("## Notes");
        builder.AppendLine();
        builder.AppendLine("- Objects listed under L02 or L03 duplicate candidates should be removed from that level content after they are moved into the shared additive art scene.");
        builder.AppendLine("- Objects with no duplicate candidate may still be shared if they bridge the selected source level and the target level; review them visually before extraction.");

        Directory.CreateDirectory(Path.GetDirectoryName(ReportPath));
        File.WriteAllText(ReportPath, builder.ToString(), Encoding.UTF8);
        AssetDatabase.ImportAsset(ReportPath);
        Debug.Log($"Wrote shared art selection report to {ReportPath} with {entries.Length} selected objects, {l02Matches.Length} L02 candidates, {l03Matches.Length} L03 candidates, {l04Matches.Length} L04 candidates, {l045Matches.Length} L045 candidates, and {l05Matches.Length} L05 candidates.");
    }

    [MenuItem("Tools/SuperBreadMan/Formal Art/Apply L01-L02 Shared-Art Extraction")]
    public static void ApplyL01L02SharedArtExtraction()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            return;

        if (File.Exists(SharedScenePath) && !EditorUtility.DisplayDialog(
                "Overwrite shared art scene?",
                $"{SharedScenePath} already exists. Recreate it from the current L01/L02 extraction specs?",
                "Recreate", "Cancel"))
            return;

        GameObject l01Root = PrefabUtility.LoadPrefabContents(L01PrefabPath);
        GameObject l02Root = PrefabUtility.LoadPrefabContents(L02PrefabPath);
        int sharedCreated = 0;
        int l01Removed = 0;
        int l02Removed = 0;
        bool carpetMoved = false;

        Scene sharedScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        GameObject sharedRoot = new GameObject("SharedArt_L01_L02");
        try
        {
            foreach (ExtractionSpec spec in L01L02SharedSpecs)
            {
                GameObject source = FindObject(l01Root, spec);
                if (source == null)
                {
                    Debug.LogWarning($"Could not find L01 shared source `{spec.Name}`.");
                    continue;
                }

                CloneToRoot(source, sharedRoot.transform);
                sharedCreated++;
                UnityEngine.Object.DestroyImmediate(source);
                l01Removed++;

                GameObject l02Duplicate = FindObject(l02Root, spec);
                if (l02Duplicate != null)
                {
                    UnityEngine.Object.DestroyImmediate(l02Duplicate);
                    l02Removed++;
                }
            }

            GameObject carpet = FindObject(l01Root, L02OwnedCarpet);
            if (carpet != null)
            {
                CloneToRoot(carpet, l02Root.transform);
                UnityEngine.Object.DestroyImmediate(carpet);
                carpetMoved = true;
                l01Removed++;
            }
            else
            {
                Debug.LogWarning("Could not find L01 `Carpet2 (1)` to move into L02 ownership.");
            }

            PrefabUtility.SaveAsPrefabAsset(l01Root, L01PrefabPath);
            PrefabUtility.SaveAsPrefabAsset(l02Root, L02PrefabPath);
            EditorSceneManager.SaveScene(sharedScene, SharedScenePath);
            AssetDatabase.Refresh();

            Debug.Log(
                $"Applied L01-L02 shared art extraction. Shared objects: {sharedCreated}; " +
                $"L01 removed: {l01Removed}; L02 duplicates removed: {l02Removed}; " +
                $"Carpet moved to L02: {carpetMoved}.");
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(l02Root);
            PrefabUtility.UnloadPrefabContents(l01Root);
        }
    }

    [MenuItem("Tools/SuperBreadMan/Formal Art/Apply L02-L03 Shared-Art Extraction")]
    public static void ApplyL02L03SharedArtExtraction()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            return;

        if (File.Exists(SharedL02L03ScenePath) && !EditorUtility.DisplayDialog(
                "Overwrite shared art scene?",
                $"{SharedL02L03ScenePath} already exists. Recreate it from the current report?",
                "Recreate", "Cancel"))
            return;

        SelectionEntry[] selected = CollectSelectionEntries();
        var entries = new List<SelectionEntry>();
        foreach (SelectionEntry entry in selected)
        {
            if (entry.OverviewRoot == "OVERVIEW_L02_Content")
                entries.Add(entry);
        }

        if (entries.Count == 0)
        {
            EditorUtility.DisplayDialog("No L02 selection", "Select the shared objects under OVERVIEW_L02_Content first.", "OK");
            return;
        }

        GameObject l02Root = PrefabUtility.LoadPrefabContents(L02PrefabPath);
        GameObject l03Root = PrefabUtility.LoadPrefabContents(L03PrefabPath);
        Scene sharedScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        GameObject sharedRoot = new GameObject("SharedArt_L02_L03");
        int sharedCreated = 0;
        int l02Removed = 0;
        int l03Removed = 0;

        try
        {
            foreach (SelectionEntry entry in entries)
            {
                ExtractionSpec spec = new ExtractionSpec(
                    entry.SourceObjectName,
                    entry.MeshGuid,
                    entry.MeshLocalId,
                    entry.Position);
                GameObject source = FindObject(l02Root, spec);
                if (source == null)
                {
                    Debug.LogWarning($"Could not find selected L02 source `{entry.SourceObjectName}` at {Format(entry.Position)}.");
                    continue;
                }

                CloneToRoot(source, sharedRoot.transform);
                UnityEngine.Object.DestroyImmediate(source);
                sharedCreated++;
                l02Removed++;

                GameObject l03Duplicate = FindObject(l03Root, spec);
                if (l03Duplicate != null)
                {
                    UnityEngine.Object.DestroyImmediate(l03Duplicate);
                    l03Removed++;
                }
                else
                {
                    Debug.LogWarning($"No matching L03 duplicate found for `{entry.SourceObjectName}` at {Format(entry.Position)}.");
                }
            }

            PrefabUtility.SaveAsPrefabAsset(l02Root, L02PrefabPath);
            PrefabUtility.SaveAsPrefabAsset(l03Root, L03PrefabPath);
            EditorSceneManager.SaveScene(sharedScene, SharedL02L03ScenePath);
            AssetDatabase.Refresh();
            Debug.Log($"Applied L02-L03 shared art extraction. Shared objects: {sharedCreated}; L02 removed: {l02Removed}; L03 duplicates removed: {l03Removed}.");
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(l03Root);
            PrefabUtility.UnloadPrefabContents(l02Root);
        }
    }

    [MenuItem("Tools/SuperBreadMan/Formal Art/Apply L03-L04 Shared-Art Extraction")]
    public static void ApplyL03L04SharedArtExtraction()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            return;

        if (File.Exists(SharedL03L04ScenePath) && !EditorUtility.DisplayDialog(
                "Overwrite shared art scene?",
                $"{SharedL03L04ScenePath} already exists. Recreate it from the current selection?",
                "Recreate", "Cancel"))
            return;

        SelectionEntry[] selected = CollectSelectionEntries();
        var entries = new List<SelectionEntry>();
        foreach (SelectionEntry entry in selected)
        {
            if (entry.OverviewRoot == "OVERVIEW_L03_Content" || entry.OverviewRoot == "OVERVIEW_L04_Content")
                entries.Add(entry);
        }

        if (entries.Count == 0)
        {
            EditorUtility.DisplayDialog("No L03/L04 selection", "Select the shared objects under OVERVIEW_L03_Content or OVERVIEW_L04_Content first.", "OK");
            return;
        }

        bool sourceIsL04 = entries[0].OverviewRoot == "OVERVIEW_L04_Content";
        string sourcePrefabPath = sourceIsL04 ? L04PrefabPath : L03PrefabPath;
        string targetPrefabPath = sourceIsL04 ? L03PrefabPath : L04PrefabPath;
        GameObject sourceRoot = PrefabUtility.LoadPrefabContents(sourcePrefabPath);
        GameObject targetRoot = PrefabUtility.LoadPrefabContents(targetPrefabPath);
        Scene sharedScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        GameObject sharedRoot = new GameObject("SharedArt_L03_L04");
        int sharedCreated = 0;
        int l03Removed = 0;
        int l04Removed = 0;

        try
        {
            foreach (SelectionEntry entry in entries)
            {
                ExtractionSpec spec = new ExtractionSpec(entry.SourceObjectName, entry.MeshGuid, entry.MeshLocalId, entry.Position);
                GameObject source = FindObject(sourceRoot, spec);
                if (source == null)
                {
                    Debug.LogWarning($"Could not find selected L03 source `{entry.SourceObjectName}` at {Format(entry.Position)}.");
                    continue;
                }

                CloneToRoot(source, sharedRoot.transform);
                UnityEngine.Object.DestroyImmediate(source);
                sharedCreated++;
                l03Removed++;

                GameObject targetDuplicate = FindObject(targetRoot, spec);
                if (targetDuplicate != null)
                {
                    UnityEngine.Object.DestroyImmediate(targetDuplicate);
                    l04Removed++;
                }
                else
                {
                    Debug.LogWarning($"No matching L04 duplicate found for `{entry.SourceObjectName}` at {Format(entry.Position)}.");
                }
            }

            PrefabUtility.SaveAsPrefabAsset(sourceRoot, sourcePrefabPath);
            PrefabUtility.SaveAsPrefabAsset(targetRoot, targetPrefabPath);
            EditorSceneManager.SaveScene(sharedScene, SharedL03L04ScenePath);
            AssetDatabase.Refresh();
            Debug.Log($"Applied L03-L04 shared art extraction. Shared objects: {sharedCreated}; L03 removed: {l03Removed}; L04 duplicates removed: {l04Removed}.");
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(targetRoot);
            PrefabUtility.UnloadPrefabContents(sourceRoot);
        }
    }

    [MenuItem("Tools/SuperBreadMan/Formal Art/Apply L04-L045 Shared-Art Extraction")]
    public static void ApplyL04L045SharedArtExtraction()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            return;

        if (File.Exists(SharedL04L045ScenePath) && !EditorUtility.DisplayDialog(
                "Overwrite shared art scene?",
                $"{SharedL04L045ScenePath} already exists. Recreate it from the current selection?",
                "Recreate", "Cancel"))
            return;

        SelectionEntry[] selected = CollectSelectionEntries();
        var entries = new List<SelectionEntry>();
        foreach (SelectionEntry entry in selected)
        {
            if (entry.OverviewRoot == "OVERVIEW_L04_Content" || entry.OverviewRoot == "OVERVIEW_L045_Content")
                entries.Add(entry);
        }

        if (entries.Count == 0)
        {
            EditorUtility.DisplayDialog("No L04/L045 selection", "Select the shared objects under OVERVIEW_L04_Content or OVERVIEW_L045_Content first.", "OK");
            return;
        }

        bool sourceIsL045 = entries[0].OverviewRoot == "OVERVIEW_L045_Content";
        string sourcePrefabPath = sourceIsL045 ? L045PrefabPath : L04PrefabPath;
        string targetPrefabPath = sourceIsL045 ? L04PrefabPath : L045PrefabPath;
        GameObject sourceRoot = PrefabUtility.LoadPrefabContents(sourcePrefabPath);
        GameObject targetRoot = PrefabUtility.LoadPrefabContents(targetPrefabPath);
        Scene sharedScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        GameObject sharedRoot = new GameObject("SharedArt_L04_L045");
        int sharedCreated = 0;
        int sourceRemoved = 0;
        int targetRemoved = 0;

        try
        {
            foreach (SelectionEntry entry in entries)
            {
                ExtractionSpec spec = new ExtractionSpec(entry.SourceObjectName, entry.MeshGuid, entry.MeshLocalId, entry.Position);
                GameObject source = FindObject(sourceRoot, spec);
                if (source == null)
                {
                    Debug.LogWarning($"Could not find selected L04/L045 source `{entry.SourceObjectName}` at {Format(entry.Position)}.");
                    continue;
                }

                CloneToRoot(source, sharedRoot.transform);
                UnityEngine.Object.DestroyImmediate(source);
                sharedCreated++;
                sourceRemoved++;

                GameObject targetDuplicate = FindObject(targetRoot, spec);
                if (targetDuplicate != null)
                {
                    UnityEngine.Object.DestroyImmediate(targetDuplicate);
                    targetRemoved++;
                }
                else
                {
                    Debug.LogWarning($"No matching L04/L045 duplicate found for `{entry.SourceObjectName}` at {Format(entry.Position)}.");
                }
            }

            PrefabUtility.SaveAsPrefabAsset(sourceRoot, sourcePrefabPath);
            PrefabUtility.SaveAsPrefabAsset(targetRoot, targetPrefabPath);
            EditorSceneManager.SaveScene(sharedScene, SharedL04L045ScenePath);
            AssetDatabase.Refresh();
            Debug.Log($"Applied L04-L045 shared art extraction. Shared objects: {sharedCreated}; source removed: {sourceRemoved}; target duplicates removed: {targetRemoved}.");
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(targetRoot);
            PrefabUtility.UnloadPrefabContents(sourceRoot);
        }
    }

    [MenuItem("Tools/SuperBreadMan/Formal Art/Apply L045-L05 Shared-Art Extraction")]
    public static void ApplyL045L05SharedArtExtraction()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            return;

        if (File.Exists(SharedL045L05ScenePath) && !EditorUtility.DisplayDialog(
                "Overwrite shared art scene?",
                $"{SharedL045L05ScenePath} already exists. Recreate it from the current selection?",
                "Recreate", "Cancel"))
            return;

        SelectionEntry[] selected = CollectSelectionEntries();
        var entries = new List<SelectionEntry>();
        foreach (SelectionEntry entry in selected)
        {
            if (entry.OverviewRoot == "OVERVIEW_L045_Content" || entry.OverviewRoot == "OVERVIEW_L05_Content")
                entries.Add(entry);
        }

        if (entries.Count == 0)
        {
            EditorUtility.DisplayDialog("No L045/L05 selection", "Select the shared objects under OVERVIEW_L045_Content or OVERVIEW_L05_Content first.", "OK");
            return;
        }

        bool sourceIsL05 = entries[0].OverviewRoot == "OVERVIEW_L05_Content";
        string sourcePrefabPath = sourceIsL05 ? L05PrefabPath : L045PrefabPath;
        string targetPrefabPath = sourceIsL05 ? L045PrefabPath : L05PrefabPath;
        GameObject sourceRoot = PrefabUtility.LoadPrefabContents(sourcePrefabPath);
        GameObject targetRoot = PrefabUtility.LoadPrefabContents(targetPrefabPath);
        Scene sharedScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        GameObject sharedRoot = new GameObject("SharedArt_L045_L05");
        int sharedCreated = 0;
        int sourceRemoved = 0;
        int targetRemoved = 0;

        try
        {
            foreach (SelectionEntry entry in entries)
            {
                ExtractionSpec spec = new ExtractionSpec(entry.SourceObjectName, entry.MeshGuid, entry.MeshLocalId, entry.Position);
                GameObject source = FindObject(sourceRoot, spec);
                if (source == null)
                {
                    Debug.LogWarning($"Could not find selected L045/L05 source `{entry.SourceObjectName}` at {Format(entry.Position)}.");
                    continue;
                }

                CloneToRoot(source, sharedRoot.transform);
                UnityEngine.Object.DestroyImmediate(source);
                sharedCreated++;
                sourceRemoved++;

                GameObject targetDuplicate = FindObject(targetRoot, spec);
                if (targetDuplicate != null)
                {
                    UnityEngine.Object.DestroyImmediate(targetDuplicate);
                    targetRemoved++;
                }
                else
                {
                    Debug.LogWarning($"No matching L045/L05 duplicate found for `{entry.SourceObjectName}` at {Format(entry.Position)}.");
                }
            }

            PrefabUtility.SaveAsPrefabAsset(sourceRoot, sourcePrefabPath);
            PrefabUtility.SaveAsPrefabAsset(targetRoot, targetPrefabPath);
            EditorSceneManager.SaveScene(sharedScene, SharedL045L05ScenePath);
            AssetDatabase.Refresh();
            Debug.Log($"Applied L045-L05 shared art extraction. Shared objects: {sharedCreated}; source removed: {sourceRemoved}; target duplicates removed: {targetRemoved}.");
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(targetRoot);
            PrefabUtility.UnloadPrefabContents(sourceRoot);
        }
    }

    [MenuItem("Tools/SuperBreadMan/Formal Art/Recover L04-L045 Shared Scenes")]
    public static void RecoverL04L045SharedScenes()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            return;

        if (File.Exists(SharedL04L045ScenePath) && !EditorUtility.DisplayDialog(
                "Recover overwritten shared scene?",
                "The current FormalSharedArt_L04_L045 scene will be copied to FormalSharedArt_L045_L05, then the L04/L045 scene will be rebuilt from the saved report data.",
                "Recover", "Cancel"))
            return;

        AssetDatabase.CopyAsset(SharedL04L045ScenePath, SharedL045L05ScenePath);
        AssetDatabase.Refresh();

        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        GameObject root = new GameObject("SharedArt_L04_L045");
        int created = 0;
        try
        {
            foreach (RecoverySpec spec in L04L045RecoverySpecs)
            {
                Mesh mesh = LoadAsset<Mesh>(spec.MeshGuid, spec.MeshLocalId);
                Material material = LoadAsset<Material>(spec.MaterialGuid, 0);
                if (mesh == null || material == null)
                {
                    Debug.LogWarning($"Could not recover `{spec.Name}` because its Mesh or Material asset was not found.");
                    continue;
                }

                GameObject recovered = new GameObject(spec.Name);
                recovered.transform.SetParent(root.transform, false);
                recovered.transform.SetPositionAndRotation(spec.Position, Quaternion.Euler(spec.EulerAngles));
                recovered.transform.localScale = spec.Scale;
                MeshFilter filter = recovered.AddComponent<MeshFilter>();
                filter.sharedMesh = mesh;
                MeshRenderer renderer = recovered.AddComponent<MeshRenderer>();
                renderer.sharedMaterial = material;
                MeshCollider collider = recovered.AddComponent<MeshCollider>();
                collider.sharedMesh = mesh;
                created++;
            }

            EditorSceneManager.SaveScene(scene, SharedL04L045ScenePath);
            AssetDatabase.Refresh();
            Debug.Log($"Recovered shared scenes. Rebuilt L04/L045 objects: {created}; copied overwritten scene to {SharedL045L05ScenePath}.");
        }
        finally
        {
            if (scene.IsValid() && scene.isLoaded)
                EditorSceneManager.CloseScene(scene, true);
        }
    }

    private static SelectionEntry[] CollectSelectionEntries()
    {
        var entries = new List<SelectionEntry>();
        foreach (GameObject selected in Selection.gameObjects)
        {
            if (selected == null || selected.scene.name != "FormalSharedArtSelectionOverview")
                continue;

            Renderer renderer = selected.GetComponent<Renderer>();
            MeshFilter meshFilter = selected.GetComponent<MeshFilter>();
            if (renderer == null || meshFilter == null || meshFilter.sharedMesh == null)
                continue;

            GameObject source = PrefabUtility.GetCorrespondingObjectFromSource(selected);
            string sourcePrefabPath = GetSourcePrefabPath(source);
            string sourceObjectName = source != null ? source.name : selected.name;
            string meshGuid = GetAssetGuid(meshFilter.sharedMesh, out long meshLocalId);

            var materialGuids = new List<string>();
            foreach (Material material in renderer.sharedMaterials)
                materialGuids.Add(material != null ? GetAssetGuid(material, out _) : "null");

            entries.Add(new SelectionEntry
            {
                GameObject = selected,
                ScenePath = GetHierarchyPath(selected.transform),
                OverviewRoot = GetOverviewRootName(selected.transform),
                SourcePrefabPath = sourcePrefabPath,
                SourceObjectName = sourceObjectName,
                Position = selected.transform.position,
                Rotation = selected.transform.rotation,
                Scale = selected.transform.lossyScale,
                MeshGuid = meshGuid,
                MeshLocalId = meshLocalId,
                MaterialGuids = materialGuids.ToArray()
            });
        }

        return entries.ToArray();
    }

    private static DuplicateMatch[] FindMatches(SelectionEntry[] selectedEntries, string prefabPath, string levelLabel)
    {
        if (selectedEntries.Length == 0)
            return Array.Empty<DuplicateMatch>();

        var matches = new List<DuplicateMatch>();
        GameObject prefabRoot = PrefabUtility.LoadPrefabContents(prefabPath);
        try
        {
            bool isSelectedSourcePrefab = selectedEntries[0].SourcePrefabPath == prefabPath;
            if (isSelectedSourcePrefab)
                return Array.Empty<DuplicateMatch>();

            Renderer[] renderers = prefabRoot.GetComponentsInChildren<Renderer>(true);
            foreach (SelectionEntry selected in selectedEntries)
            {
                foreach (Renderer renderer in renderers)
                {
                    MeshFilter meshFilter = renderer.GetComponent<MeshFilter>();
                    if (meshFilter == null || meshFilter.sharedMesh == null)
                        continue;

                    string meshGuid = GetAssetGuid(meshFilter.sharedMesh, out long meshLocalId);
                    if (meshGuid != selected.MeshGuid || meshLocalId != selected.MeshLocalId)
                        continue;

                    if (!MaterialsMatch(renderer.sharedMaterials, selected.MaterialGuids))
                        continue;

                    float positionDistance = Vector3.Distance(renderer.transform.position, selected.Position);
                    float scaleDistance = Vector3.Distance(renderer.transform.lossyScale, selected.Scale);
                    if (positionDistance > PositionTolerance || scaleDistance > ScaleTolerance)
                        continue;

                    matches.Add(new DuplicateMatch
                    {
                        Selected = selected,
                        L02Path = $"{levelLabel}_Content/{GetHierarchyPath(renderer.transform)}",
                        PositionDistance = positionDistance,
                        ScaleDistance = scaleDistance
                    });
                }
            }
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(prefabRoot);
        }

        return matches.ToArray();
    }

    private static bool MaterialsMatch(Material[] materials, string[] materialGuids)
    {
        if (materials.Length != materialGuids.Length)
            return false;

        for (int i = 0; i < materials.Length; i++)
        {
            string guid = materials[i] != null ? GetAssetGuid(materials[i], out _) : "null";
            if (guid != materialGuids[i])
                return false;
        }

        return true;
    }

    private static string GetSourcePrefabPath(GameObject source)
    {
        if (source == null)
            return string.Empty;

        GameObject root = source.transform.root.gameObject;
        return AssetDatabase.GetAssetPath(root);
    }

    private static string GetAssetGuid(UnityEngine.Object asset, out long localId)
    {
        localId = 0;
        if (asset == null)
            return "null";

        AssetDatabase.TryGetGUIDAndLocalFileIdentifier(asset, out string guid, out localId);
        return guid;
    }

    private static string GetOverviewRootName(Transform transform)
    {
        Transform root = transform;
        while (root.parent != null)
            root = root.parent;

        return root.name;
    }

    private static string GetHierarchyPath(Transform transform)
    {
        var names = new Stack<string>();
        Transform current = transform;
        while (current != null)
        {
            names.Push(current.name);
            current = current.parent;
        }

        return string.Join("/", names);
    }

    private static string Format(Vector3 value)
    {
        return string.Format(CultureInfo.InvariantCulture, "{0:0.####}, {1:0.####}, {2:0.####}", value.x, value.y, value.z);
    }

    private static T LoadAsset<T>(string guid, long localId) where T : UnityEngine.Object
    {
        string path = AssetDatabase.GUIDToAssetPath(guid);
        if (string.IsNullOrEmpty(path))
            return null;

        UnityEngine.Object[] assets = AssetDatabase.LoadAllAssetsAtPath(path);
        foreach (UnityEngine.Object asset in assets)
        {
            AssetDatabase.TryGetGUIDAndLocalFileIdentifier(asset, out string assetGuid, out long assetLocalId);
            if (assetGuid == guid && assetLocalId == localId)
                return asset as T;
        }

        return AssetDatabase.LoadAssetAtPath<T>(path);
    }

    private static GameObject FindObject(GameObject root, ExtractionSpec spec)
    {
        MeshFilter[] meshFilters = root.GetComponentsInChildren<MeshFilter>(true);
        foreach (MeshFilter meshFilter in meshFilters)
        {
            if (meshFilter == null || meshFilter.sharedMesh == null || meshFilter.name != spec.Name)
                continue;

            string meshGuid = GetAssetGuid(meshFilter.sharedMesh, out long meshLocalId);
            if (meshGuid != spec.MeshGuid || meshLocalId != spec.MeshLocalId)
                continue;

            if (Vector3.Distance(meshFilter.transform.position, spec.Position) > PositionTolerance)
                continue;

            return meshFilter.gameObject;
        }

        return null;
    }

    private static void CloneToRoot(GameObject source, Transform root)
    {
        Vector3 position = source.transform.position;
        Quaternion rotation = source.transform.rotation;
        Vector3 scale = source.transform.lossyScale;
        GameObject clone = UnityEngine.Object.Instantiate(source);
        clone.name = source.name;
        clone.transform.SetParent(root, false);
        clone.transform.SetPositionAndRotation(position, rotation);
        clone.transform.localScale = scale;
    }

    private sealed class SelectionEntry
    {
        public GameObject GameObject;
        public string ScenePath;
        public string OverviewRoot;
        public string SourcePrefabPath;
        public string SourceObjectName;
        public Vector3 Position;
        public Quaternion Rotation;
        public Vector3 Scale;
        public string MeshGuid;
        public long MeshLocalId;
        public string[] MaterialGuids;
    }

    private sealed class DuplicateMatch
    {
        public SelectionEntry Selected;
        public string L02Path;
        public float PositionDistance;
        public float ScaleDistance;
    }

    private sealed class ExtractionSpec
    {
        public readonly string Name;
        public readonly string MeshGuid;
        public readonly long MeshLocalId;
        public readonly Vector3 Position;

        public ExtractionSpec(string name, string meshGuid, long meshLocalId, Vector3 position)
        {
            Name = name;
            MeshGuid = meshGuid;
            MeshLocalId = meshLocalId;
            Position = position;
        }
    }

    private sealed class RecoverySpec
    {
        public readonly string Name;
        public readonly string MeshGuid;
        public readonly long MeshLocalId;
        public readonly string MaterialGuid;
        public readonly Vector3 Position;
        public readonly Vector3 Scale;
        public readonly Vector3 EulerAngles;

        public RecoverySpec(string name, string meshGuid, long meshLocalId, string materialGuid, Vector3 position, Vector3 scale, Vector3 eulerAngles)
        {
            Name = name;
            MeshGuid = meshGuid;
            MeshLocalId = meshLocalId;
            MaterialGuid = materialGuid;
            Position = position;
            Scale = scale;
            EulerAngles = eulerAngles;
        }
    }
}
