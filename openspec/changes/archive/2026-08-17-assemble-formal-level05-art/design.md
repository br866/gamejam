## Context

The active Unity selection contains the intended final Level 5 hospital visuals. Source parent transforms are unreliable for direct hierarchy copying, so each selected Renderer must be flattened and assigned its effective world transform explicitly.

## Goals / Non-Goals

**Goals:**
- Preserve selected Renderer placement exactly by global transform.
- Exclude only the prototype PlayerSystem display renderers.

**Non-Goals:**
- Add collision, entry anchors, gameplay, checkpoints, exit behavior, or route transitions.
- Automatically remove selected adjacent Level 4.5 content.

## Decisions

### Flatten selected renderers by global transform

Each selected Renderer is copied to a flat Prefab root with source world Position, Rotation, and Lossy Scale explicitly written. Source-to-Prefab Bounds are verified before the scene switch that clears Unity Selection.
