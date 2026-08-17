## Context

See `proposal.md`. `FormalLevelController` currently owns separate entrance references for the human and dog, but checkpoint activation stores those entrance positions rather than checkpoint-specific positions. `FormalLevel01` already contains hand-authored collision objects and credible entrance anchors. `FormalLevel02` has accepted art at hospital-world elevations but still uses default near-origin spawn and checkpoint positions that do not correspond to that environment.

## Goals / Non-Goals

**Goals:**
- Make entrance and checkpoint positions explicit, two-character, and physically valid.
- Make reset behavior deterministic from entrance and checkpoint states.
- Define small, repeatable collision and walking checks before per-level gameplay is attached.
- Establish Level 1 and Level 2 as the first validated consumers without assuming a final route for later levels.

**Non-Goals:**
- Implement pressure plates, doors, monster behavior, footprints, or completion logic.
- Migrate additional art or modify the source prototype scene.
- Replace formal player movement or add general puzzle abstractions.

## Decisions

### Use explicit transforms for every respawn destination

Formal checkpoints will own or reference separate human and dog respawn transforms. The level controller stores those transform positions only when the checkpoint is activated, and reset returns each actor to its recorded checkpoint position. Entrance transforms remain the initial fallback when no checkpoint exists.

Alternative considered: save the actors' instantaneous positions when a checkpoint is triggered. Rejected because an actor can stand in a doorway, on a trigger edge, or on unsafe geometry, producing an invalid reset location.

### Validate anchors before routes

Each anchor is checked for ground support, blocking overlap, and post-simulation stability before route testing. Route checks then use fixed anchor pairs, allowing failures to distinguish bad spawn placement from broken collision along the route.

Alternative considered: manually walk the entire scene first. Rejected because a fall or overlap gives no stable place to reproduce the failure.

### Build collision around navigable volume, not every visible mesh

Add collision to floors, walls, boundaries, gates, and major fixed blockers required by the approved route. Leave small set dressing non-blocking unless it affects walking. Trigger colliders remain non-blocking.

Alternative considered: generate colliders for every art mesh. Rejected because it creates unstable snagging, expensive physics, and accidental route blockages.

### Keep Level 1 and Level 2 anchors scene-owned

Anchor transforms and hand-authored collision live in their formal scenes or dedicated scene-owned prefabs. Art Prefabs remain visual source assemblies and do not become authoritative player placement data.

Alternative considered: infer anchors from art object bounds at runtime. Rejected because art layout changes would silently move player starts and invalidate saved route evidence.

## Risks / Trade-offs

- [Anchor has ground visually but no physical support] -> Simulate after placement and test capsule grounding against non-trigger colliders.
- [Human and dog capsule sizes differ] -> Validate every anchor and route segment with both actor colliders, not a shared point probe.
- [A gate or furniture collider closes a valid path] -> Validate segments with the expected initial gate state and record route exceptions.
- [Level 2 art remains at a different vertical band from defaults] -> Select anchors directly from supported Level 2 geometry before changing scene transforms.

## Migration Plan

1. Inventory existing Formal Level 1 and Level 2 entrance, checkpoint, exit, floor, boundary, and blocker objects.
2. Correct formal checkpoint state to store explicit checkpoint anchors and fall back to entrance anchors before activation.
3. Establish and validate Level 1 anchor pairs and route segments using existing collision as the reference implementation.
4. Establish supported Level 2 entrance, checkpoint, and exit anchors from the accepted art layout, then add only required foundational collision.
5. Record world positions and play-mode evidence; defer mechanism-specific routes to their later level changes.
