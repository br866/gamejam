using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class MeshColliderSyncAuditor
{
    public enum SyncState
    {
        Ok,
        Proxy,
        Broken,
        Missing
    }

    public class Issue
    {
        public GameObject GameObject;
        public SyncState State;
        public Mesh FilterMesh;
        public Mesh ColliderMesh;
        public Vector3 RendererWorldSize;
        public Vector3 ColliderWorldSize;
    }

    const float AxisTolerance = 0.1f;

    [MenuItem("Tools/Final/Audit MeshCollider Sync On Selection (Report Only)")]
    static void AuditSelectionMenu()
    {
        if (Selection.gameObjects == null || Selection.gameObjects.Length == 0)
        {
            Debug.LogWarning("[MeshColliderSyncAuditor] select at least one GameObject first.");
            return;
        }

        List<Issue> issues = Audit(Selection.gameObjects);
        LogReport(issues, "audit");
    }

    [MenuItem("Tools/Final/Fix MeshCollider Sync On Selection")]
    static void FixSelectionMenu()
    {
        if (Selection.gameObjects == null || Selection.gameObjects.Length == 0)
        {
            Debug.LogWarning("[MeshColliderSyncAuditor] select at least one GameObject first.");
            return;
        }

        List<Issue> issues = Audit(Selection.gameObjects);
        int fixable = CountFixable(issues);
        if (fixable == 0)
        {
            Debug.Log("[MeshColliderSyncAuditor] nothing to fix under selection.");
            return;
        }

        if (!EditorUtility.DisplayDialog(
                "Fix MeshCollider Sync",
                fixable + " broken/missing MeshCollider mesh reference(s) found under selection.\n\nRepoint them to each renderer's current mesh?",
                "Fix",
                "Cancel"))
            return;

        int fixedCount = Fix(issues);
        LogReport(issues, "post-fix");
        Debug.Log($"[MeshColliderSyncAuditor] fixed {fixedCount} of {fixable} issue(s).");
    }

    public static List<Issue> Audit(IEnumerable<GameObject> roots)
    {
        var issues = new List<Issue>();
        if (roots == null)
            return issues;

        var visitedColliders = new HashSet<Object>();
        foreach (GameObject root in roots)
        {
            if (root == null)
                continue;

            foreach (Transform node in root.GetComponentsInChildren<Transform>(true))
            {
                MeshFilter filter = node.GetComponent<MeshFilter>();
                MeshCollider collider = node.GetComponent<MeshCollider>();
                if (filter == null || collider == null || filter.sharedMesh == null)
                    continue;
                if (!visitedColliders.Add(collider))
                    continue;

                Mesh filterMesh = filter.sharedMesh;
                Mesh colliderMesh = collider.sharedMesh;
                Vector3 rendererSize = GetRenderWorldSize(node.gameObject, filterMesh);
                Vector3 colliderSize = collider.bounds.size;

                SyncState state;
                if (colliderMesh == null)
                    state = SyncState.Missing;
                else if (colliderMesh == filterMesh)
                    state = SyncState.Ok;
                else if (BoundsMatchWithinTolerance(rendererSize, colliderSize))
                    state = SyncState.Proxy;
                else
                    state = SyncState.Broken;

                issues.Add(new Issue
                {
                    GameObject = node.gameObject,
                    State = state,
                    FilterMesh = filterMesh,
                    ColliderMesh = colliderMesh,
                    RendererWorldSize = rendererSize,
                    ColliderWorldSize = colliderSize
                });
            }
        }

        return issues;
    }

    public static int CountFixable(List<Issue> issues)
    {
        int count = 0;
        if (issues == null)
            return count;
        foreach (Issue issue in issues)
            if (issue.State == SyncState.Broken || issue.State == SyncState.Missing)
                count++;
        return count;
    }

    public static int Fix(List<Issue> issues)
    {
        int count = 0;
        if (issues == null)
            return count;

        foreach (Issue issue in issues)
        {
            if (issue.State != SyncState.Broken && issue.State != SyncState.Missing)
                continue;
            if (issue.GameObject == null)
                continue;

            MeshCollider collider = issue.GameObject.GetComponent<MeshCollider>();
            MeshFilter filter = issue.GameObject.GetComponent<MeshFilter>();
            if (collider == null || filter == null || filter.sharedMesh == null)
                continue;

            SerializedObject serialized = new SerializedObject(collider);
            serialized.FindProperty("m_Mesh").objectReferenceValue = filter.sharedMesh;
            serialized.ApplyModifiedProperties();
            EditorUtility.SetDirty(collider);
            count++;
        }

        return count;
    }

    public static string Describe(Issue issue)
    {
        string path = issue.GameObject.name;
        Transform parent = issue.GameObject.transform.parent;
        while (parent != null)
        {
            path = parent.name + "/" + path;
            parent = parent.parent;
        }

        string colliderMesh = issue.ColliderMesh != null ? AssetDatabase.GetAssetPath(issue.ColliderMesh) : "null";
        string rendererSizeText = issue.RendererWorldSize.ToString("F2");
        string colliderSizeText = issue.ColliderWorldSize.ToString("F2");
        return $"{issue.State} [{path}] renderer={rendererSizeText} collider={colliderSizeText} filterMesh={AssetDatabase.GetAssetPath(issue.FilterMesh)} colliderMesh={colliderMesh}";
    }

    static Vector3 GetRenderWorldSize(GameObject gameObject, Mesh filterMesh)
    {
        MeshRenderer renderer = gameObject.GetComponent<MeshRenderer>();
        if (renderer != null)
            return renderer.bounds.size;

        Vector3 halfExtent = Vector3.Scale(filterMesh.bounds.extents, gameObject.transform.lossyScale);
        return halfExtent * 2f;
    }

    static bool BoundsMatchWithinTolerance(Vector3 a, Vector3 b)
    {
        for (int i = 0; i < 3; i++)
        {
            float max = Mathf.Max(Mathf.Abs(a[i]), Mathf.Abs(b[i]));
            if (max < 1e-4f)
                continue;
            if (Mathf.Abs(a[i] - b[i]) > max * AxisTolerance)
                return false;
        }

        return true;
    }

    static void LogReport(List<Issue> issues, string phase)
    {
        int ok = 0, proxy = 0, broken = 0, missing = 0;
        foreach (Issue issue in issues)
        {
            switch (issue.State)
            {
                case SyncState.Ok:
                    ok++;
                    break;
                case SyncState.Proxy:
                    proxy++;
                    Debug.Log("[MeshColliderSyncAuditor/" + phase + "] " + Describe(issue));
                    break;
                case SyncState.Broken:
                    broken++;
                    Debug.LogWarning("[MeshColliderSyncAuditor/" + phase + "] " + Describe(issue));
                    break;
                case SyncState.Missing:
                    missing++;
                    Debug.LogWarning("[MeshColliderSyncAuditor/" + phase + "] " + Describe(issue));
                    break;
            }
        }

        Debug.Log($"[MeshColliderSyncAuditor/{phase}] scanned={issues.Count} ok={ok} proxy={proxy} broken={broken} missing={missing}");
    }
}
