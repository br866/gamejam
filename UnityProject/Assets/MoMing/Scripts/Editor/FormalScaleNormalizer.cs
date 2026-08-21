using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 把选中物体下"无子节点的静态网格叶子"的非均匀/超大缩放烘焙进网格副本，
/// 然后把该节点缩放复位为 1。同步补偿同节点上的 Box/Sphere/Capsule/Mesh 碰撞体。
/// 刻意不处理：带子节点的节点、SkinnedMeshRenderer、粒子，避免破坏动画与层级。
/// </summary>
public static class FormalScaleNormalizer
{
    const string OutputFolder = "Assets/MoMing/BakedMeshes";

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