## Context

See `proposal.md`. The current source-scene selection contains 270 objects over world bounds `(-83.30, 0.39, -32.80)` to `(-6.04, 17.39, 26.31)`. Of these, 149 have an explicit `Static Scene/Level3` hierarchy path, two have a `Level4` path, and five share a world position with existing Level 2 visual content. The remaining selected objects include shared static scene content, floors, outer walls, interactive objects, player-related objects, and models with no sufficient Level 3 attribution.

## Goals / Non-Goals

**Goals:**
- Establish Formal Level 3 as a scene-owned, source-audited art assembly.
- Preserve confirmed Level 3 world transforms and avoid duplicating Formal Level 1 or 2 content.
- Leave a clear audit queue for shared or unresolved source candidates.

**Non-Goals:**
- Infer that every selected shared object belongs to Level 3.
- Configure Level 3 collision, gameplay, navigation, character spawns, checkpoints, or exits.
- Move, delete, or re-parent objects in the source scene.

## Decisions

### Use explicit Level3 hierarchy attribution as the initial acceptance set

Objects beneath `Static Scene/Level3` are the initial confirmed Level 3 visual candidate set. They still undergo world-position checks against existing formal art to prevent cross-level duplication.

Alternative considered: migrate the whole selection and remove non-Level 3 objects afterward. Rejected because the selection includes known Level 4, player, interaction, and shared content.

### Treat shared selection objects as review candidates

Objects without explicit Level 3 attribution are not automatically discarded or migrated. The manifest records their identity and world position as unresolved candidates, allowing later visual review to include them without losing provenance.

Alternative considered: infer ownership solely from world bounds. Rejected because Level 2, Level 3, Level 4, and shared art overlap in the prototype scene.

### Preserve effective source world transforms in a flattened content Prefab

The Level 3 content Prefab uses the source objects' effective world position, rotation, and scale. Source GlobalObjectId remains the primary audit identity; mesh GUID/local file ID is secondary.

Alternative considered: preserve source local transforms and hierarchy. Rejected because source parent hierarchy is mixed-level and cannot safely become Formal Level 3 ownership.

## Risks / Trade-offs

- [Explicit Level3 path omits shared art needed for the final visual route] -> Record unresolved candidates for a later visual review rather than making untraceable assumptions.
- [An accepted object duplicates prior formal content] -> Compare world positions against Level 1 and Level 2 content before assembly.
- [Flattened Prefab loses source object identity] -> Record source-to-destination mapping during copying and verify world transforms after assembly.

## Migration Plan

1. Read the active source selection and record each object identity, hierarchy path, mesh identity, and world Transform.
2. Classify explicit Level 3 visuals, prior-level duplicates, Level 4 objects, player/runtime objects, interaction objects, and unresolved shared candidates.
3. Create Formal Level 3 and a Level 3-owned content Prefab from only accepted visual objects.
4. Verify source-to-destination world transforms and source-scene preservation.
5. Write the Level 3 manifest and defer traversal and mechanics work to later changes.
