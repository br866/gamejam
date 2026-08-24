# Proposal: route-settings-volume-through-wwise

## Why

The settings menu still applies music and SFX volume through the legacy Unity audio managers, while production audio now plays through Wwise. Unity audio is already disabled project-wide, except that cutscene videos intentionally use direct embedded-audio output.

## What Changes

- Keep Unity's built-in audio disabled and preserve direct `VideoPlayer` audio for cutscenes.
- Route the settings menu's Music and SFX sliders to the authored global Wwise Game Parameters `MusicVolume` and `SFXVolume`.
- Restore saved slider values after Wwise initializes, before the settings panel is opened.
- Remove the settings menu's dependency on the legacy Unity `MusicManager` and `AudioListener` volume path.

## Impact

- Runtime settings and cutscene scripts under `UnityProject/Assets/MoMing/Scripts/`.
- Existing shared Wwise UI settings asset/bootstrap gains strongly typed RTPC references.
- No scene or prefab reserialization and no change to authored Wwise bus curves.
