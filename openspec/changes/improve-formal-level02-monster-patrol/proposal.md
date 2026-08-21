# Proposal: improve-formal-level02-monster-patrol

## Why

The FormalLevel02 monster exists in `L02_Content` but is inert: `MonsterPatrol.waypoints` holds only null references, so `MonsterPatrol.Awake` disables the component and the second level has no threat at all. The scene already contains `L02_MonsterWaypointA` and `L02_MonsterWaypointB` inside the monster room, but they were never connected to the monster.

## What Changes

- Wire the existing scene waypoints `L02_MonsterWaypointA` and `L02_MonsterWaypointB` into the `MonsterPatrol.waypoints` of the `L02_Content` monster through prefab-instance overrides in `FormalLevel02.unity`.
- Keep the existing patrol, detection, chase, safe-zone, and catch behavior unchanged; no script rewrites in this change.
- Add regression coverage asserting the Level02 monster has valid, non-null waypoints that live in the Level02 scene.
- Verify in Play Mode that the monster patrols, chases, stops at the safe zone, and resets with the level.
- Navigation (A* grid graph) wiring remains out of scope and is recorded as follow-up if direct movement is insufficient.

## Impact

- Target scene: `UnityProject/Assets/MoMing/FormalLevels/FormalLevel02.unity` (prefab-instance overrides only).
- Test file: `UnityProject/Assets/Editor/FormalTraversalValidationTests.cs`.
- No changes to `MonsterPatrol`, `LevelMonsterNavigation`, player controls, collision, or audio.
- `UnityProject/.idea/` remains untracked and excluded.
