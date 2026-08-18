using UnityEditor;
using UnityEngine;

public static class FormalDoorPrefabGenerator
{
    private const string OutputFolder = "Assets/MoMing/FormalLevels/Prefabs/Doors";

    private struct DoorDefinition
    {
        public string name;
        public string meshGuid;
        public string materialGuid;
    }

    private static readonly DoorDefinition[] Definitions =
    {
        new DoorDefinition { name = "FormalPivotDoor_Door3", meshGuid = "353bc00b702188a4990af49810ce7830", materialGuid = "4e5519b1c95658d4ead5f377390c3843" },
        new DoorDefinition { name = "FormalPivotDoor_MetalGrille", meshGuid = "", materialGuid = "" },
        new DoorDefinition { name = "FormalPivotDoor_BigDoor", meshGuid = "18194154474b9cc47b8f3b7624391376", materialGuid = "d5d453d807bfbc041ad9a9a3e23130a7" }
    };

    [MenuItem("Tools/SuperBreadMan/Formal Art/Generate Door Prefab Variants")]
    public static void GenerateDoorPrefabVariants()
    {
        EnsureFolder(OutputFolder);
        foreach (DoorDefinition definition in Definitions)
        {
            if (string.IsNullOrEmpty(definition.meshGuid))
            {
                Debug.LogWarning($"Skipped {definition.name}: mesh GUID is not configured yet.");
                continue;
            }

            string meshPath = AssetDatabase.GUIDToAssetPath(definition.meshGuid);
            Mesh mesh = FindMeshAsset(meshPath);
            if (mesh == null)
            {
                Debug.LogWarning($"Skipped {definition.name}: mesh asset not found for {definition.meshGuid}.");
                continue;
            }

            Material material = string.IsNullOrEmpty(definition.materialGuid)
                ? null
                : AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(definition.materialGuid));

            GameObject root = new GameObject(definition.name);
            GameObject pivot = new GameObject("DoorPivot");
            pivot.transform.SetParent(root.transform, false);
            GameObject leaf = new GameObject("DoorLeaf");
            leaf.transform.SetParent(pivot.transform, false);

            MeshFilter filter = leaf.AddComponent<MeshFilter>();
            filter.sharedMesh = mesh;
            MeshRenderer renderer = leaf.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
            MeshCollider collider = leaf.AddComponent<MeshCollider>();
            collider.sharedMesh = mesh;
            collider.sharedMaterial = AssetDatabase.LoadAssetAtPath<PhysicMaterial>(
                "Assets/MoMing/FormalLevels/Physics/FormalWallNoFriction.physicMaterial");

            FormalDoor door = root.AddComponent<FormalDoor>();
            SerializedObject serializedDoor = new SerializedObject(door);
            serializedDoor.FindProperty("visualPivot").objectReferenceValue = pivot.transform;
            serializedDoor.FindProperty("blockingCollider").objectReferenceValue = collider;
            serializedDoor.ApplyModifiedPropertiesWithoutUndo();

            string prefabPath = $"{OutputFolder}/{definition.name}.prefab";
            PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            Object.DestroyImmediate(root);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    static void EnsureFolder(string path)
    {
        string[] parts = path.Split('/');
        string current = parts[0];
        for (int i = 1; i < parts.Length; i++)
        {
            string next = current + "/" + parts[i];
            if (!AssetDatabase.IsValidFolder(next))
                AssetDatabase.CreateFolder(current, parts[i]);
            current = next;
        }
    }

    static Mesh FindMeshAsset(string assetPath)
    {
        foreach (Object asset in AssetDatabase.LoadAllAssetsAtPath(assetPath))
        {
            Mesh mesh = asset as Mesh;
            if (mesh != null)
                return mesh;
        }

        return null;
    }
}
