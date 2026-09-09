using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>Restrained, location-bound ambience for the existing hospital art.</summary>
public sealed class FormalAsylumAudioRouter : MonoBehaviour
{
    private readonly List<Transform>[] candidates = { new List<Transform>(), new List<Transform>(), new List<Transform>(), new List<Transform>(), new List<Transform>() };
    private readonly AK.Wwise.Event[] events = new AK.Wwise.Event[5];
    private readonly GameObject[] sources = new GameObject[5];
    private readonly uint[] playing = new uint[5];
    private readonly float[] next = new float[5];
    private readonly float[] ends = new float[5];
    private readonly System.Random random = new System.Random();
    private float nextScan;
    private bool paused;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Install()
    {
        if (FindObjectOfType<FormalAsylumAudioRouter>() != null) return;
        var host = new GameObject("[FormalAsylumAudioRouter]");
        DontDestroyOnLoad(host);
        host.AddComponent<FormalAsylumAudioRouter>();
    }

    private void Awake()
    {
        string[] names = { "Draft", "Grille", "Pipe", "Bed", "Window" };
        for (int i = 0; i < names.Length; i++)
        {
            var reference = Resources.Load<WwiseEventReference>("AsylumAudio/Play_Asylum_" + names[i]);
            events[i] = new AK.Wwise.Event { WwiseObjectReference = reference };
            next[i] = Time.time + Range(7f, 16f) + i * 3f;
        }
        SceneManager.sceneLoaded += SceneLoaded;
        SceneManager.sceneUnloaded += SceneUnloaded;
        Scan();
    }

    private float Range(float min, float max) => min + (float)random.NextDouble() * (max - min);
    private void SceneLoaded(Scene scene, LoadSceneMode mode) => Scan();
    private void SceneUnloaded(Scene scene) => Scan();

    private void Scan()
    {
        foreach (var list in candidates) list.Clear();
        for (int s = 0; s < SceneManager.sceneCount; s++)
        {
            Scene scene = SceneManager.GetSceneAt(s);
            if (!scene.isLoaded || !(scene.name.StartsWith("FormalLevel", StringComparison.Ordinal) ||
                scene.name.StartsWith("FormalSharedArt", StringComparison.Ordinal))) continue;
            foreach (GameObject root in scene.GetRootGameObjects())
            foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
            {
                string n = t.name.ToLowerInvariant();
                // Exclude organizational groups; bind to actual rendered prop geometry.
                if (t.GetComponent<Renderer>() == null) continue;
                if (n.Contains("grille") || n.Contains("railing") || n.Contains("fence")) candidates[1].Add(t);
                if (n.Contains("window") || (n.Contains("air") && n.Contains("vent"))) candidates[0].Add(t);
                if (n.Contains("pipe")) candidates[2].Add(t);
                if (n.Contains("bed") && !n.Contains("bedside")) candidates[3].Add(t);
                if (n.Contains("window") && !n.Contains("grille")) candidates[4].Add(t);
            }
        }
        nextScan = Time.unscaledTime + 5f;
    }

    private void Update()
    {
        if (!AkUnitySoundEngine.IsInitialized()) return;
        if (!FormalGameplayState.CanSimulate)
        {
            if (!paused) StopAll();
            paused = true;
            return;
        }
        paused = false;
        if (Time.unscaledTime >= nextScan) Scan();
        Camera camera = Camera.main;
        if (camera == null) return;
        for (int i = 0; i < 5; i++)
        {
            if (playing[i] != AkUnitySoundEngine.AK_INVALID_PLAYING_ID &&
                (sources[i] == null || !sources[i].activeInHierarchy || Time.time >= ends[i])) Stop(i);
            if (Time.time < next[i]) continue;
            Vector2 interval = i == 0 ? FormalSfxMixProfile.Instance.draftInterval : i == 3 ? new Vector2(45f, 80f) : i == 4 ? new Vector2(30f, 55f) : FormalSfxMixProfile.Instance.metalInterval;
            next[i] = Time.time + Range(interval.x, interval.y);
            bool crowded = false;
            for (int other = 1; other < 5; other++)
                if (other != i && Time.time < ends[other] + FormalSfxMixProfile.Instance.metalSeparation) crowded = true;
            if (i != 0 && crowded) continue;
            if (events[i].WwiseObjectReference == null || !events[i].WwiseObjectReference.IsAutoBankLoaded) continue;
            var eligible = new List<Transform>();
            foreach (Transform t in candidates[i])
                if (t != null && t.gameObject.activeInHierarchy &&
                    (t.GetComponent<Renderer>().bounds.center - camera.transform.position).sqrMagnitude < FormalSfxMixProfile.Instance.ambienceRadius * FormalSfxMixProfile.Instance.ambienceRadius)
                    eligible.Add(t);
            if (eligible.Count == 0) continue;
            Transform anchor = eligible[random.Next(eligible.Count)];
            Stop(i);
            var source = new GameObject("Asylum " + i + " audio");
            source.transform.SetParent(anchor, false);
            source.transform.position = anchor.GetComponent<Renderer>().bounds.center;
            source.AddComponent<AkGameObj>();
            sources[i] = source;
            playing[i] = events[i].Post(source);
            ends[i] = Time.time + (i == 0 ? 12.2f : 3.5f);
        }
    }

    private void Stop(int i)
    {
        if (playing[i] != AkUnitySoundEngine.AK_INVALID_PLAYING_ID && AkUnitySoundEngine.IsInitialized())
            AkUnitySoundEngine.ExecuteActionOnPlayingID(AkActionOnEventType.AkActionOnEventType_Stop, playing[i], 150,
                AkCurveInterpolation.AkCurveInterpolation_Linear);
        playing[i] = AkUnitySoundEngine.AK_INVALID_PLAYING_ID;
        if (sources[i] != null) Destroy(sources[i]);
        sources[i] = null;
    }
    private void StopAll() { for (int i = 0; i < 5; i++) Stop(i); }
    private void OnDisable() => StopAll();
    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= SceneLoaded;
        SceneManager.sceneUnloaded -= SceneUnloaded;
        StopAll();
    }
}
