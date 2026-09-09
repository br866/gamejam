using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Reflection;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
public static class ArtAudioBindingReview
{
    public static void Run()
    {
        var report = new StringBuilder(); int errors = 0, monsters = 0, ceramic = 0, metal = 0, wood = 0;
        foreach (string name in new[] { "FormalLevel01", "FormalLevel02", "FormalLevel03", "FormalLevel04", "FormalLevel045", "FormalLevel05", "FormalSharedArt_L01_L02", "FormalSharedArt_L02_L03", "FormalSharedArt_L03_L04", "FormalSharedArt_L04_L045", "FormalSharedArt_L045_L05" })
        {
            var scene = EditorSceneManager.OpenScene("Assets/MoMing/FormalLevels/" + name + ".unity", OpenSceneMode.Single);
            typeof(FormalAudioSurface).GetMethod("Reset", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, null);
            foreach (var root in scene.GetRootGameObjects())
            {
                foreach (var m in root.GetComponentsInChildren<MonsterPatrol>(true))
                {
                    string family = m.GetComponentsInChildren<Animator>().Select(FormalMonsterArtAudio.Identify).FirstOrDefault(f => f != null);
                    report.AppendLine(name + " monster=" + m.name + " family=" + family); monsters++; if (family == null) errors++;
                }
                foreach (var door in root.GetComponentsInChildren<FormalDoor>(true))
                {
                    var audio = door.gameObject.AddComponent<FormalMechanismAudioEmitter>(); audio.Initialize(door, null);
                    report.AppendLine(name + " door=" + door.name + " metal=" + audio.MetalDoor);
                    if (audio.MetalDoor) metal++; else wood++;
                }
                foreach (var renderer in root.GetComponentsInChildren<MeshRenderer>())
                {
                    if (!renderer.name.ToLowerInvariant().Contains("floor_tile_blue")) continue;
                    Bounds b = renderer.bounds; bool tile = FormalAudioSurface.IsCeramic(new Vector3(b.center.x, b.max.y + .2f, b.center.z));
                    if (!tile) errors++; ceramic++;
                }
            }
        }
        if (monsters != 3 || ceramic == 0 || metal == 0 || wood == 0) errors++;
        report.AppendLine($"monsters={monsters}; ceramic meshes={ceramic}; metal doors={metal}; wood doors={wood}; errors={errors}");
        File.WriteAllText(Path.Combine(Environment.GetEnvironmentVariable("SFX_REVIEW_OUTPUT"), "art-binding-validation.txt"), report.ToString());
        EditorApplication.Exit(errors == 0 ? 0 : 1);
    }
}
