# Design: make-formal-level04-monster-nav-react-to-doors

## Context

- `LevelMonsterNavigation` builds one shared runtime GridGraph ("Level2MonsterGraph", found by name so both L04 monsters reuse it), collision mask = NavStatic + NavDynamic, scanned once in `Start`. Node size 0.5, actor diameter 1 (capsule diameter 2 nodes).
- L04 doors are `L01_Door_Mechanism` instances with root layer overridden to 8 (NavDynamic); blocking BoxCollider ≈ 3.45 × 7 × 0.25 on the root; all three sit inside the nav area.
- Door logical state lives in `FormalDoor.IsComplete` (`IsOpen`). Physics collider disables instantly on Open but re-enables only after the closing animation finishes — collider state cannot express "closed" instantly, so it must not be the signal for path blocking.
- `LevelMonsterNavigation.Update` early-returns forever once `currentPath.error` is set, and `SetDestination` early-outs on identical targets → permanent stall after one failed request.
- `ForcedChase` moves by transform straight-line; the monster prefab has no physics body, so nothing stops it.
- A* API surface already proven in-repo (`Astar_ClickMovement_Test`): `GraphUpdateObject`, `FlushGraphUpdates`, queued graph updates.

## Decisions

### D1. Logical door state, not colliders, drives walkability
Use a GUO with `modifyWalkability = true, setWalkability = door.IsOpen` over the door's blocking-collider world bounds (contracted ~0.05 to avoid grazing frame posts). Rationale: satisfies "instant block on Close" regardless of animation/collider timing; zero changes to player-facing physics.

Known risk (accepted for v1): if the contracted bounds overlap frame-post nodes, those nodes get force-walkabled and monsters may shave corners. Fallback plan if playtest shows clipping: per-node `Physics.OverlapBox` filtering that only frees nodes overlapped solely by this door's collider, or switch to `updatePhysics` GUO plus an opt-in instant-collider flag on FormalDoor doors.

### D2. Event-driven, not polling
`FormalDoor` gains `public event System.Action<FormalDoor> StateChanged`, raised at the end of `Open()`, `Close()`, and `SetStateImmediate()` — every state transition funnels through these three (Awake's initial `SetClosedImmediate` fires before subscribers exist; harmless). Navigation subscribes at `Start` when the flag is on, unsubscribes in `OnDestroy`. On event: apply GUO → `AstarPath.FlushGraphUpdates()` → force `RequestPath()`. Multiple subscribers applying idempotent updates to the shared graph is safe.

### D3. Stall fix + failure signaling
In `Update`, treat an errored `currentPath` as retryable: clear it and let the normal repath timer re-request. Expose `public bool LastPathFailed` set from `OnPathComplete`. Consumers decide behavior:
- `Chase`: on failure → drop target, return to `Patrol` (waypoints keep it moving).
- `ForcedChase`: on failure → temporarily steer to the nearest/current waypoint (~1 s hold before retrying the player) while keeping `forcedChase` true, so pursuit resumes automatically once connectivity returns.

### D4. ForcedChase routes through navigation
Rewrite `ForcedChase` to mirror `Chase`'s navigation branch: `SetMoveSpeed(chaseSpeed)`, `SetDestination(player)`, stop-distance clearing. Keep the legacy straight-line body only as the `navigation == null` fallback. `BeginForcedChase` calls `navigation.RescanGraph()` once so geometry loaded since monster `Start` (L045 content) is baked before pursuit begins.

### D5. Push-out slide
After a close event's graph flush, check the monster's nearest node; if unwalkable, run a short coroutine sliding `transform.position` toward the nearest walkable node center over ~0.2 s, pausing `FollowPath` for that duration, then re-request the path.

### D6. Opt-in flag and scene ownership
`[SerializeField] bool dynamicDoorNavigation` defaults false; when false the component behaves exactly as today (no door discovery, no subscriptions). Enabling the flag and enlarging `areaCenter/areaSize` for the two FormalLevel04 monster instances is a manual editor task owned by the user; scripts never hard-code scene data.

### D7. Test isolation
`Assets/MoMing/Scripts/Debug/DoorNavTestGm.cs`: entire class inside `#if UNITY_EDITOR`; only calls public APIs (`Open/Close/SetDestination/BeginForcedChase`). Scene `Assets/MoMing/Scenes/Test/Test_DoorNav.unity`: static copies of the L04/L045 content roots, camera auto-framed by GM script, excluded from Build Settings. No persistent/flow controllers are imported, so no production systems run there.

## Risks / Trade-offs

- Walkability GUO bypasses erosion: freed corridor width equals raw gap minus existing neighbor unwalkables; validated visually against grid gizmos (mitigation in D1).
- Same-frame `FlushGraphUpdates` costs one synchronous partial update per event; events are human-scale (door toggles), not per-frame.
- Rescan on `BeginForcedChase` causes a one-time hitch during the scripted moment — accepted (jam scope).

## Migration

None required. Defaults preserve current behavior everywhere except opted-in L04 monsters.
