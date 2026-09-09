using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>Uses the supplied blue ceramic floor meshes; worn stone is the default.</summary>
public static class FormalAudioSurface
{
    private static readonly List<Renderer> tiles = new List<Renderer>();
    private static float nextScan;
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void Reset()
    {
        tiles.Clear(); nextScan = 0;
        SceneManager.sceneLoaded -= Loaded; SceneManager.sceneLoaded += Loaded;
        SceneManager.sceneUnloaded -= Unloaded; SceneManager.sceneUnloaded += Unloaded;
    }
    private static void Loaded(Scene scene, LoadSceneMode mode) { tiles.Clear(); nextScan = 0; }
    private static void Unloaded(Scene scene) { tiles.Clear(); nextScan = 0; }
    public static bool IsCeramic(Vector3 feet)
    {
        if (Time.unscaledTime >= nextScan)
        {
            tiles.Clear();
            foreach (var renderer in Object.FindObjectsOfType<MeshRenderer>())
            {
                var mesh = renderer.GetComponent<MeshFilter>();
                if (renderer.name.ToLowerInvariant().Contains("floor_tile_blue") ||
                    (mesh != null && mesh.sharedMesh != null && mesh.sharedMesh.name.ToLowerInvariant().Contains("floor_tile_blue")))
                    tiles.Add(renderer);
            }
            nextScan = Time.unscaledTime + 3f;
        }
        foreach (var tile in tiles)
        {
            if (tile == null || !tile.enabled || !tile.gameObject.activeInHierarchy) continue;
            Bounds b = tile.bounds;
            if (feet.x >= b.min.x && feet.x <= b.max.x && feet.z >= b.min.z && feet.z <= b.max.z && Mathf.Abs(feet.y - b.max.y) < 2f) return true;
        }
        return false;
    }
}
