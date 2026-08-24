# Proposal: route-human-footsteps-through-wwise

## Why

The formal route uses `FormalPlayerActor`, which currently has no footstep playback. The authored Wwise event `Play_Footstep_Human` and its generated AutoBank are ready, so the formal Human actor needs distance-based positional playback without changing formal movement or the not-yet-migrated Dog path.

## What Changes

- Add a serialized Wwise Event reference and animation-matched distance cadence to `FormalPlayerActor`.
- Post that Event only on the formal Human actor while grounded.
- Add `AkGameObj` to `FormalHumanActor` and reference `Play_Footstep_Human` from the formal player prefab.
- Leave the formal Dog actor and legacy `PlayerController` footstep system unchanged.
- Reset the cadence origin when the formal actor is repositioned so level transitions do not produce a false step.
- Match the 30 FPS Walk (31-frame span) and Run (19-frame span) clips at current movement speeds with separate 2.07 m and 2.22 m half-cycle distances.

## Impact

- Runtime code changes in `UnityProject/Assets/MoMing/Scripts/LevelRuntime/FormalPlayerActor.cs`.
- Prefab hookup in `UnityProject/Assets/MoMing/FormalLevels/Prefabs/FormalPlayerActors.prefab`.
- No Wwise authoring changes and no Dog or Enemy migration in this change.
