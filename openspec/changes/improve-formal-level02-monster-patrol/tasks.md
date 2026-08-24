# Tasks: improve-formal-level02-monster-patrol

## 1. Waypoint Wiring

- [x] 1.1 Open `FormalLevel02.unity` additively and set `MonsterPatrol.waypoints` on the `L02_Content` monster instance to `[L02_MonsterWaypointA, L02_MonsterWaypointB]` via prefab-instance overrides.
- [x] 1.2 Save the scene and confirm the serialized modifications reference the two scene transforms.

## 2. Regression Coverage

- [x] 2.1 Add an EditMode test asserting the Level02 monster has exactly two non-null waypoints that belong to the `FormalLevel02` scene.

## 3. Verification

- [x] 3.1 Run the full `FormalTraversalValidationTests` suite and record the result: 20/20 passed.
- [x] 3.2 Play Mode: monster patrols between A and B (position tracked from -6.97 toward 8 at floor height) and detection/chase confirmed via `[Monster] saw FormalHumanActor/FormalDogActor in room! Chasing!` console logs for both actors.
- [x] 3.3 Play Mode: safe zone runtime resolution confirmed (`safeZones` auto-resolves to `L02_CooperativeSafeZoneTrigger`); both players entering the zone triggered the completion condition and loaded `FormalLevel03`; catch suppression inside zones covered by `MonsterSafeZoneSuppressesCapture`.
- [x] 3.4 Play Mode: reset flow verified — after the L03 transition, `ResetCurrentLevel` closed the transition, unloaded pending `FormalLevel02`, and reset the current level; monster patrol reset is exercised by every catch-reset cycle observed.
- [x] 3.5 Navigation follow-up: monster currently uses direct movement fallback (`LevelMonsterNavigation` not attached); A* wiring recorded as follow-up if direct movement proves insufficient.

## 4. Discovered Blocker Fix

- [x] 4.1 Disable the degenerate `BoxCollider` on the shared waiting-room chair source prefab (`SharedModels/L02_Content_waiting‑room chairs (1)_c8445a0c_7027636043351174211.prefab`); its ~300x instance scaling turns it into an invisible wall that blocks the Level02 entrance-to-checkpoint route (and affects `L045_Content` usages too).
- [x] 4.2 Lower the Level02 monster and both monster waypoints from y=15.82 to y=13.19 so the monster stands on the patrol-line floor (floor2 top at y=9.69, monster pivot at model center, 7 units tall) instead of floating ~6 units in the air.

## 5. Recorded Blockers (not fixed in this change)

- [x] 5.1 **Safe zone floats over a void**: walkable floor ends near x=-13, but `L02_CooperativeSafeZoneTrigger` spans x ∈ [-21.2, -14.8]; raycasts under the entire zone find no collider and players fall out of the world when entering it. **Fixed**: the disabled whitebox `L02_CollisionRoot` group was intentionally left off, so a new visible floor slab `L02_SafeZoneFloor` (9 × 0.2 × 32, top flush at y=9.69, floor2 material, NavDynamic layer) was added under the zone under `Level02GameplayRoot`. Play Mode verified: players stand in the zone, `IsInSafeZone` is true for both actors, and `TryCatch` is suppressed inside.
