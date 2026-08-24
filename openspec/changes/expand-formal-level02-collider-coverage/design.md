## Context

See `proposal.md`. Level 2 currently relies on four floor volumes, boundaries, and architecture wall proxies. Its copied art Prefab contains many visible objects without collision. `FormalPersistent` already owns the player spawner and a serialized `initialLevelScene`, but it is set to Level 1 in the scene asset.

## Goals / Non-Goals

**Goals:**
- Make the migrated Level 2 environment broadly tangible for physical player testing.
- Keep known route anchors and visual clues traversable.
- Make direct Level 2 testing use the production persistent-spawner and additive-flow path.

**Non-Goals:**
- Add or wire puzzle behavior, monster AI, door state changes, exits, or character-specific mechanisms.
- Place player instances in `FormalLevel02`.
- Replace the existing route foundation or source-art identity mapping.

## Decisions

### Use primitive colliders where practical and mesh colliders for irregular static visuals

Existing dedicated route volumes remain the authoritative floor and boundary collision. Broad coverage will add BoxCollider, CapsuleCollider, or MeshCollider according to the visible object's shape, static state, and rendered bounds. Collider components remain on the Level 2 content assembly or dedicated collision root, never in the source prototype scene.

Alternative considered: a single large collision shell. Rejected because it would not match individual furniture, doors, or monster shapes for collision testing.

### Treat visual guidance as non-blocking

Footprints, particles, and explicitly decorative small visuals remain without blocking collision. Doors and monsters receive collider coverage but their behavior remains unchanged.

Alternative considered: add a collider to every Renderer. Rejected because it would turn visual guidance and incidental decoration into unintended route blockers.

### Configure direct testing through FormalPersistent

The serialized initial scene selection in `FormalPersistent` is the test entry point. Developers select `FormalLevel02`, enter Play Mode from `FormalPersistent`, and use the same player-spawner/game-flow configuration as normal play. The default shipped value remains `FormalLevel01` after verification.

Alternative considered: duplicate player Prefabs into every level. Rejected because it creates duplicate actors when scenes are loaded additively and bypasses the formal game flow.

## Risks / Trade-offs

- [MeshCollider on an animated or dynamic object causes physics issues] -> Apply MeshCollider only to static visuals; use primitives or no collider for dynamic/visual-only objects.
- [Broad collider coverage blocks a verified route] -> Re-run human and dog overlap, grounding, and route checks after each object category.
- [Direct testing changes the normal first level] -> Restore `FormalPersistent` to `FormalLevel01` before completing the change and document the test procedure.

## Migration Plan

1. Inventory migrated Level 2 Renderers and classify them as blockers or visual-only.
2. Add collision by class, preserving existing floor, boundary, and wall volumes.
3. Validate both actors at entrance/checkpoint anchors and approved route segments.
4. Set `FormalPersistent` to Level 2 only for Play Mode verification, then restore its Level 1 default.
5. Record collider counts, exclusions, and direct-test procedure in the Level 2 manifest.
