# Proposal: restart-gameplay-music-and-add-parchment-ui-audio

## Why

Restarting a formal level currently recovers gameplay state without restarting the persistent Wwise music instance, so the music continues from its previous timeline position. Formal parchment-style panels also need consistent authored open and close sounds.

## What Changes

- Extend `FormalWwiseMusicController` to own the authored `Stop_Gameplay_Music` Event and expose one restart operation that stops, waits for the authored 0.5-second fade, reapplies States, and posts `Play_Gameplay_Music` again.
- Invoke that operation from the centralized `FormalGameFlowController.ResetCurrentLevel` path so pause, death, automatic, and debug restarts share the same behavior.
- Add a small shared `FormalParchmentAudio` component on the existing registered `Audio_Music` GameObject.
- Post `Play_UI_Parchment_Open` and `Play_UI_Parchment_Close` from formal Pause, Settings, and Tutorial/notice UI transitions.
- Leave the DeathScreen unchanged.

## Impact

- Focused runtime changes in the existing music, flow, pause, settings, and tutorial scripts.
- One new reusable UI-audio script.
- A localized `Audio_Music` component update in `FormalPersistent.unity`.
- Unity Wwise reference assets for the new authored Events.
- No Wwise Work Unit, source audio, container, or generated SoundBank changes.
