using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 统一地面工具集（Tools/Formal/Ground/）。全部为小工具：
/// 1) 从选中渲染器建/重 fitting 地面体  2) 禁用 NavGround 层旧地面碰撞(可回滚)
/// 3) 审计覆盖洞/高度错位  4) 多选同步表面高度。
/// </summary>
public static class FormalGroundTools
{
    const string JournalFolder = "Assets/MoMing/GroundJournals";
    const int NavGroundLayer = 6; // ProjectSettings/TagManager: NavGround

    // ---------------------------------------------------------------- 1. Volume From Selection

    [MenuItem("Tools/Formal/Ground/Create Or Fit Volume From Selection")]
    static void CreateOrFitVolumeFromSelection()
    {
        FormalGroundVolume target = Selection.gameObjects
            .Select(go => go.GetComponent<FormalGroundVolume>())
            .FirstOrDefault(v => v != null);

        List<Renderer> renderers = new List<Renderer>();
        foreach (GameObject go in Selection.gameObjects)
        {
            if (go.GetComponent<FormalGroundVolume>() != null)
                continue;
            renderers.AddRange(go.GetComponentsInChildren<Renderer>(false));
        }

        if (renderers.Count == 0)
        {
            Debug.LogWarning("[FormalGroundTools] 选中内容里没有可用的 Renderer（或只选了地面体本身）。");
            return;
        }

        Bounds bounds = renderers[0].bounds;
        foreach (Renderer r in renderers)
            bounds.Encapsulate(r.bounds);

        if (target == null)
        {
            GameObject go = new GameObject("FormalGroundVolume");
            Undo.RegisterCreatedObjectUndo(go, "Create FormalGroundVolume");
            go.layer = NavGroundLayer;
            target = go.AddComponent<FormalGroundVolume>();
            target.Box.size = new Vector3(1f, target.Thickness, 1f);
        }
        else
        {
            Undo.RecordObject(target.transform, "Fit FormalGroundVolume");
            Undo.RecordObject(target.Box, "Fit FormalGroundVolume");
        }

        target.transform.position = new Vector3(bounds.center.x, 0f, bounds.center.z);
        Vector3 size = target.Box.size;
        size.x = Mathf.Max(bounds.size.x, 0.1f);
        size.z = Mathf.Max(bounds.size.z, 0.1f);
        target.Box.size = size;
        EditorUtility.SetDirty(target);
        Debug.Log($"[FormalGroundTools] volume at {target.transform.position}, footprint {size.x:F1}x{size.z:F1}, top={target.TopHeight:F3}");
    }

    // ---------------------------------------------------------------- 2. Disable NavGround colliders (+ rollback)

