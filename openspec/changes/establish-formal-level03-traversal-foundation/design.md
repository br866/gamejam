## Context

See `proposal.md`. Formal Level 3 contains 91 visual objects and no Collider components. Its accepted floor meshes have rendered top surfaces near `y=9.30` and occupy a contiguous world-space region approximately `x=-91` through `-29` and `z=-29` through `25`.

## Goals / Non-Goals

**Goals:**
- Establish reliable Level 3 entry and route anchors on the accepted art.
- Add only the foundational collision required for physical testing.
- Use the existing persistent player and checkpoint systems without duplicating runtime actors.

**Non-Goals:**
- Implement Level 3 gates, pressure plates, enemies, navigation graphs, UI, or exit handoff.
- Add player objects to the Level 3 scene.
- Modify the source prototype or Level 1/2 scenes.

## Decisions

### Place anchors on measured floor mesh support

Select anchor positions from the accepted Level 3 floor bounds and place player capsule centers approximately one unit above the measured floor top. Verify with downward raycasts and overlap capsules before route checks.

Alternative considered: reuse Level 2 coordinates. Rejected because Level 3 occupies a different world-space region.

### Use scene-owned primitive collision proxies

Create floor and boundary BoxColliders under a Level 3 collision root, then add wall and major blocker proxies from accepted visual bounds. Small visual decoration remains non-blocking until a route review proves it must block.

Alternative considered: add MeshCollider to every visual. Rejected because it creates unnecessary snagging and can make non-convex dynamic assets invalid physics inputs.

### Use FormalPersistent for direct testing

Temporarily set `initialLevelScene` to the exact scene name `FormalLevel03`, enter Play Mode from `FormalPersistent`, verify one human/dog pair, then restore the normal Level 1 default unless the user explicitly chooses a different testing default.

Alternative considered: place actors in FormalLevel03. Rejected because it duplicates the persistent actor pair.

## Risks / Trade-offs

- [Visual furniture blocks the selected route] -> Disable only the specific offending proxy after capsule overlap and route checks.
- [Anchor appears supported but falls at runtime] -> Confirm with actual Play Mode placement and post-physics grounding.
- [Direct test uses a wrong scene name] -> Use exact `FormalLevel03` and verify the loaded scene before evaluating player state.

## Migration Plan

1. Choose measured Level 3 entrance, checkpoint, and provisional exit points on floor meshes.
2. Add floor, boundary, wall, and major blocker proxies.
3. Configure checkpoint references and persistent direct-test startup.
4. Validate both actors in Editor physics and Play Mode.
5. Record coordinates, collider counts, route exceptions, and deferred mechanics in the Level 3 manifest.
