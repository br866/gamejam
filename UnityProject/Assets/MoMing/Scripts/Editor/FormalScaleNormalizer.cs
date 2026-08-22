using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 把选中物体下"无子节点的静态网格叶子"的非均匀/超大缩放烘焙进网格副本，
/// 然后把该节点缩放复位为 1。同步补偿同节点上的 Box/Sphere/Capsule/Mesh 碰撞体。
/// 刻意不处理：带子节点的节点、SkinnedMeshRenderer、粒子，避免破坏动画与层级。
///
/// 批量模式：NormalizeScene 写逐节点 JSONL 日志（先写日志再保存场景）；
/// RollbackScene 依据最近一次日志恢复原缩放/网格并删除烘焙资产。
/// </summary>
public static class FormalScaleNormalizer
{
    const string OutputFolder = "Assets/MoMing/BakedMeshes";
    const string JournalFolder = "Assets/MoMing/BakedMeshes/Journals";

    [MenuItem("Tools/Formal/Normalize Scale On Selected Leaf Meshes")]
    static void NormalizeSelected()
    {
        int normalized = 0;
        foreach (GameObject root in Selection.gameObjects)
        {
            foreach (Transform node in root.GetComponentsInChildren<Transform>(true))
            {
                if (node.childCount != 0)
                    continue;
                if (!IsNormalizableLeaf(node))
                    continue;

                if (NormalizeNode(node))
                    normalized++;
            }
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"[FormalScaleNormalizer] normalized {normalized} leaf mesh node(s).");
    }

    [MenuItem("Tools/Formal/Rollback Last Scene Normalization")]
    static void RollbackActiveSceneMenu()
    {
        int restored = RollbackScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        Debug.Log($"[FormalScaleNormalizer] rollback finished: {restored} node(s) restored.");
    }

    public static string JournalPathFor(string sceneName)
    {
        return Path.Combine(JournalFolder, Sanitize(sceneName) + "_normalize.jsonl");
    }

