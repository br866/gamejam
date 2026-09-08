using System.Collections.Generic;
using UnityEngine;

/// <summary>Shared references keep the new auto-banks resident while formal SFX need them.</summary>
public static class FormalSfxEvents
{
    private static readonly Dictionary<string, AK.Wwise.Event> Events = new Dictionary<string, AK.Wwise.Event>();
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void Reset() => Events.Clear();

    public static void Preload()
    {
        foreach (string name in new[] { "Play_Door_Move", "Play_Door_Close", "Play_Gate_Drive", "Play_Gate_Stop", "Play_Human_Land", "Play_Human_Tile", "Play_DoorMetal_Move", "Play_DoorMetal_Close", "Play_Heavy_Presence", "Play_Uniform_Presence", "Play_Gaunt_Presence", "Play_Heavy_Attack", "Play_Uniform_Attack", "Play_Gaunt_Attack", "Play_Footstep_Brutedoc", "Play_Footstep_Monster2", "Play_Footstep_MonsterC" }) Get(name);
    }

    private static AK.Wwise.Event Get(string name)
    {
        if (Events.TryGetValue(name, out var ev)) return ev;
        var reference = Resources.Load<WwiseEventReference>("AsylumAudio/" + name);
        ev = new AK.Wwise.Event { WwiseObjectReference = reference };
        Events.Add(name, ev);
        if (reference == null) Debug.LogWarning("[FormalSfx] Missing event reference: " + name);
        return ev;
    }

    public static uint Post(string name, GameObject source)
    {
        if (!AkUnitySoundEngine.IsInitialized() || source == null) return 0;
        var ev = Get(name);
        return ev.IsValid() ? ev.Post(source) : 0;
    }

    public static void Stop(ref uint playingId, int fade = 100)
    {
        if (playingId != 0 && AkUnitySoundEngine.IsInitialized())
            AkUnitySoundEngine.ExecuteActionOnPlayingID(AkActionOnEventType.AkActionOnEventType_Stop,
                playingId, fade, AkCurveInterpolation.AkCurveInterpolation_Linear);
        playingId = 0;
    }
}
