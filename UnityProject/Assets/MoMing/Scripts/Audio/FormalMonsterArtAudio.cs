using System;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>Sound identity follows the active visual, including scene model overrides.</summary>
[DefaultExecutionOrder(150), DisallowMultipleComponent, RequireComponent(typeof(AkGameObj))]
public sealed class FormalMonsterArtAudio : MonoBehaviour
{
    private MonsterPatrol patrol;
    private Animator visual;
    private string family;
    private string previousClip;
    private float previousPhase;
    private Vector3 previousPosition;
    private float nextPresence;
    private float nextScan;
    private bool attacking;
    private uint footId, presenceId, attackId;
    private readonly uint[] stepTails = new uint[4];
    private int tailIndex;
    private FootstepDistanceTracker fallback;
    public string Family => family;
    public bool OwnsFootsteps => isActiveAndEnabled && !string.IsNullOrEmpty(family);
    public int ContactCount { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Register()
    {
        SceneManager.sceneLoaded -= Loaded;
        SceneManager.sceneLoaded += Loaded;
    }
    private static void Loaded(Scene scene, LoadSceneMode mode)
    {
        if (!scene.name.StartsWith("FormalLevel", StringComparison.Ordinal)) return;
        foreach (var root in scene.GetRootGameObjects())
        foreach (var monster in root.GetComponentsInChildren<MonsterPatrol>(true))
            if (monster.GetComponent<FormalMonsterArtAudio>() == null)
                monster.gameObject.AddComponent<FormalMonsterArtAudio>();
    }
    public static string Identify(Animator animator)
    {
        if (animator == null || animator.runtimeAnimatorController == null) return null;
        string n = animator.runtimeAnimatorController.name.ToLowerInvariant();
        if (n.Contains("monster3")) return "Heavy";
        if (n.Contains("monster2")) return "Uniform";
        if (n.Contains("monster1")) return "Gaunt";
        return null;
    }
    private void OnEnable()
    {
        patrol = GetComponent<MonsterPatrol>();
        previousPosition = transform.position;
        fallback.Reset(previousPosition);
        previousClip = null;
        ResolveVisual();
        nextPresence = Time.time + UnityEngine.Random.Range(18f, 30f);
    }
    private void ResolveVisual()
    {
        Animator found = null;
        foreach (var candidate in GetComponentsInChildren<Animator>())
            if (candidate.isActiveAndEnabled && Identify(candidate) != null) { found = candidate; break; }
        string resolved = Identify(found);
        if (visual != found || family != resolved)
        {
            StopSounds(); previousClip = null; fallback.Reset(transform.position);
            visual = found; family = resolved;
        }
        nextScan = Time.unscaledTime + .5f;
    }
    public static bool ContactCrossed(float before, float now, float phase)
        => now >= before && now - before < .75f && Mathf.FloorToInt(now - phase) > Mathf.FloorToInt(before - phase);

    // Contact windows measured from the supplied animation foot trajectories.
    public static bool ContactPhases(string clip, out float left, out float right)
    {
        left = right = 0;
        switch (clip.ToLowerInvariant().Replace(" ", ""))
        {
            case "monster3walk": case "monster2walk": left = .075f; right = .6583f; return true;
            case "monster3run": left = .15f; right = .675f; return true;
            case "monster2run1": left = .2333f; right = .7417f; return true;
            case "monster2run2": left = .125f; right = .65f; return true;
            case "monster1walk1": left = .7583f; right = .25f; return true;
            case "monster1walk2": left = .3583f; right = .8667f; return true;
            case "monster1run1": left = .1833f; right = .6083f; return true;
            case "monster1run2": left = .1333f; right = .6417f; return true;
            default: return false;
        }
    }
    private void LateUpdate()
    {
        if (Time.unscaledTime >= nextScan || visual == null || !visual.isActiveAndEnabled) ResolveVisual();
        Vector3 delta = transform.position - previousPosition; previousPosition = transform.position;
        if (!OwnsFootsteps || patrol == null || !patrol.isActiveAndEnabled || !FormalGameplayState.CanSimulate ||
            delta.magnitude > Mathf.Max(2f, 12f * Time.deltaTime))
        {
            StopSounds(); previousClip = null; attacking = patrol != null && patrol.IsAttacking; fallback.Reset(transform.position);
            nextPresence = Time.time + 18f; return;
        }
        if (patrol.IsAttacking && !attacking)
        {
            FormalSfxEvents.Stop(ref attackId);
            attackId = FormalSfxEvents.Post("Play_" + family + "_Attack", gameObject);
        }
        attacking = patrol.IsAttacking;
        var camera = Camera.main;
        if (!attacking && Time.time >= nextPresence)
        {
            nextPresence = Time.time + UnityEngine.Random.Range(18f, 30f);
            if (camera != null && (camera.transform.position - transform.position).sqrMagnitude < 144f)
                presenceId = FormalSfxEvents.Post("Play_" + family + "_Presence", gameObject);
        }
        delta.y = 0;
        bool moving = !attacking && delta.magnitude > Mathf.Max(.000001f, .2f * Time.deltaTime);
        if (!moving) { previousClip = null; fallback.Reset(transform.position); return; }
        var clips = visual.GetCurrentAnimatorClipInfo(0);
        if (clips.Length == 0) { previousClip = null; return; }
        string clip = clips[0].clip.name;
        float phase = visual.GetCurrentAnimatorStateInfo(0).normalizedTime;
        if (ContactPhases(clip, out float left, out float right))
        {
            if (previousClip == clip && (ContactCrossed(previousPhase, phase, left) || ContactCrossed(previousPhase, phase, right))) Step();
            fallback.Reset(transform.position);
        }
        else if (clip.IndexOf("walk", StringComparison.OrdinalIgnoreCase) >= 0 || clip.IndexOf("run", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            if (fallback.Tick(transform.position, patrol.IsChasing ? 2.2f : 1.8f, true, Time.deltaTime)) Step();
        }
        previousClip = clip; previousPhase = phase;
    }
    private void Step()
    {
        // Single owner replaces the legacy distance emitter; separate steps can retain short natural tails.
        string name = family == "Heavy" ? "Play_Footstep_Brutedoc" : family == "Uniform" ? "Play_Footstep_Monster2" : "Play_Footstep_MonsterC";
        FormalSfxEvents.Stop(ref stepTails[tailIndex], 15);
        footId = FormalSfxEvents.Post(name, gameObject);
        stepTails[tailIndex] = footId; tailIndex = (tailIndex + 1) % stepTails.Length;
        if (footId != 0) ContactCount++;
    }
    private void StopSounds()
    {
        for (int i = 0; i < stepTails.Length; i++) FormalSfxEvents.Stop(ref stepTails[i]);
        footId = 0; FormalSfxEvents.Stop(ref presenceId); FormalSfxEvents.Stop(ref attackId);
    }
    private void OnDisable() { StopSounds(); previousClip = null; }
}
