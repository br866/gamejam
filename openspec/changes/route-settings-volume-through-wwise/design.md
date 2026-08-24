# Design: route-settings-volume-through-wwise

## Context

- `ProjectSettings/AudioManager.asset` already has Unity audio disabled.
- Cutscenes use `VideoAudioOutputMode.Direct`, which bypasses Unity audio processing and sends embedded audio to platform output.
- Wwise already defines global Game Parameters `MusicVolume` and `SFXVolume`, both with an initial value of 100 and RTPC curves on their corresponding Audio Busses.
- The settings sliders and saved PlayerPrefs values are normalized from 0 to 1.

## Decisions

### D1. Preserve video through direct output

Keep Unity audio disabled and force `CutscenePlayer` videos to use `VideoAudioOutputMode.Direct`. This preserves embedded cutscene audio without reopening the legacy `AudioSource` path.

### D2. Use strongly typed global RTPC references

Extend the existing shared Wwise UI settings asset with `AK.Wwise.RTPC` references resolved from the authored Game Parameter GUIDs. The slider maps its normalized value to the Wwise 0–100 range and calls `SetGlobalValue`.

### D3. Apply saved values after Wwise initialization

The settings panel is initially inactive, so volume cannot depend on its `Awake` or `OnEnable`. Subscribe a static settings callback to Wwise initialization/reinitialization and also apply immediately when the sound engine is already initialized.

### D4. Stop driving legacy Unity volume

`SettingsManager` no longer writes `AudioListener.volume` or calls `MusicManager`. Legacy call sites remain harmless while Unity audio is disabled and can be migrated to Wwise Events independently.

## Risks / Trade-offs

- Direct video audio intentionally bypasses the Wwise Music/SFX RTPCs. A dedicated video volume control can be added later if required.
- The RTPC reference asset must be refreshed by Unity after the Wwise Game Parameters exist; the editor bootstrap retries and exposes a manual refresh menu command.
