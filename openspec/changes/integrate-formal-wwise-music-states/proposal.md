# Proposal: integrate-formal-wwise-music-states

## Why

The formal route now plays the looping Wwise `Play_MusicMode` event from `FormalPersistent`, but Unity does not drive its `MusicMode` or `AnxietyLevel` State Groups. The authored music therefore cannot react to monster detection or formal anxiety, and restart/additive scene transitions can leave the music in a stale state.

## What Changes

- Add a formal-route Wwise music controller that owns the initial State application and posts the existing `AkAmbient` event once.
- Map `MusicMode` to monster chase state: `Combat` while any active loaded `MonsterPatrol` is chasing, otherwise `Explore`.
- Map `AnxietyLevel` to `FormalAnxietyState.Normalized` using configurable Low/Mid/High thresholds aligned with the formal HUD defaults.
- Refresh the monster cache as additive formal scenes load and unload, and reapply State values after formal state instances change or a level restarts.
- Keep the existing authored Wwise event, playlists, SoundBanks, and formal scene objects unchanged in code; scene hookup remains an Inspector operation.

## Impact

- New runtime script under `UnityProject/Assets/MoMing/Scripts/Audio/` with its Unity `.meta` file.
- Runtime dependencies: `AkAmbient`, `FormalAnxietyState`, `MonsterPatrol`, and additive `SceneManager` events.
- `Audio_Music` must use `AkAmbient.Trigger On = Nothing` so the controller can set initial States before posting the event.
- No modification to current user-owned `FormalPersistent.unity` or Wwise authoring assets in this implementation pass.

