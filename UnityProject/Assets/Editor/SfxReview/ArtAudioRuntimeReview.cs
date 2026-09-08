using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Reflection;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public sealed class ArtAudioReviewMotion : MonoBehaviour
{
    public bool Move = true;
    private void Update() { if (Move) transform.position += Vector3.right * (2.5f * Time.deltaTime); }
}

[InitializeOnLoad]
public static class ArtAudioRuntimeReview
{
    static string Out => Environment.GetEnvironmentVariable("SFX_REVIEW_OUTPUT");
    const string Active = "ArtAudioReview.Active", Finish = "ArtAudioReview.Finish";
    static readonly string[] events = { "Play_Human_Tile", "Play_DoorMetal_Move", "Play_DoorMetal_Close", "Play_Asylum_Bed", "Play_Asylum_Window", "Play_Heavy_Presence", "Play_Uniform_Presence", "Play_Gaunt_Presence", "Play_Heavy_Attack", "Play_Uniform_Attack", "Play_Gaunt_Attack" };
    static readonly string[] roles = { "MonsterA", "Monster2", "MonsterC" };
    static readonly string[] families = { "Heavy", "Uniform", "Gaunt" };
    static readonly string[] walks = { "monster3 walk", "monster2 walk", "monster1 walk1" };
    static readonly StringBuilder report = new StringBuilder();
    static double start, mark;
    static int stage, index, errors, contacts;
    static GameObject source, actor;
    static FormalMonsterArtAudio sound;
    static readonly BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;
    static ArtAudioRuntimeReview() { EditorApplication.update += Tick; }
    static void Check(string key, bool value) { report.AppendLine(key + "=" + value); if (!value) errors++; }
    public static void Run()
    {
        SessionState.SetBool(Active, true); SessionState.SetBool(Finish, false);
        EditorSceneManager.OpenScene("Assets/MoMing/FormalLevels/FormalPersistent.unity", OpenSceneMode.Single);
        EditorApplication.EnterPlaymode();
    }
    static void Tick()
    {
        if (!SessionState.GetBool(Active, false)) return;
        if (SessionState.GetBool(Finish, false))
        {
            if (!EditorApplication.isPlayingOrWillChangePlaymode) { SessionState.SetBool(Active, false); EditorApplication.Exit(SessionState.GetInt("ArtAudioReview.Errors", 1)); }
            return;
        }
        if (!EditorApplication.isPlaying || EditorApplication.isCompiling) return;
        if (start == 0) start = EditorApplication.timeSinceStartup;
        double t = EditorApplication.timeSinceStartup - start;
        if (AkUnitySoundEngine.IsInitialized()) AkUnitySoundEngine.WakeupFromSuspend();
        if (stage == 0 && t > 15)
        {
            Check("EngineReady", AkUnitySoundEngine.IsInitialized());
            foreach (var behaviour in UnityEngine.Object.FindObjectsOfType<MonoBehaviour>())
            {
                if (behaviour.GetType().Name.StartsWith("Ak", StringComparison.Ordinal)) continue;
                behaviour.StopAllCoroutines(); behaviour.enabled = false;
            }
            foreach (var collider in UnityEngine.Object.FindObjectsOfType<Collider>()) collider.enabled = false;
            foreach (var body in UnityEngine.Object.FindObjectsOfType<Rigidbody>()) body.isKinematic = true;
            AkUnitySoundEngine.StopAll();
            source = new GameObject("Art audio review source"); source.AddComponent<AkGameObj>();
            var listener = UnityEngine.Object.FindObjectOfType<AkAudioListener>();
            source.transform.position = listener.transform.position;
            AkUnitySoundEngine.SetRTPCValue("SFXVolume", 100); AkUnitySoundEngine.SetRTPCValue("AmbienceVolume", 100);
            foreach (string name in events) Resources.Load<WwiseEventReference>("AsylumAudio/" + name);
            FormalSfxEvents.Preload(); mark = t; stage = 1;
            Check("PhaseWrap", FormalMonsterArtAudio.ContactCrossed(.99f, 1.09f, .075f));
            Check("PhaseRewindSilent", !FormalMonsterArtAudio.ContactCrossed(.8f, .1f, .075f));
            Check("PhaseSeekSilent", !FormalMonsterArtAudio.ContactCrossed(0, 2, .075f));
            foreach (string walk in walks) Check(walk + " calibrated", FormalMonsterArtAudio.ContactPhases(walk, out _, out _));
        }
        else if (stage == 1 && t > mark + 4)
        {
            AkUnitySoundEngine.StopAll();
            Check(events[index] + " capture", AkUnitySoundEngine.StartOutputCapture(Path.Combine(Out, "runtime-" + events[index] + ".wav")) == AKRESULT.AK_Success);
            Check(events[index] + " post", FormalSfxEvents.Post(events[index], source) != 0);
            mark = t; stage = 2;
        }
        else if (stage == 2 && t > mark + 3)
        {
            AkUnitySoundEngine.StopOutputCapture(); AkUnitySoundEngine.StopAll(); index++;
            if (index < events.Length) { mark = t - 4; stage = 1; }
            else { index = 0; stage = 3; }
        }
        else if (stage == 3)
        {
            actor = UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/MoMing/FormalLevels/Prefabs/Monster/" + roles[index] + ".prefab"));
            actor.name = "Unrelated object name";
            actor.transform.position = source.transform.position - Vector3.right * 5;
            foreach (var b in actor.GetComponentsInChildren<MonoBehaviour>()) if (!(b is AkGameObj)) b.enabled = false;
            foreach (var c in actor.GetComponentsInChildren<Collider>()) c.enabled = false;
            foreach (var b in actor.GetComponentsInChildren<Rigidbody>()) b.isKinematic = true;
            var patrol = actor.GetComponent<MonsterPatrol>(); patrol.waypoints = new[] { actor.transform }; patrol.patrolSpeed = 0; patrol.detectionRange = 0; patrol.catchRadius = 0; patrol.enabled = true;
            var animator = actor.GetComponentInChildren<Animator>(); animator.cullingMode = AnimatorCullingMode.AlwaysAnimate; animator.Play(walks[index], 0, 0);
            actor.AddComponent<ArtAudioReviewMotion>(); sound = actor.AddComponent<FormalMonsterArtAudio>();
            Check(roles[index] + " visual identity", sound.Family == families[index]);
            AkUnitySoundEngine.StartOutputCapture(Path.Combine(Out, "runtime-walk-" + families[index] + ".wav"));
            mark = t; stage = 4;
        }
        else if (stage == 4 && t > mark + 4)
        {
            AkUnitySoundEngine.StopOutputCapture();
            report.AppendLine(families[index] + " contacts=" + sound.ContactCount);
            Check(families[index] + " animated steps", sound.ContactCount >= (index == 2 ? 2 : 5));
            actor.GetComponent<ArtAudioReviewMotion>().Move = false; contacts = sound.ContactCount; mark = t; stage = 5;
        }
        else if (stage == 5 && t > mark + .4)
        {
            Check(families[index] + " stationary silent", sound.ContactCount == contacts);
            actor.transform.position += Vector3.right * 25; mark = t; stage = 6;
        }
        else if (stage == 6 && t > mark + .2)
        {
            Check(families[index] + " teleport silent", sound.ContactCount == contacts);
            Time.timeScale = 0; mark = t; stage = 7;
        }
        else if (stage == 7 && t > mark + .2)
        {
            Check(families[index] + " pause stops", (uint)typeof(FormalMonsterArtAudio).GetField("footId", Private).GetValue(sound) == 0);
            Time.timeScale = 1; actor.SetActive(false);
            Check(families[index] + " disable relinquishes", !sound.OwnsFootsteps);
            UnityEngine.Object.Destroy(actor); index++; stage = index < roles.Length ? 3 : 8;
        }
        else if (stage == 8) End();
        if (t > 140) { Check("TimedOut", false); End(); }
    }
    static void End()
    {
        Time.timeScale = 1; AkUnitySoundEngine.StopOutputCapture();
        report.AppendLine("Errors=" + errors); File.WriteAllText(Path.Combine(Out, "art-runtime-validation.txt"), report.ToString());
        SessionState.SetInt("ArtAudioReview.Errors", errors == 0 ? 0 : 1); SessionState.SetBool(Finish, true); EditorApplication.ExitPlaymode();
    }
}
