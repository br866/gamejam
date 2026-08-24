using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

public class FormalMeshColliderSyncAuditorTests
{
    GameObject _root;
    Mesh _cube;
    Mesh _sphere;
    Mesh _bigCube;

    [SetUp]
    public void SetUp()
    {
        _root = new GameObject("AuditorTestRoot");
        GameObject cubePrimitive = GameObject.CreatePrimitive(PrimitiveType.Cube);
        _cube = cubePrimitive.GetComponent<MeshFilter>().sharedMesh;
        GameObject spherePrimitive = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        _sphere = spherePrimitive.GetComponent<MeshFilter>().sharedMesh;
        Object.DestroyImmediate(cubePrimitive);
        Object.DestroyImmediate(spherePrimitive);

        _bigCube = Object.Instantiate(_cube);
        Vector3[] vertices = _bigCube.vertices;
        for (int i = 0; i < vertices.Length; i++)
            vertices[i] *= 10f;
        _bigCube.vertices = vertices;
        _bigCube.RecalculateBounds();
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(_root);
        Object.DestroyImmediate(_bigCube);
    }

    GameObject MakeNode(string name, Transform parent, Mesh filterMesh, Mesh colliderMesh)
    {
        var node = new GameObject(name);
        node.transform.SetParent(parent, false);
        node.AddComponent<MeshFilter>().sharedMesh = filterMesh;
        node.AddComponent<MeshRenderer>();
        node.AddComponent<MeshCollider>().sharedMesh = colliderMesh;
        return node;
    }

    static MeshColliderSyncAuditor.Issue FindByNodeName(List<MeshColliderSyncAuditor.Issue> issues, string name)
    {
        return issues.Find(issue => issue.GameObject.name == name);
    }

    [Test]
    public void ClassifiesSyncProxyBrokenAndMissingPairs()
    {
        MakeNode("ok", _root.transform, _cube, _cube);
        MakeNode("proxy", _root.transform, _cube, _sphere);
        MakeNode("broken", _root.transform, _cube, _bigCube);
        MakeNode("missing", _root.transform, _cube, null);

        List<MeshColliderSyncAuditor.Issue> issues = MeshColliderSyncAuditor.Audit(new[] { _root });

        Assert.AreEqual(MeshColliderSyncAuditor.SyncState.Ok, FindByNodeName(issues, "ok").State);
        Assert.AreEqual(MeshColliderSyncAuditor.SyncState.Proxy, FindByNodeName(issues, "proxy").State);
        Assert.AreEqual(MeshColliderSyncAuditor.SyncState.Broken, FindByNodeName(issues, "broken").State);
        Assert.AreEqual(MeshColliderSyncAuditor.SyncState.Missing, FindByNodeName(issues, "missing").State);
        Assert.AreEqual(2, MeshColliderSyncAuditor.CountFixable(issues));
    }

    [Test]
    public void FixReassignsOnlyBrokenAndMissingColliders()
    {
        GameObject proxyNode = MakeNode("proxy", _root.transform, _cube, _sphere);
        GameObject brokenNode = MakeNode("broken", _root.transform, _cube, _bigCube);
        GameObject missingNode = MakeNode("missing", _root.transform, _cube, null);

        List<MeshColliderSyncAuditor.Issue> issues = MeshColliderSyncAuditor.Audit(new[] { _root });
        int fixedCount = MeshColliderSyncAuditor.Fix(issues);

        Assert.AreEqual(2, fixedCount);
        Assert.AreEqual(_cube, brokenNode.GetComponent<MeshCollider>().sharedMesh);
        Assert.AreEqual(_cube, missingNode.GetComponent<MeshCollider>().sharedMesh);
        Assert.AreEqual(_sphere, proxyNode.GetComponent<MeshCollider>().sharedMesh);

        List<MeshColliderSyncAuditor.Issue> reaudited = MeshColliderSyncAuditor.Audit(new[] { _root });
        Assert.AreEqual(0, MeshColliderSyncAuditor.CountFixable(reaudited));
        Assert.AreEqual(MeshColliderSyncAuditor.SyncState.Proxy, FindByNodeName(reaudited, "proxy").State);
    }

    [Test]
    public void L01ContentPrefabHasNoBrokenMeshColliderPairs()
    {
        AssertContentPrefabHasNoViolations("Assets/MoMing/FormalLevels/Prefabs/L01_Content.prefab");
    }

    [Test]
    public void L03ContentPrefabHasNoBrokenMeshColliderPairs()
    {
        AssertContentPrefabHasNoViolations("Assets/MoMing/FormalLevels/Prefabs/L03_Content.prefab");
    }

    static void AssertContentPrefabHasNoViolations(string path)
    {
        GameObject contents = PrefabUtility.LoadPrefabContents(path);
        try
        {
            List<MeshColliderSyncAuditor.Issue> issues = MeshColliderSyncAuditor.Audit(new[] { contents });
            foreach (MeshColliderSyncAuditor.Issue issue in issues)
                Debug.Log("[MeshColliderSyncAuditorTests] " + path + " " + MeshColliderSyncAuditor.Describe(issue));
            Assert.Greater(issues.Count, 0, "expected at least one filter+collider pair under " + path);
            Assert.AreEqual(0, MeshColliderSyncAuditor.CountFixable(issues), path);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(contents);
        }
    }
}
