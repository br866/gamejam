## Context

The prototype source is `Assets/Scenes/Test/superbreadman 1.unity`. Explicit Level 4 source hierarchy contains `floor/Level4`, `Item/Level4`, and `Static Scene/Level4`; `floor/Level4.5` is a separate downstream route and must remain excluded. The Level 4 hospital area is approximately x `-126..-81`, z `-34..14` with a source visual floor near y `9.8`.

## Goals / Non-Goals

**Goals:**
- Preserve explicit Level 4 visual world placement in a visual-only reusable Prefab.
- Establish initial player grounding without adding formal player objects to Level 4.

**Non-Goals:**
- Import Level 4.5, source runtime components, enemy AI, gates, plates, pickups, checkpoint progression, or exits.
- Alter prototype source objects or manually prune final Level 4 art.

## Decisions

### Copy explicit Level 4 groups and strip behavior

The three Level 4 source groups are copied as art roots with their effective world transform preserved. Only rendering dependencies remain; all prototype physics and behavior components are stripped. This preserves visual layout while the formal scene owns collision and traversal.

### Use a dedicated foundational floor and boundaries

FormalLevel04 owns a broad support floor and outer boundaries matching the source area. This gives the persistent player pair a stable entry while detailed per-prop collision is deferred until the visual layout is reviewed.

## Risks / Trade-offs

- [Explicit Level4 hierarchy contains distant prototype dressing] -> Preserve it for source fidelity and document it for later manual pruning.
- [Broad floor crosses intended rooms] -> It is restricted to foundational support; detailed walls and mechanics remain a later pass.
