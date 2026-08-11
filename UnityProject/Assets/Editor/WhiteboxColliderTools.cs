using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class WhiteboxColliderTools
{
    private const string WhiteboxScenePath = "Assets/MoMing/Scenes/Test/superbreadman.unity";

    [MenuItem("Tools/SuperBreadMan/Replace Whitebox Mesh Colliders With Boxes")]
    public static void ReplaceWhiteboxMeshCollidersWithBoxes()
    {
        Scene scene = EditorSceneManager.OpenScene(WhiteboxScenePath, OpenSceneMode.Single);
        MeshCollider[] meshColliders = Object.FindObjectsOfType<MeshCollider>(true);
        int convertedCount = 0;

        foreach (MeshCollider meshCollider in meshColliders)
        {
            if (meshCollider.gameObject.scene != scene || meshCollider.sharedMesh == null)
                continue;

            Bounds bounds = meshCollider.sharedMesh.bounds;
            GameObject gameObject = meshCollider.gameObject;
            var boxCollider = Undo.AddComponent<BoxCollider>(gameObject);
            boxCollider.center = bounds.center;
            boxCollider.size = bounds.size;
            boxCollider.enabled = meshCollider.enabled;
            boxCollider.isTrigger = meshCollider.isTrigger;
            boxCollider.sharedMaterial = meshCollider.sharedMaterial;
            boxCollider.contactOffset = meshCollider.contactOffset;

            Undo.DestroyObjectImmediate(meshCollider);
            convertedCount++;
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log($"Converted {convertedCount} MeshCollider components to BoxCollider components in {WhiteboxScenePath}.");
    }
}
