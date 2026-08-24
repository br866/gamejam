## Context

The active Unity selection contains the intended Level 4.5 corridor visuals. As with corrected Level 4 assembly, source parent transforms are not trusted for placement; each selected Renderer is copied independently with its effective global transform explicitly written.

## Goals / Non-Goals

**Goals:**
- Preserve all selected visual Renderer placements without relying on names or hierarchy.
- Exclude only player-system display renderers that conflict with FormalPersistent ownership.

**Non-Goals:**
- Add traversal collision, spawn anchors, mechanics, checkpoints, exits, or route flow.
- Prune selected Level 4 edge content automatically.

## Decisions

### Flatten selected renderers by global transform

Each Renderer is copied to a flat content Prefab root and receives source world Position, Rotation, and Lossy Scale. This avoids parent-scale drift and permits immediate world-Bounds verification before switching scenes.

## Risks / Trade-offs

- [Selection includes adjacent Level 4 visual content] -> Preserve it because user selection is authoritative; document it for later visual review.
- [Prototype player renderers duplicate formal actors] -> Exclude PlayerSystem descendants only.
