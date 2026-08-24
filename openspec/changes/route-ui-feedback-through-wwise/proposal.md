# Proposal: route-ui-feedback-through-wwise

## Why

The authored Wwise events `Play_UI_Hover` and `Play_UI_Click` and their generated Auto-Defined SoundBanks are ready, but the Unity UI currently has no shared Wwise feedback path. UI is spread across `Start.unity`, `FormalPersistent.unity`, and legacy UI prefabs, so wiring individual button callbacks would be incomplete and fragile.

## What Changes

- Add a persistent Wwise UI feedback router with serialized `AK.Wwise.Event` references.
- Discover Unity UI `Button` components in loaded scenes, including initially inactive menus, and attach lightweight pointer/submit handlers without changing existing callbacks.
- Post hover once on pointer entry or non-pointer selection, and post click on valid left-press or submit.
- Skip disabled or non-interactable buttons and prevent duplicate handler installation.
- Add an editor bootstrap that creates the shared settings asset from the authored Wwise Event GUIDs when missing.
- Leave existing scene, prefab, UI manager, and legacy `SfxManager` behavior unchanged.

## Impact

- Runtime code under `UnityProject/Assets/MoMing/Scripts/UI/`.
- Editor bootstrap under `UnityProject/Assets/MoMing/Scripts/Editor/`.
- One generated project settings asset under `UnityProject/Assets/Resources/MoMing/` plus Wwise Event reference assets created by the integration when needed.
- No manual scene or prefab reserialization.
