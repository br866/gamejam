# Tasks: make-formal-level04-monster-nav-react-to-doors

## 1. Production code

- [x] 1.1 `FormalDoor.cs`: add `public event System.Action<FormalDoor> StateChanged;` and a private raiser; invoke at the end of `Open()`, `Close()`, and `SetStateImmediate()` (after state assignment).
- [x] 1.2 `LevelMonsterNavigation.cs`: retry errored paths â€?in `Update`, when `currentPath != null && currentPath.error && Time.time >= nextRepathTime`, request a new path instead of early-returning forever.
- [x] 1.3 `LevelMonsterNavigation.cs`: set `public bool LastPathFailed` from `OnPathComplete` (`path == null || path.error`).
- [x] 1.4 `LevelMonsterNavigation.cs`: add `[SerializeField] bool dynamicDoorNavigation` (default false) + public read-only property.
- [x] 1.5 `LevelMonsterNavigation.cs`: when enabled, collect scene `FormalDoor`s whose blocking-collider bounds intersect the nav area rect at `Start`; subscribe/unsubscribe `StateChanged` (OnDestroy-safe); guard duplicate subscriptions.
- [x] 1.6 `LevelMonsterNavigation.cs`: door event handler â€?build contracted GUO over the collider bounds with `modifyWalkability`, `setWalkability = door.IsOpen`; queue it, call `AstarPath.FlushGraphUpdates()`, then force `RequestPath()`.
- [x] 1.7 `LevelMonsterNavigation.cs`: push-out coroutine (~0.2 s slide to nearest walkable node, pause path following during the slide, repath afterwards) triggered by close events when the monster's nearest node is unwalkable.
- [x] 1.8 `LevelMonsterNavigation.cs`: add `public void RescanGraph()` that rescans the shared grid graph when it exists.
- [x] 1.9 `MonsterPatrol.cs`: in `Chase`, if `navigation.LastPathFailed` â†?clear target and return to `State.Patrol`.
- [x] 1.10 `MonsterPatrol.cs`: rework `ForcedChase` to drive via `navigation` (`SetMoveSpeed(chaseSpeed)`, `SetDestination(target)`, stop-distance clearing); on `LastPathFailed` steer to current waypoint for ~1 s before retrying the player; keep straight-line body only as no-navigation fallback.
- [x] 1.11 `MonsterPatrol.cs`: `BeginForcedChase` calls `navigation.RescanGraph()` once per pursuit activation.

## 2. Compile & static checks

- [x] 2.1 Unity compiles with zero errors/warnings introduced by the three files (read console after refresh).
- [x] 2.2 Confirm no other scripts reference removed/changed members (grep usages of `ForcedChase`, `LastPathFailed`, `StateChanged`).

## 3. Test harness

- [x] 3.1 Create `Assets/MoMing/Scripts/Debug/DoorNavTestGm.cs` fully wrapped in `#if UNITY_EDITOR`: auto-find doors (number-key toggles with OnGUI legend showing name+state), right-click group order via `SetDestination`, F-key `BeginForcedChase` to a marker transform, camera framing helper.
- [x] 3.2 Create `Assets/MoMing/Scenes/Test/Test_DoorNav.unity`: instantiate the L04/L045 content roots (mirroring real scene roots, minus persistent/flow objects), camera + directional light, GM component attached; scene not added to Build Settings.
- [x] 3.3 In the test scene only, enable `dynamicDoorNavigation` on both monster instances (scene-level override).

## 4. Verification (test scene)

- [x] 4.1 Open a door via GM key â†?right-click beyond it â†?monster paths through the doorway.
- [x] 4.2 Close that door while monster approaches â†?route cuts immediately during swing animation.
- [x] 4.3 Order monster into doorway then close â†?monster slides out within ~0.2 s.
- [x] 4.4 Force an unreachable target (all doors closed, click far side) â†?monster returns to waypoints and keeps moving; no freeze.
- [x] 4.5 Press F â†?monsters run along valid routes toward the marker, honoring walls and closed doors.
- [x] 4.6 Console shows no errors across all scenarios.

## 5. Regression & handoff

- [ ] 5.1 Spot-check FormalLevel01/03 play mode briefly: doors behave as before (no event-related regressions), monsters unchanged.
- [x] 5.2 Update task statuses; remind user of manual FormalLevel04 edits: enable flag on both monster instances, enlarge `areaCenter`/`areaSize` to cover the L4â†’L4.5 pursuit route, verify door blockingCollider references.
