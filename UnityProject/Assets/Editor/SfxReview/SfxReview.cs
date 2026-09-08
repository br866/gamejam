using System;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class SfxReview
{
    public static void Validate()
    {
        var report = new StringBuilder();
        int errors = 0;
        string dir = Environment.GetEnvironmentVariable("SFX_REVIEW_OUTPUT");
        foreach (string role in new[] { "Monster2", "MonsterA", "MonsterC" })
        {
            var go = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/MoMing/FormalLevels/Prefabs/Monster/" + role + ".prefab");
            var patrol = go.GetComponent<MonsterPatrol>();
            var so = new SerializedObject(patrol);
            var reference = so.FindProperty("footstepEvent").FindPropertyRelative("WwiseObjectReference").objectReferenceValue as WwiseEventReference;
            bool valid = reference != null && reference.Id != 0;
            report.AppendLine(role + " footstep valid=" + valid);
            if (!valid) errors++;
        }
        foreach (string kind in new[] { "Draft", "Grille", "Pipe" })
        {
            var reference = Resources.Load<WwiseEventReference>("AsylumAudio/Play_Asylum_" + kind);
            bool valid = reference != null && reference.Id != 0;
            report.AppendLine(kind + " event resource valid=" + valid);
            if (!valid) errors++;
        }
        foreach (string scene in new[] { "FormalPersistent", "FormalLevel01", "FormalLevel02", "FormalLevel03", "FormalLevel04", "FormalLevel045", "FormalLevel05", "FormalSharedArt_L01_L02", "FormalSharedArt_L02_L03", "FormalSharedArt_L03_L04", "FormalSharedArt_L04_L045", "FormalSharedArt_L045_L05" })
        {
            var s = EditorSceneManager.OpenScene("Assets/MoMing/FormalLevels/" + scene + ".unity", OpenSceneMode.Single);
            var objects = s.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<Transform>(true)).ToArray();
            var props = objects.Where(t => t.GetComponent<Renderer>() != null && (t.name.ToLowerInvariant().Contains("grille") || t.name.ToLowerInvariant().Contains("pipe") || t.name.ToLowerInvariant().Contains("window") || t.name.ToLowerInvariant().Contains("vent"))).ToArray();
            report.AppendLine(scene + " ambient geometry=" + props.Length);
            foreach (var t in props) report.AppendLine("  " + t.name + " " + t.GetComponent<Renderer>().bounds.center);
            foreach (var m in objects.Select(t => t.GetComponent<MonsterPatrol>()).Where(m => m != null))
            {
                var so = new SerializedObject(m);
                var r = so.FindProperty("footstepEvent").FindPropertyRelative("WwiseObjectReference").objectReferenceValue as WwiseEventReference;
                report.AppendLine("  monster " + m.name + " valid=" + (r != null));
                if (r == null) errors++;
            }
        }
        report.AppendLine("Errors=" + errors);
        File.WriteAllText(Path.Combine(dir, "unity-scene-audit.txt"), report.ToString());
        Debug.Log("SFX_REVIEW_COMPLETE errors=" + errors);
        EditorApplication.Exit(errors == 0 ? 0 : 1);
    }
}
