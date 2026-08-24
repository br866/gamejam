# Proposal: route-dog-footsteps-through-wwise

## Why

The formal Dog actor shares `FormalPlayerActor` with the Human, but the current Wwise footstep path explicitly handles only the Human role. The authored `Play_Footstep_Dog` Event and its generated AutoBank are ready, so the Dog needs equivalent positional playback with cadence matched to its own walk animation.

## What Changes

- Add a serialized Dog Wwise Event reference and Dog-specific walk cadence to `FormalPlayerActor`.
- Select the Human or Dog Event from the actor role while preserving the existing grounded distance trigger.
- Add `AkGameObj` to `FormalDogActor` and assign `Play_Footstep_Dog` in the formal player prefab.
- Keep random sample selection inside the authored Wwise Random Container.
- Match the Dog's 30-frame looping walk clip at 3 m/s with a 1.5 m half-cycle distance.

## Impact

- Runtime code changes in `UnityProject/Assets/MoMing/Scripts/LevelRuntime/FormalPlayerActor.cs`.
- Prefab hookup in `UnityProject/Assets/MoMing/FormalLevels/Prefabs/FormalPlayerActors.prefab`.
- One Unity Wwise Event reference asset for the existing `Play_Footstep_Dog` Event.
- No Wwise container or generated SoundBank changes.