    /// <summary>批量归一化整个场景，返回处理数量。journalPath 输出日志文件路径。</summary>
    public static int NormalizeScene(string sceneName, out string journalPath)
    {
        var scene = UnityEngine.SceneManagement.SceneManager.GetSceneByName(sceneName);
        if (!scene.IsValid() || !scene.isLoaded)
        {
            journalPath = null;
            throw new System.InvalidOperationException($"scene '{sceneName}' is not loaded");
        }

        Directory.CreateDirectory(JournalFolder);
        journalPath = JournalPathFor(sceneName);
        var lines = new List<string>();

        int normalized = 0;
        foreach (var root in scene.GetRootGameObjects())
            foreach (var node in root.GetComponentsInChildren<Transform>(true))
            {
                string path = GetNodePath(node);
                if (node.childCount != 0 || !IsNormalizableLeaf(node))
                {
                    if (NeedsAttention(node))
                        lines.Add(JsonLine("skipped", sceneName, path, node.localScale, DescribeMesh(node), null, node.GetComponent<MeshFilter>() != null ? "has-children-or-unsupported" : "no-mesh"));
                    continue;
                }

                Vector3 worldBefore = node.lossyScale;
                Mesh before = node.GetComponent<MeshFilter>().sharedMesh;
                Vector3 originalLocalScale = node.localScale;
                string entryBaked = null;
                if (NormalizeNode(node))
                {
                    entryBaked = node.GetComponent<MeshFilter>().sharedMesh != before
                        ? AssetDatabase.GetAssetPath(node.GetComponent<MeshFilter>().sharedMesh)
                        : null;
                    lines.Add(JsonLine("normalized", sceneName, path, originalLocalScale, MeshRef(before), entryBaked,
                        $"worldScaleBefore={worldBefore.x:F3},{worldBefore.y:F3},{worldBefore.z:F3}"));
                    normalized++;
                }
            }

        File.WriteAllLines(journalPath, lines.ToArray());
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);
        return normalized;
    }

    /// <summary>按最近一次日志恢复场景节点的原始缩放与网格引用。</summary>
    public static int RollbackScene(string sceneName)
    {
        string journalPath = JournalPathFor(sceneName);
        if (!File.Exists(journalPath))
            throw new FileNotFoundException($"no journal for scene '{sceneName}' at {journalPath}");

        var scene = UnityEngine.SceneManagement.SceneManager.GetSceneByName(sceneName);
        if (!scene.IsValid() || !scene.isLoaded)
            throw new System.InvalidOperationException($"scene '{sceneName}' is not loaded");

        var bakedToDelete = new List<string>();
        int restored = 0;
        foreach (string line in File.ReadAllLines(journalPath))
        {
            if (string.IsNullOrEmpty(line) || !line.Contains("\"normalized\""))
                continue;

            var fields = ParseJournal(line);
            Transform node = FindNode(scene, fields.path);
            if (node == null)
            {
                Debug.LogWarning($"[FormalScaleNormalizer] rollback skipped missing node {fields.path}");
                continue;
            }

            Mesh original = LoadMesh(fields.meshContainer, fields.meshName);
            var filter = node.GetComponent<MeshFilter>();
            if (original != null && filter != null)
                filter.sharedMesh = original;

            Vector3 s = fields.oldScale;
            node.localScale = s;

            BoxCollider box = node.GetComponent<BoxCollider>();
            if (box != null && s.x != 0f && s.y != 0f && s.z != 0f)
            {
                box.center = new Vector3(box.center.x / s.x, box.center.y / s.y, box.center.z / s.z);
                box.size = new Vector3(box.size.x / s.x, box.size.y / s.y, box.size.z / s.z);
            }

            if (!string.IsNullOrEmpty(fields.bakedAsset) && File.Exists(fields.bakedAsset))
                bakedToDelete.Add(fields.bakedAsset);

            EditorUtility.SetDirty(node.gameObject);
            restored++;
        }

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);
        using (StreamWriter writer = File.AppendText(journalPath))
        {
            writer.WriteLine(JsonLine("rolledback", sceneName, "*", Vector3.zero, null, null, $"restored={restored}"));
        }
        foreach (string asset in bakedToDelete.ToArray())
        {
            if (!IsReferencedByAnyScene(asset))
                AssetDatabase.DeleteAsset(asset);
        }
        return restored;
    }

    struct JournalFields
    {
        public string action, scene, path, meshContainer, meshName, bakedAsset;
        public Vector3 oldScale;
    }

    static JournalFields ParseJournal(string line)
    {
        var f = new JournalFields();
        f.action = Extract(line, "\"action\": \"");
        f.scene = Extract(line, "\"scene\": \"");
        f.path = Extract(line, "\"path\": \"");
        f.meshContainer = Extract(line, "\"meshContainer\": \"");
        f.meshName = Extract(line, "\"meshName\": \"");
        f.bakedAsset = Extract(line, "\"bakedAsset\": \"");
        float x = ParseFloat(Extract(line, "\"oldScaleX\": "));
        float y = ParseFloat(Extract(line, "\"oldScaleY\": "));
        float z = ParseFloat(Extract(line, "\"oldScaleZ\": "));
        f.oldScale = new Vector3(x, y, z);
        return f;
    }

    static string Extract(string line, string key)
    {
        int start = line.IndexOf(key, System.StringComparison.Ordinal);
        if (start < 0) return "";
        start += key.Length;
        int endChar = key.EndsWith("\" ") || key.EndsWith(": \"") ? '"' : ',';
        int end = endChar == '"' ? line.IndexOf('"', start) : line.IndexOf(',', start);
        if (end < 0) end = line.Length;
        return line.Substring(start, end - start);
    }

    static float ParseFloat(string value)
    {
        float result;
        return float.TryParse(value, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out result) ? result : 0f;
    }

    static string JsonLine(string action, string scene, string path, Vector3 scale, string meshRef, string bakedAsset, string note)
    {
        string meshContainer = "", meshName = "";
        if (meshRef != null && meshRef.StartsWith("@"))
        {
            int split = meshRef.IndexOf('|');
            meshContainer = meshRef.Substring(1, split - 1);
            meshName = meshRef.Substring(split + 1);
        }
        return $"{{\"action\": \"{action}\", \"scene\": \"{Escape(scene)}\", \"path\": \"{Escape(path)}\", " +
               $"\"oldScaleX\": {scale.x.ToString(System.Globalization.CultureInfo.InvariantCulture)}, " +
               $"\"oldScaleY\": {scale.y.ToString(System.Globalization.CultureInfo.InvariantCulture)}, " +
               $"\"oldScaleZ\": {scale.z.ToString(System.Globalization.CultureInfo.InvariantCulture)}, " +
               $"\"meshContainer\": \"{Escape(meshContainer)}\", \"meshName\": \"{Escape(meshName)}\", " +
               $"\"bakedAsset\": \"{Escape(bakedAsset ?? "")}\", \"note\": \"{Escape(note ?? "")}\"}}";
    }

    static string MeshRef(Mesh mesh)
    {
        if (mesh == null) return null;
        string assetPath = AssetDatabase.GetAssetPath(mesh);
        return "@" + assetPath + "|" + mesh.name;
    }

    static string DescribeMesh(Transform node)
    {
        var filter = node.GetComponent<MeshFilter>();
        return filter != null && filter.sharedMesh != null ? MeshRef(filter.sharedMesh) : null;
    }

    static Mesh LoadMesh(string containerPath, string meshName)
    {
        if (string.IsNullOrEmpty(containerPath)) return null;
        foreach (var asset in AssetDatabase.LoadAllAssetsAtPath(containerPath))
            if (asset is Mesh && asset.name == meshName)
                return (Mesh)asset;
        return null;
    }

    static Transform FindNode(UnityEngine.SceneManagement.Scene scene, string path)
    {
        string[] segments = path.Split('/');
        GameObject current = null;
        foreach (var root in scene.GetRootGameObjects())
            if (root.name == segments[0]) { current = root; break; }
        if (current == null) return null;
        for (int i = 1; i < segments.Length; i++)
        {
            Transform next = null;
            foreach (Transform child in current.transform)
                if (child.name == segments[i]) { next = child; break; }
            if (next == null) return null;
            current = next.gameObject;
        }
        return current.transform;
    }

    static bool IsReferencedByAnyScene(string assetGuid)
    {
        foreach (var scene in AssetDatabase.FindAssets("t:Scene"))
        {
            string path = AssetDatabase.GUIDToAssetPath(scene);
            if (File.Exists(path))
            {
                string text = File.ReadAllText(path);
                if (text.Contains(assetGuid)) return true;
            }
        }
        return false;
    }

    static bool NeedsAttention(Transform node)
    {
        return node.GetComponent<MeshFilter>() != null && node.childCount == 0 &&
               node.GetComponent(typeof(SkinnedMeshRenderer)) != null;
    }

    static string GetNodePath(Transform node)
    {
        string path = node.name;
        var t = node.parent;
        while (t != null)
        {
            path = t.name + "/" + path;
            t = t.parent;
        }
        return path;
    }

    static string Escape(string value)
    {
        return value == null ? "" : value.Replace("\\", "\\\\").Replace("\"", "\\\"");
    }

    static bool IsNormalizableLeaf(Transform node)
    {
        if (node.GetComponent<MeshFilter>() == null || node.GetComponent<MeshRenderer>() == null)
            return false;
        if (node.GetComponentInChildren<SkinnedMeshRenderer>(true) != null)
            return false;
        if (node.GetComponent<ParticleSystem>() != null || node.GetComponent<TrailRenderer>() != null)
            return false;
        Vector3 s = node.localScale;
        return Mathf.Abs(s.x - 1f) > 0.001f || Mathf.Abs(s.y - 1f) > 0.001f || Mathf.Abs(s.z - 1f) > 0.001f;
    }

    static bool NormalizeNode(Transform node)
    {
        MeshFilter filter = node.GetComponent<MeshFilter>();
        Mesh source = filter.sharedMesh;
        if (source == null)
            return false;

        Vector3 scale = node.localScale;
        Mesh baked = BakeMeshCopy(source, scale);
        if (baked == null)
            return false;

        string assetPath = PersistMesh(baked, source.name, node.name);
        if (string.IsNullOrEmpty(assetPath))
            return false;

        filter.sharedMesh = AssetDatabase.LoadAssetAtPath<Mesh>(assetPath);

        BoxCollider box = node.GetComponent<BoxCollider>();
        if (box != null)
        {
            box.center = Vector3.Scale(box.center, scale);
            box.size = Vector3.Scale(box.size, scale);
        }

        SphereCollider sphere = node.GetComponent<SphereCollider>();
        if (sphere != null)
        {
            float average = (Mathf.Abs(scale.x) + Mathf.Abs(scale.y) + Mathf.Abs(scale.z)) / 3f;
            sphere.radius *= average;
            sphere.center = Vector3.Scale(sphere.center, scale);
        }

        CapsuleCollider capsule = node.GetComponent<CapsuleCollider>();
        if (capsule != null)
        {
            float radial = Mathf.Max(Mathf.Abs(scale.x), Mathf.Abs(scale.z));
            capsule.radius *= radial;
            if (capsule.direction == 1)
                capsule.height *= Mathf.Abs(scale.y);
            else if (capsule.direction == 0)
                capsule.height *= Mathf.Abs(scale.x);
            else
                capsule.height *= Mathf.Abs(scale.z);
            capsule.center = Vector3.Scale(capsule.center, scale);
        }

        MeshCollider meshCollider = node.GetComponent<MeshCollider>();
        if (meshCollider != null)
            meshCollider.sharedMesh = filter.sharedMesh;

        node.localScale = Vector3.one;
        EditorUtility.SetDirty(node.gameObject);
        return true;
    }

    static Mesh BakeMeshCopy(Mesh source, Vector3 scale)
    {
        Mesh copy = Object.Instantiate(source);
        copy.name = source.name + "_Normalized";

        Vector3[] vertices = copy.vertices;
        for (int i = 0; i < vertices.Length; i++)
            vertices[i] = Vector3.Scale(vertices[i], scale);
        copy.vertices = vertices;

        Vector3[] normals = copy.normals;
        if (normals != null && normals.Length > 0)
        {
            Quaternion rotation = Quaternion.LookRotation(
                Vector3.Scale(Vector3.forward, scale),
                Vector3.Scale(Vector3.up, scale));
            Matrix4x4 normalMatrix = Matrix4x4.TRS(Vector3.zero, rotation, new Vector3(
                scale.x != 0f ? 1f / Mathf.Abs(scale.x) : 1f,
                scale.y != 0f ? 1f / Mathf.Abs(scale.y) : 1f,
                scale.z != 0f ? 1f / Mathf.Abs(scale.z) : 1f));
            for (int i = 0; i < normals.Length; i++)
                normals[i] = normalMatrix.MultiplyVector(normals[i]).normalized;
            copy.normals = normals;
        }

        copy.RecalculateBounds();
        return copy;
    }

    static string PersistMesh(Mesh mesh, string sourceName, string nodeName)
    {
        if (!Directory.Exists(OutputFolder))
            Directory.CreateDirectory(OutputFolder);

        string safeName = Sanitize(nodeName) + "_" + Sanitize(sourceName) + "_" + Mathf.Abs(mesh.GetInstanceID()) + ".asset";
        string path = Path.Combine(OutputFolder, safeName);
        try
        {
            AssetDatabase.CreateAsset(mesh, path);
        }
        catch (System.Exception exception)
        {
            Debug.LogError($"[FormalScaleNormalizer] failed to save mesh asset {path}: {exception.Message}");
            return null;
        }
        return path;
    }

    static string Sanitize(string name)
    {
        string safe = name.Replace('/', '_').Replace('\\', '_').Replace(' ', '_');
        foreach (char invalid in Path.GetInvalidFileNameChars())
            safe = safe.Replace(invalid, '_');
        return safe;
    }
}