    [MenuItem("Tools/Formal/Ground/Disable NavGround Colliders (Except Volumes)")]
    static void DisableNavGroundColliders()
    {
        string journalPath = JournalPath("disable");
        using (StreamWriter writer = new StreamWriter(journalPath, false))
        {
            int disabled = 0;
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                if (!scene.isLoaded)
                    continue;
                foreach (Collider collider in EnumerateColliders(scene))
                    disabled += TryDisable(collider, scene.name, writer);
            }
            Debug.Log($"[FormalGroundTools] disabled {disabled} NavGround collider(s). journal={journalPath}");
        }
    }

    [MenuItem("Tools/Formal/Ground/Rollback Last Disable Pass")]
    static void RollbackLastDisablePass()
    {
        string journalPath = Directory.Exists(JournalFolder)
            ? Directory.GetFiles(JournalFolder, "*_disable.jsonl").OrderBy(File.GetLastWriteTime).LastOrDefault()
            : null;
        if (journalPath == null)
        {
            Debug.LogWarning("[FormalGroundTools] 没有可回滚的 disable 日志。");
            return;
        }

        int restored = 0;
        foreach (string line in File.ReadAllLines(journalPath))
        {
            Collider collider = FindColliderByInstanceId(line);
            if (collider != null && !collider.enabled)
            {
                Undo.RecordObject(collider, "Rollback Ground Disable");
                collider.enabled = true;
                EditorUtility.SetDirty(collider);
                restored++;
            }
        }
        MarkLoadedScenesDirty();
        Debug.Log($"[FormalGroundTools] rollback finished: {restored} collider(s) re-enabled from {Path.GetFileName(journalPath)}.");
    }

    static int TryDisable(Collider collider, string sceneName, StreamWriter writer)
    {
        if (collider == null || !collider.enabled || collider.gameObject.layer != NavGroundLayer)
            return 0;
        if (collider.GetComponentInParent<FormalGroundVolume>() != null)
            return 0;

        Undo.RecordObject(collider, "Disable Ground Proxy");
        collider.enabled = false;
        EditorUtility.SetDirty(collider);
        writer.WriteLine(JsonLine("disable", sceneName, PathOf(collider.transform), collider.GetInstanceID()));
        return 1;
    }

    // ---------------------------------------------------------------- 3. Coverage audit

    [MenuItem("Tools/Formal/Ground/Audit Ground Coverage")]
    static void AuditGroundCoverage()
    {
        List<FormalGroundVolume> volumes = Object.FindObjectsOfType<FormalGroundVolume>(false).ToList();
        if (volumes.Count == 0)
        {
            Debug.LogWarning("[FormalGroundTools] 场景里没有 FormalGroundVolume，无法审计。");
            return;
        }

        float referenceTop = volumes[0].TopHeight;
        List<string> holes = new List<string>();
        List<string> misaligned = new List<string>();
        HashSet<Renderer> visited = new HashSet<Renderer>();

        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene scene = SceneManager.GetSceneAt(i);
            if (!scene.isLoaded)
                continue;
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                foreach (Renderer renderer in root.GetComponentsInChildren<Renderer>(false))
                {
                    if (!visited.Add(renderer) || renderer is ParticleSystemRenderer)
                        continue;
                    if (renderer.GetComponentInParent<FormalPlayerActor>() != null)
                        continue;

                    Bounds b = renderer.bounds;
                    float top = b.max.y;
                    bool nearFloor = top <= referenceTop + 1f && top >= referenceTop - 2f;
                    if (!nearFloor)
                        continue;

                    bool insideAnyVolume = volumes.Any(v => CoversXZ(v, b));
                    float deviation = Mathf.Abs(top - referenceTop);
                    if (!insideAnyVolume && deviation > 0.05f)
                        holes.Add($"{scene.name} :: {PathOf(renderer.transform)} top={top:F3} (outside volume XZ)");
                    else if (deviation > 0.05f)
                        misaligned.Add($"{scene.name} :: {PathOf(renderer.transform)} top={top:F3} dev={deviation:F3}");
                }
            }
        }

        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.AppendLine($"[FormalGroundTools] audit: volumes={volumes.Count} referenceTop={referenceTop:F3} holes={holes.Count} misaligned={misaligned.Count}");
        foreach (string line in holes.Take(40)) sb.AppendLine("  HOLE  " + line);
        foreach (string line in misaligned.Take(40)) sb.AppendLine("  OFFSET " + line);
        Debug.Log(sb.ToString());
    }

    static bool CoversXZ(FormalGroundVolume volume, Bounds b)
    {
        BoxCollider box = volume.Box;
        Bounds world = TransformBox(volume.transform, box);
        return b.min.x >= world.min.x - 0.01f && b.max.x <= world.max.x + 0.01f &&
               b.min.z >= world.min.z - 0.01f && b.max.z <= world.max.z + 0.01f;
    }

    static Bounds TransformBox(Transform t, BoxCollider box)
    {
        Vector3 center = t.TransformPoint(box.center);
        Vector3 half = Vector3.Scale(box.size * 0.5f, t.lossyScale);
        return new Bounds(center, half * 2f);
    }

    // ---------------------------------------------------------------- 4. Copy top height

    [MenuItem("Tools/Formal/Ground/Copy Top Height To Selected Volumes")]
    static void CopyTopHeightToSelection()
    {
        FormalGroundVolume[] selected = Selection.gameObjects
            .Select(go => go.GetComponent<FormalGroundVolume>())
            .Where(v => v != null)
            .ToArray();
        if (selected.Length < 2)
        {
            Debug.LogWarning("[FormalGroundTools] 请选中两个及以上带 FormalGroundVolume 的物体。");
            return;
        }

        float source = selected[0].TopHeight;
        int applied = 0;
        for (int i = 1; i < selected.Length; i++)
        {
            Undo.RecordObject(selected[i], "Copy Ground Top Height");
            selected[i].TopHeight = source;
            EditorUtility.SetDirty(selected[i]);
            applied++;
        }
        Debug.Log($"[FormalGroundTools] copied topHeight {source:F3} to {applied} volume(s).");
    }

    // ---------------------------------------------------------------- helpers

    static IEnumerable<Collider> EnumerateColliders(Scene scene)
    {
        foreach (GameObject root in scene.GetRootGameObjects())
            foreach (Collider collider in root.GetComponentsInChildren<Collider>(true))
                yield return collider;
    }

    static Collider FindColliderByInstanceId(string jsonLine)
    {
        const string key = "\"instanceId\":";
        int start = jsonLine.IndexOf(key);
        if (start < 0)
            return null;
        start += key.Length;
        int end = jsonLine.IndexOfAny(new[] { ',', '}' }, start);
        if (!int.TryParse(jsonLine.Substring(start, end - start).Trim(), out int id))
            return null;
        return EditorUtility.InstanceIDToObject(id) as Collider;
    }

    static string JournalPath(string kind)
    {
        if (!Directory.Exists(JournalFolder))
            Directory.CreateDirectory(JournalFolder);
        return Path.Combine(JournalFolder, $"{Sanitize(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name)}_{kind}.jsonl");
    }

    static string Sanitize(string name)
    {
        foreach (char c in Path.GetInvalidFileNameChars())
            name = name.Replace(c, '_');
        return name;
    }

    static string JsonLine(string op, string sceneName, string path, int instanceId)
    {
        return $"{{\"op\":\"{op}\",\"scene\":\"{sceneName}\",\"path\":\"{path.Replace("\\", "/")}\",\"instanceId\":{instanceId}}}";
    }

    static string PathOf(Transform node)
    {
        string path = node.name;
        while (node.parent != null)
        {
            node = node.parent;
            path = node.name + "/" + path;
        }
        return path;
    }

    static void MarkLoadedScenesDirty()
    {
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene scene = SceneManager.GetSceneAt(i);
            if (scene.isLoaded)
                EditorSceneManager.MarkSceneDirty(scene);
        }
    }
}
