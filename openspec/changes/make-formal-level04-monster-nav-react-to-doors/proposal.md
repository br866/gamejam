# Proposal: make-formal-level04-monster-nav-react-to-doors

## Why

FormalLevel04 has two monsters whose A* grid graph is baked once at `Start`. The three doors inside the monster navigation area ("door5 (6)/(7)/(8)" from `L01_Door_Mechanism`, roots already on the NavDynamic layer) are therefore permanent walls in the nav graph regardless of their open/closed state. Monsters cannot chase players through opened doors, and door state changes never affect path connectivity. Additionally, the Level4.5 scripted pursuit (`FormalGameFlowController.StartLevel045PursuitAfterDelay`) drives monsters with straight-line movement that ignores walls, doors, and navigation entirely, and `LevelMonsterNavigation` permanently stalls after a single failed path request.

## What Changes

- Add an additive `StateChanged` event to `FormalDoor`, fired by `Open()`, `Close()`, and `SetStateImmediate()` (instant semantics on Close; no collider-timing changes).
- Extend `LevelMonsterNavigation` behind a default-off `dynamicDoorNavigation` flag:
  - Establish the A* startup contract: create the `AstarPath` singleton before any navigation request may be issued, defer `StartPath` until its GridGraph has been created and scanned, then issue the pending initial request.
  - Fix the stall bug so failed paths are re-requested after the repath interval.
  - Expose `LastPathFailed`.
  - Subscribe to in-area doors' `StateChanged` and flip walkability of the door's grid region on the same frame (`FlushGraphUpdates`) with immediate repath.
  - Push a monster out of a newly blocked region with a ~0.2 s slide to the nearest walkable node.
  - Add `RescanGraph()` for one-shot rescans.
- Rework `MonsterPatrol.ForcedChase` to move through `LevelMonsterNavigation` (falls back to legacy straight-line only when no navigation component), fall back to waypoints while a path is unavailable, and trigger `RescanGraph()` once from `BeginForcedChase`.
- `Chase` drops its target back to patrol (waypoints) when pathfinding fails, so monsters never freeze.
- Build a fully isolated editor-only test scene `Assets/MoMing/Scenes/Test/Test_DoorNav.unity` plus `Assets/MoMing/Scripts/Debug/DoorNavTestGm.cs` (`#if UNITY_EDITOR`) providing GM door toggles, right-click group pathing orders, and a forced-chase hotkey. Test assets never enter production scenes or builds.
- Scene enablement of the flag and enlarged nav bounds for the two FormalLevel04 monster instances is performed manually by the user in the Unity Editor (reminded at completion); scripts ship with the flag default-off.

## Impact

- Affected code: `UnityProject/Assets/MoMing/Scripts/LevelRuntime/FormalDoor.cs`, `UnityProject/Assets/MoMing/Scripts/Enemy/LevelMonsterNavigation.cs`, `UnityProject/Assets/MoMing/Scripts/Enemy/MonsterPatrol.cs`.
- New isolated test assets under `UnityProject/Assets/MoMing/Scripts/Debug/` and `UnityProject/Assets/MoMing/Scenes/Test/`.
- Behavior scope: only monsters that opt in via the flag (the two FormalLevel04 instances) gain dynamic connectivity; all other levels keep current behavior because defaults are unchanged.
- No changes to door mechanisms, triggers, player controls, collision layers, audio, or other scenes/prefabs.
- Out of scope: making doors auto-open for monsters, L045/L05 content edits, changing physical collider timing of doors.

## Verification

- Play Mode in `Test_DoorNav.unity`: open door → monster paths through doorway; close door → path blocked immediately even during swing animation; monster inside doorway slides out (~0.2 s); forced-chase horde navigates around walls and only passes open doors.
- Regression: L01/L03 scenes unaffected; console free of errors; existing validation tests still pass.
