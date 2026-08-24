## Why

The Formal route's human/dog actors pivot at their capsule center (1 m above the feet), so every spawn point, checkpoint anchor, crate grip point, and camera focus is authored against a floating pivot. This makes visual size tuning fragile (magic offsets), keeps the dog wearing a human-sized 2 m capsule, and blocks the planned unified-ground work that needs a predictable feet-on-floor convention.

## What Changes

- **BREAKING** (physics convention): `FormalPlayerActor` roots move from capsule-center to feet-center. Capsules sit above the root via collider center offset.
- Placeholder capsule sizes: human h1.7/r0.35, dog h0.9/r0.3 (visual scale tuning stays with the designer via `FormalPlayerVisualLoader` fields).
- Add two child anchors to each actor in `FormalPlayerActors.prefab`:
  - `FocusAnchor` at (0, +1, 0) — camera target; preserves current framing exactly; freely draggable later.
  - `MoverAttachPoint` at (0, +1, 0) — semantic constant equal to the old half-height; used for all "point coincidence" placements.
- Script wiring:
  - `FormalPlayerControl.SetCameraTarget()` targets the active actor's FocusAnchor (fallback: root).
  - `FormalPushableCrate` + `FormalCooperativeRailMover` place actors by MoverAttachPoint-coincidence instead of root-at-point (engage snap + per-frame keep-at-point).
  - `FormalLevelController.MovePlayer()` places actors by MoverAttachPoint-coincidence for spawns and checkpoints (zero scene-data edits).

## Capabilities

### New Capabilities
- `formal-actor-foot-pivot`: Defines the foot-pivot convention for Formal player actors: capsule-above-root placement, anchor semantics (FocusAnchor framing, MoverAttachPoint old-half-height), and point-coincidence placement rules for spawns, checkpoints, crates, and rail movers.

### Modified Capabilities
<!-- None: no existing spec governs actor pivot/placement behavior. -->

## Impact

- `UnityProject/Assets/MoMing/FormalLevels/Prefabs/FormalPlayerActors.prefab` (capsules, anchors, initial poses)
- `Assets/MoMing/Scripts/LevelRuntime/FormalPlayerActor.cs`, `FormalPlayerControl.cs`, `FormalLevelController.cs`
- `Assets/MoMing/Scripts/Level01/FormalPushableCrate.cs`, `Assets/MoMing/Scripts/LevelRuntime/FormalCooperativeRailMover.cs`
- No scene-file edits required; no changes to animation, audio, or navigation assets. A* graphs are unaffected by this change (dog follower already flattens Y).
