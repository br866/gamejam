using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public static class SfxPlaybackReview
{
    private const string Key = "SfxReview.Playback";
    private static double entered;
    private static bool posted;
    private static bool cleanupChecked;
    private static readonly StringBuilder Report = new StringBuilder();
    static SfxPlaybackReview() { EditorApplication.update += Tick; }
    public static void Run()
    {
        SessionState.SetBool(Key, true);
        EditorSceneManager.OpenScene("Assets/MoMing/FormalLevels/FormalPersistent.unity", OpenSceneMode.Single);
        EditorApplication.EnterPlaymode();
    }
    private static void Tick()
    {
        if (!SessionState.GetBool(Key, false)) return;
        if (!EditorApplication.isPlaying || EditorApplication.isCompiling) return;
        if (AkUnitySoundEngine.IsInitialized()) AkUnitySoundEngine.WakeupFromSuspend();
        if (entered == 0) entered = EditorApplication.timeSinceStartup;
        double elapsed = EditorApplication.timeSinceStartup - entered;
        if (!posted && elapsed > 18)
        {
            posted = true;
            Report.AppendLine("CaptureStart=" + AkUnitySoundEngine.StartOutputCapture(Path.Combine(Environment.GetEnvironmentVariable("SFX_REVIEW_OUTPUT"), "runtime-functional-probe.wav")));
            Report.AppendLine("SoundEngineInitialized=" + AkUnitySoundEngine.IsInitialized());
            Report.AppendLine("Listeners=" + UnityEngine.Object.FindObjectsOfType<AkAudioListener>().Length);
            for (int i = 0; i < SceneManager.sceneCount; i++) Report.AppendLine("Scene=" + SceneManager.GetSceneAt(i).name);
            var actor = UnityEngine.Object.FindObjectOfType<FormalPlayerActor>();
            GameObject source = actor != null ? actor.gameObject : new GameObject("ReviewSource");
            foreach (string name in new[] { "Play_Footstep_Human", "Play_Footstep_Dog", "Play_Footstep_Brutedoc", "Play_Asylum_Draft", "Play_Asylum_Grille", "Play_Asylum_Pipe" })
            {
                // Reference asset load exercises the project's normal auto-bank lifecycle.
                var reference = name.StartsWith("Play_Asylum_")
                    ? Resources.Load<WwiseEventReference>("AsylumAudio/" + name)
                    : AssetDatabase.FindAssets("t:WwiseEventReference").Select(g => AssetDatabase.LoadAssetAtPath<WwiseEventReference>(AssetDatabase.GUIDToAssetPath(g))).FirstOrDefault(r => r.ObjectName == name);
                if (reference == null) { Report.AppendLine(name + " missing reference"); continue; }
                var ev = new AK.Wwise.Event { WwiseObjectReference = reference };
                uint id = ev.Post(source);
                Report.AppendLine(name + " autoBankLoaded=" + reference.IsAutoBankLoaded + " playingId=" + id);
            }
            var router = UnityEngine.Object.FindObjectOfType<FormalAsylumAudioRouter>();
            Report.AppendLine("Router=" + (router != null));
            if (router != null)
            {
                var lists = (System.Collections.IList[])typeof(FormalAsylumAudioRouter).GetField("candidates", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(router);
                for (int i = 0; i < lists.Length; i++) Report.AppendLine("CandidateGroup" + i + "=" + lists[i].Count);
            }
            Camera c = Camera.main;
            if (c != null)
            {
                var rt = new RenderTexture(1280, 720, 24);var old = c.targetTexture;var active = RenderTexture.active;
                c.targetTexture = rt;c.Render();RenderTexture.active = rt;
                var tex = new Texture2D(1280, 720, TextureFormat.RGB24, false);tex.ReadPixels(new Rect(0, 0, 1280, 720), 0, 0);tex.Apply();
                File.WriteAllBytes(Path.Combine(Environment.GetEnvironmentVariable("SFX_REVIEW_OUTPUT"), "formal-scene-check.png"), tex.EncodeToPNG());
                c.targetTexture = old;RenderTexture.active = active;UnityEngine.Object.Destroy(rt);UnityEngine.Object.Destroy(tex);
            }
        }
        if (posted && !cleanupChecked && elapsed > 28)
        {
            cleanupChecked = true;
            var router = UnityEngine.Object.FindObjectOfType<FormalAsylumAudioRouter>();
            var flags = BindingFlags.NonPublic | BindingFlags.Instance;
            var lists = (System.Collections.IList[])typeof(FormalAsylumAudioRouter).GetField("candidates", flags).GetValue(router);
            var due = (float[])typeof(FormalAsylumAudioRouter).GetField("next", flags).GetValue(router);
            if (lists[0].Count > 0 && Camera.main != null)
            {
                Vector3 old = Camera.main.transform.position;
                Camera.main.transform.position = ((Transform)lists[0][0]).GetComponent<Renderer>().bounds.center + Vector3.up;
                due[0] = -1;
                typeof(FormalAsylumAudioRouter).GetMethod("Update", flags).Invoke(router, null);
                var ids = (uint[])typeof(FormalAsylumAudioRouter).GetField("playing", flags).GetValue(router);
                Report.AppendLine("ScheduledDraftPlayingId=" + ids[0]);
                router.enabled = false;
                Report.AppendLine("DisableClearsAllPlayingIds=" + ids.All(id => id == 0));
                Camera.main.transform.position = old;
            }
        }
        if (posted && elapsed > 34)
        {
            Report.AppendLine("CaptureStop=" + AkUnitySoundEngine.StopOutputCapture());
            File.WriteAllText(Path.Combine(Environment.GetEnvironmentVariable("SFX_REVIEW_OUTPUT"), "unity-playback-audit.txt"), Report.ToString());
            SessionState.SetBool(Key, false);
            EditorApplication.ExitPlaymode();
            // Exit is queued after play-mode shutdown callbacks have released Wwise.
            EditorApplication.delayCall += () => EditorApplication.Exit(0);
        }
        if (elapsed > 75)
        {
            SessionState.SetBool(Key, false);
            File.WriteAllText(Path.Combine(Environment.GetEnvironmentVariable("SFX_REVIEW_OUTPUT"), "unity-playback-audit.txt"), Report + "\nTimed out");
            EditorApplication.Exit(1);
        }
    }
}
