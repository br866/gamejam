using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
public static class SfxDeepReview
{
    private const string Active = "SfxDeepReview.Active";
    private const string Finish = "SfxDeepReview.Finish";
    private static readonly StringBuilder report = new StringBuilder();
    private static double started;
    private static int phase;
    private static GameObject source;
    private static GameObject doorObject;
    private static GameObject gateObject;
    private static FormalMechanismAudioEmitter doorEmitter;
    private static FormalMechanismAudioEmitter gateEmitter;
    private static int errors;
    private static double mechanismStarted;
    private static double closeStarted;
    private static string Output => Environment.GetEnvironmentVariable("SFX_REVIEW_OUTPUT");
    private static readonly BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;
    static SfxDeepReview() { EditorApplication.update += Tick; }
    private static void Check(string name, bool value)
    {
        report.AppendLine(name + "=" + value);
        if (!value) errors++;
    }
    public static void Run()
    {
        SessionState.SetBool(Active, true);
        SessionState.SetBool(Finish, false);
        EditorSceneManager.OpenScene("Assets/MoMing/FormalLevels/FormalPersistent.unity", OpenSceneMode.Single);
        EditorApplication.EnterPlaymode();
    }
    private static void TrackerChecks()
    {
        var t = new FootstepDistanceTracker();t.Reset(Vector3.zero);
        bool first = t.Tick(new Vector3(1.2f, 0, 0), 2f, true, .1f);
        bool back = t.Tick(Vector3.zero, 2f, true, .1f);
        Check("ZigzagTravelEmits", !first && back);
        t.Reset(Vector3.zero);Check("TeleportSilent", !t.Tick(Vector3.right * 20, 2, true, .02f));
        t.Reset(Vector3.zero);Check("VerticalTravelSilent", !t.Tick(Vector3.up * 3, 2, true, .02f));
        t.Reset(Vector3.zero);t.Tick(Vector3.right, 2, true, .1f);t.Tick(Vector3.right, 2, true, .2f);
        Check("StationaryReset", !t.Tick(Vector3.right * 2, 2, true, .1f));
        t.Reset(Vector3.zero);t.Tick(Vector3.right * 1.5f, 2, true, .1f);t.Tick(Vector3.right * 2, 2, false, .1f);
        Check("PauseReset", !t.Tick(Vector3.right * 2.7f, 2, true, .1f));
        t.Reset(Vector3.zero);int highFrameSteps = 0;
        for (int i = 1; i <= 12000; i++)
            if (t.Tick(Vector3.right * (i * .0002f), 1f, true, .0001f)) highFrameSteps++;
        Check("HighFrameRateTravelEmits", highFrameSteps == 2);
    }
    private static void Tick()
    {
        if (!SessionState.GetBool(Active, false)) return;
        if (SessionState.GetBool(Finish, false))
        {
            if (!EditorApplication.isPlayingOrWillChangePlaymode)
            {
                SessionState.SetBool(Active, false);SessionState.SetBool(Finish, false);
                EditorApplication.Exit(SessionState.GetInt("SfxDeepReview.Errors", 1));
            }
            return;
        }
        if (!EditorApplication.isPlaying || EditorApplication.isCompiling) return;
        if (started == 0) started = EditorApplication.timeSinceStartup;
        double t = EditorApplication.timeSinceStartup - started;
        if (AkUnitySoundEngine.IsInitialized()) AkUnitySoundEngine.WakeupFromSuspend();
        if (phase == 0 && t > 15)
        {
            TrackerChecks();Check("EngineInitialized", AkUnitySoundEngine.IsInitialized());
            var lamps = UnityEngine.Object.FindObjectsOfType<FormalFluorescentLightAudioEmitter>();
            int count = lamps.Count(l => l.IsAudible);
            report.AppendLine("NearbyLampVoices=" + count);
            Check("LampCap", count <= FormalSfxMixProfile.Instance.maximumNearbyLamps);
            foreach (var router in UnityEngine.Object.FindObjectsOfType<FormalFluorescentLightAudioRouter>()) router.enabled = false;
            foreach (var router in UnityEngine.Object.FindObjectsOfType<FormalAsylumAudioRouter>()) router.enabled = false;
            foreach (var actor in UnityEngine.Object.FindObjectsOfType<FormalPlayerActor>()) actor.enabled = false;
            foreach (var actor in UnityEngine.Object.FindObjectsOfType<MonsterPatrol>()) actor.enabled = false;
            foreach (var controller in UnityEngine.Object.FindObjectsOfType<FormalWwiseMusicController>()) controller.enabled = false;
            // Isolate the volume probe from scene callbacks, coroutines and physics-triggered cues.
            foreach (var behaviour in UnityEngine.Object.FindObjectsOfType<MonoBehaviour>())
            {
                if (behaviour.GetType().Name.StartsWith("Ak", StringComparison.Ordinal)) continue;
                behaviour.StopAllCoroutines();behaviour.enabled = false;
            }
            foreach (var collider in UnityEngine.Object.FindObjectsOfType<Collider>()) collider.enabled = false;
            foreach (var body in UnityEngine.Object.FindObjectsOfType<Rigidbody>()) body.isKinematic = true;
            AkUnitySoundEngine.StopAll();
            source = new GameObject("SFX Validation Source");source.AddComponent<AkGameObj>();
            var listener = UnityEngine.Object.FindObjectOfType<AkAudioListener>();
            source.transform.position = listener != null ? listener.transform.position : Vector3.zero;
            if (listener != null) source.transform.SetParent(listener.transform, true);
            AkUnitySoundEngine.SetRTPCValue("SFXVolume", 100f);
            Check("Capture100Start", AkUnitySoundEngine.StartOutputCapture(Path.Combine(Output, "probe-sfx100.wav")) == AKRESULT.AK_Success);
            Check("GateDrivePost100", FormalSfxEvents.Post("Play_Gate_Drive", source) != 0);phase++;
        }
        else if (phase == 1 && t > 18)
        {
            AkUnitySoundEngine.StopOutputCapture();AkUnitySoundEngine.StopAll();
            AkUnitySoundEngine.SetRTPCValue("SFXVolume", 50f);
            AkUnitySoundEngine.StartOutputCapture(Path.Combine(Output, "probe-sfx50.wav"));FormalSfxEvents.Post("Play_Gate_Drive", source);phase++;
        }
        else if (phase == 2 && t > 21)
        {
            AkUnitySoundEngine.StopOutputCapture();AkUnitySoundEngine.StopAll();
            AkUnitySoundEngine.SetRTPCValue("SFXVolume", 0f);
            AkUnitySoundEngine.StartOutputCapture(Path.Combine(Output, "probe-sfx0.wav"));FormalSfxEvents.Post("Play_Gate_Drive", source);phase++;
        }
        else if (phase == 3 && t > 24)
        {
            AkUnitySoundEngine.StopOutputCapture();AkUnitySoundEngine.StopAll();AkUnitySoundEngine.SetRTPCValue("SFXVolume", 100f);
            foreach (var name in new[] { "Play_Footstep_MonsterC", "Play_Footstep_Monster2", "Play_Door_Move", "Play_Door_Close", "Play_Gate_Drive", "Play_Gate_Stop", "Play_Human_Land" })
            {
                uint id = FormalSfxEvents.Post(name, source);Check(name, id != 0);
            }
            AkUnitySoundEngine.StopAll();
            doorObject = new GameObject("Audio Review Door");doorObject.transform.position = source.transform.position;
            var door = doorObject.AddComponent<FormalDoor>();
            typeof(FormalDoor).GetField("openSpeed", Private).SetValue(door, 45f);
            doorEmitter = doorObject.AddComponent<FormalMechanismAudioEmitter>();doorEmitter.Initialize(door, null);door.Open();
            gateObject = new GameObject("Audio Review Gate");gateObject.transform.position = source.transform.position;
            var gate = gateObject.AddComponent<GateController>();gateEmitter = gateObject.AddComponent<FormalMechanismAudioEmitter>();gateEmitter.Initialize(null, gate);gate.Open();mechanismStarted = t;phase++;
        }
        else if (phase == 4 && t > mechanismStarted + .25)
        {
            report.AppendLine("DoorAngle=" + doorObject.transform.localEulerAngles.y + ";dt=" + Time.deltaTime + ";canSimulate=" + FormalGameplayState.CanSimulate);
            Check("DoorMovementSound", (uint)typeof(FormalMechanismAudioEmitter).GetField("movementId", Private).GetValue(doorEmitter) != 0);
            Check("GateMovementSound", (uint)typeof(FormalMechanismAudioEmitter).GetField("movementId", Private).GetValue(gateEmitter) != 0);phase++;
        }
        else if (phase == 5 && t > mechanismStarted + 3)
        {
            Check("GateEndStopSound", (uint)typeof(FormalMechanismAudioEmitter).GetField("releaseId", Private).GetValue(gateEmitter) != 0);
            doorObject.GetComponent<FormalDoor>().Close();closeStarted = t;phase++;
        }
        else if (phase == 6 && t > closeStarted + 3)
        {
            Check("DoorCloseSound", (uint)typeof(FormalMechanismAudioEmitter).GetField("releaseId", Private).GetValue(doorEmitter) != 0);
            gateObject.GetComponent<GateController>().Close();phase++;
        }
        else if (phase == 7 && t > closeStarted + 3.3)
        {
            gateObject.SetActive(false);
            Check("GateDisableStops", (uint)typeof(FormalMechanismAudioEmitter).GetField("movementId", Private).GetValue(gateEmitter) == 0);
            report.AppendLine("Errors=" + errors);File.WriteAllText(Path.Combine(Output, "deep-runtime-validation.txt"), report.ToString());
            SessionState.SetInt("SfxDeepReview.Errors", errors == 0 ? 0 : 1);
            SessionState.SetBool(Finish, true);EditorApplication.ExitPlaymode();
        }
        if (t > 75)
        {
            report.AppendLine("TIMEOUT");File.WriteAllText(Path.Combine(Output, "deep-runtime-validation.txt"), report.ToString());
            SessionState.SetInt("SfxDeepReview.Errors", 1);SessionState.SetBool(Finish, true);EditorApplication.ExitPlaymode();
        }
    }
}
