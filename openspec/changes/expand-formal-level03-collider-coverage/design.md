## Context

See `proposal.md`. Formal Level 3 contains the user's manually curated art layout, eight floor volumes, four boundaries, twelve wall proxies, and entrance, checkpoint, and provisional-exit anchors. The visual content intentionally has runtime behavior stripped, so physical coverage must remain scene-owned and must not rebuild or filter the content Prefab.

## Goals / Non-Goals

**Goals:**
- Add broad physical coverage that matches retained Level 3 static visuals.
- Preserve the known-good dual-character spawn and baseline route.
- Keep the user's current visual curation unchanged.

**Non-Goals:**
- Change the retained renderer set, source-art mapping, transforms, or visual hierarchy.
- Add puzzle mechanics, door state logic, enemy behavior, navigation, or an exit trigger.
- Add player actors to the Level 3 scene.

## Decisions

### Preserve the dedicated traversal foundation

Existing floor, boundary, and wall-proxy volumes remain authoritative for anchors and the baseline route. Broader fixed-prop coverage is additive and is placed in a dedicated Level 3 collision hierarchy where possible.

Alternative considered: replacing current route volumes with per-renderer collision. Rejected because it risks invalidating already validated anchors and corridors.

### Classify by physical obstruction, not source identity

Static architecture and substantial furniture receive primitive or static mesh collision according to shape. Decorative effects, visual hints, and small incidental objects remain non-blocking. Collider eligibility is determined by the retained scene object's physical role, never by its source path, name, mesh reuse, or prototype components.

Alternative considered: attach a collider to every renderer. Rejected because it makes dense decorative art block character movement.

### Test through FormalPersistent

Direct verification temporarily selects `FormalLevel03` in the existing persistent game-flow configuration, preserving the production additive loading path and its single player spawner. The persisted default is restored to `FormalLevel01` afterward.

Alternative considered: placing test players in Level 3. Rejected because it creates duplicate player ownership when loaded additively.

## Risks / Trade-offs

- [Broad collider coverage blocks a verified route] -> Retain the route foundation and run both actor capsule overlap, support, and path checks after applying coverage.
- [Irregular static art receives poor primitive fit] -> Use concise proxy geometry only where needed and do not alter the renderer transform.
- [Direct test changes normal startup] -> Restore `FormalLevel01` after the Level 3 Play Mode verification and record the procedure.

## Migration Plan

1. Inventory the retained Level 3 renderers and classify blocking and visual-only objects.
2. Add broad static coverage without changing visual content.
3. Revalidate anchors and baseline routes with both player capsules.
4. Enter Play Mode through `FormalPersistent` with `FormalLevel03`, then restore the default startup scene.
5. Record final coverage, exclusions, and verification in the Level 3 manifest.
