## Context

See `proposal.md`. The current selection contains 220 objects with 217 renderers spanning `x=-85.19..-17.39`, `y=-2.27..18.86`, and `z=-37.01..25.27`. The first Level 3 assembly now contains 163 visual objects, but the user reports missing nearby objects. The scan expands the selection bounds by eight units to capture supporting art while retaining explicit exclusions.

## Goals / Non-Goals

**Goals:**
- Complete the visual region surrounding the user-selected Level 3 area with a deliberately broad inclusion rule.
- Prevent only Level 2 and Level 4 visual duplication.
- Preserve the existing Level 3 traversal foundation and remove source runtime behavior from copied visuals.

**Non-Goals:**
- Infer or implement gameplay ownership for nearby objects.
- Add collision or mechanics to new art during this change.
- Modify the source scene or existing Formal Level 1/2 art.

## Decisions

### Use an eight-unit expanded selection volume

The combined Renderer bounds of the current selection are expanded by eight units in all directions. This captures adjacent walls, dressing, and structural continuity without scanning the entire mixed prototype map.

Alternative considered: copy the entire prototype scene. Rejected because the user explicitly excludes Level 2 and Level 4 content.

### Exclude by hierarchy attribution and formal world-position identity

Objects with paths containing Level2 or Level4 are excluded. All other nearby visual objects are accepted unless Formal Level 3 already contains an object at the same world position. The same object name, mesh, source hierarchy, or prior-level appearance does not exclude a nearby object whose world position differs.

Alternative considered: use hierarchy alone. Rejected because shared hierarchy is intentionally allowed by the user's broad inclusion rule; world position is the only duplicate key for the Level 3 visual assembly.

### Rebuild the visual Prefab from the accepted union

Use the existing Level 3 visual objects plus accepted nearby candidates as one clean flattened Prefab, with each object's world Transform set explicitly. This avoids incremental duplicate children and preserves a single audit boundary.

Alternative considered: append only candidates. Rejected because prior incremental additions have shown that mixed source-parent transforms can make position-based duplicate checks unreliable.

## Risks / Trade-offs

- [Expanded bounds reaches Level 4 art] -> Exclude source paths attributed to Level 4.
- [Nearby prototype systems look like art] -> Copy visual components only and record the stripped runtime behavior.
- [New visual object overlaps a baseline route] -> Re-run Level 3 anchor and CapsuleCast checks after rebuilding.

## Migration Plan

1. Capture current selection bounds and scan source Renderers in the expanded volume.
2. Classify candidates as accepted, Level 2/4 excluded, same-position duplicate, or non-visual.
3. Rebuild `L03_Content` from the existing accepted union with explicit world transforms and visual components only.
4. Revalidate Level 3 anchors and baseline routes.
5. Record source identities, counts, bounds, and exclusions in the Level 3 manifest.
