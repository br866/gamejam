# Design: add-formal-persistent-main-camera

## Context

- Root cause (verified by scene/script inspection): every Formal scene has `RenderSettings.m_SkyboxMaterial = {fileID: 0}` and contains no camera; `FormalPlayerControl.Start()` (`Assets/MoMing/Scripts/LevelRuntime/FormalPlayerControl.cs:18-26`) finds no `CameraFollow`, so it spawns a camera with factory defaults (`ClearFlags = Skybox` + default blue background). Skybox clear with no skybox material falls back to the background color — hence the blue.
- The proven, already-black configuration exists in the superbreadman art scenes: `PlayerSystem` prefab instance → `MainCamera` child (Camera SolidColor black / FOV 50 / near 0.3 far 1000, `CameraFollow`, AudioListener, URP AdditionalCameraData with VolumeLayerMask = Default), plus scene-root `Global Volume` using `Assets/Scenes/Game/superbreadman 2/Global Volume Profile.asset`.
- Constraint: no script changes in this change; reuse existing runtime hooks only.

## Goals / Non-Goals

Goals:
- Formal route renders through one pre-placed solid-black follow camera.
- Post-processing matches the superbreadman look on the formal route.

Non-Goals:
- Modifying `FormalPlayerControl.cs`, `GameManager.cs`, or any prefab asset.
- Changing superbreadman source scenes, `skybox.mat`, or any scene's RenderSettings.
- Fixing the orphaned prefabs found during investigation (`Assets/Prefabs/PlayerSystem.prefab`, `MoMing/Prefabs/UI_System.prefab`) or the duplicate-MainCamera ambiguity in superbreadman scenes.

## Decisions

1. **Copy the `MainCamera` child node only — not the whole `PlayerSystem` prefab.**
   The prefab also contains Human/Dog actors and a UI Canvas that would collide with `FormalPlayerSpawner`. Alternative considered: dragging the full prefab in — rejected because of spawn duplication and double AudioListener risk.
2. **Place both objects as root objects in FormalPersistent** (persistent scene, always loaded). Alternative: placing per level scene — rejected because FormalPersistent is loaded in every formal play configuration and keeps one copy for all levels.
3. **Reference the existing volume profile in place** (`Assets/Scenes/Game/superbreadman 2/Global Volume Profile.asset`) rather than cloning the asset. One shared profile keeps the look consistent and editable in one place. The profile's folder location is historical but functional.
4. **No code change for camera takeover.** Existing behavior at `FormalPlayerControl.cs:18-28` already prefers `FindObjectOfType<CameraFollow>()` and calls `SetTarget(activeActor.transform)`; the spawned-camera branch simply becomes dormant. Alternative: hardening GameManager to force all cameras black — deferred as a separate robustness change if ever needed.
5. **Reset copied transform to a sane default** (position above spawn area or origin, rotation ~60° pitch as in source); exact placement is cosmetic since CameraFollow drives the transform every frame once targeted.

## Risks / Trade-offs

- [Two enabled cameras across additively loaded scenes] → Mitigation: verification task checks exactly-one-enabled-MainCamera after startup; Formal scenes contain no other cameras today (the disabled prototype in FormalLevel03 stays disabled).
- [`FindObjectOfType<CameraFollow>()` must run after the persistent camera exists] → Mitigation: camera is placed in FormalPersistent, which is loaded before/at the same time as level scenes where FormalPlayerControl lives; verify via play test.
- [Shared volume profile edits affect superbreadman scenes too] → Accepted trade-off; it is the same intent (one look everywhere). If divergence is wanted later, clone the profile then.
- [Manual editor copy can drift from source config] → Mitigation: tasks.md lists the exact component values to verify after pasting.

## Migration Plan

1. Apply edits inside Unity Editor on FormalPersistent (duplicate MainCamera node + Global Volume from `superbreadman 1.unity`, paste into FormalPersistent, save).
2. Play-test FormalPersistent + Level01/02 multi-scene setup; confirm black background, single main camera, effects visible, console free of new errors.
3. Rollback: remove the two added objects from FormalPersistent (scene-file revert).

## Open Questions

None blocking; exact initial transform values are cosmetic and adjustable in-editor.
