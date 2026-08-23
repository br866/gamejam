# Proposal: add-formal-persistent-main-camera

## Why

Playing the Formal route (FormalPersistent + FormalLevel*) shows a Unity-default blue background: no scene contains a camera, so `FormalPlayerControl.Start()` spawns `FormalMainCamera` with factory defaults (`ClearFlags = Skybox`, default blue background), and every Formal scene has `RenderSettings.SkyboxMaterial = None`, so the skybox clear falls back to that blue color. The game's intended look (enforced elsewhere by `GameManager`) is a solid black void outside rooms.

## What Changes

- Add a pre-configured follow camera to `Assets/MoMing/FormalLevels/FormalPersistent.unity` by copying the `MainCamera` child node out of the `PlayerSystem` prefab instance in `Assets/Scenes/Test/superbreadman 1.unity` (components: Camera with `ClearFlags = SolidColor` black / FOV 50, `CameraFollow`, AudioListener, URP AdditionalCameraData).
- Copy the `Global Volume` object from `superbreadman 1.unity` into FormalPersistent so URP post-processing (Bloom, Vignette, FilmGrain, ColorAdjustments, Tonemapping, WhiteBalance, ChromaticAberration, ShadowsMidtonesHighlights) applies on the formal route.
- Do NOT copy the rest of the `PlayerSystem` prefab (Human/Dog/Canvas) — the formal flow spawns its own players via `FormalPlayerSpawner`.
- No script changes: `FormalPlayerControl.Start()` already reuses an existing `CameraFollow` via `FindObjectOfType<CameraFollow>()` and assigns the active actor as target; with a scene camera present, the blue factory camera is never created.

## Capabilities

### New Capabilities
- `formal-persistent-camera-bootstrap`: Defines that the formal runtime must always render through a pre-placed, solid-black-background follow camera in FormalPersistent (never a dynamically spawned factory-default camera), and that post-processing must come from the copied Global Volume.

### Modified Capabilities
- None.

## Impact

- Scene file: `Assets/MoMing/FormalLevels/FormalPersistent.unity` (adds two root objects: MainCamera clone + Global Volume).
- Reference assets reused without modification: `Assets/MoMing/Scripts/Core/CameraFollow.cs`, `Assets/Scenes/Game/superbreadman 2/Global Volume Profile.asset`.
- Runtime behavior affected: `Assets/MoMing/Scripts/LevelRuntime/FormalPlayerControl.cs` camera-bootstrap branch becomes dormant (reuse path taken instead).
- Out of scope / untouched: all superbreadman source scenes, `skybox.mat`, RenderSettings of any scene, GameManager, player spawner.
- Note: this change is independent of `align-superbreadman-level-scope` (whose pass forbids adding scene objects); it must not be mixed into that change's commits.
