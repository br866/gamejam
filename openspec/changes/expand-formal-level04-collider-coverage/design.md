## Context

FormalLevel04 has a user-selected L04_Content visual Prefab, a basic floor, outer boundaries, and Pad-adjacent entry anchors. Four selected Plate visuals at third-level world coordinates are known cross-level content and must not receive blocking coverage.

## Goals / Non-Goals

**Goals:**
- Add broad physical presence for substantial retained Level 4 objects.
- Preserve the accepted visual layout and safe entry points.

**Non-Goals:**
- Remove or move visual objects, including the known cross-level Plate visuals.
- Implement gates, plates, monsters, pickups, navigation, or other mechanics.

## Decisions

### Use dedicated proxy colliders

Collision lives under `L04_CollisionRoot/L04_BroadColliderCoverage`, not on L04_Content renderers. Renderer world bounds determine proxy placement; classification excludes visual-only content before a proxy is created.

### Classify by physical role and world context

Walls, doors, gates, beds, cabinets, tables, carts, and large fixed props retain collision. Footprints, plates, pads, buttons, bottles, lights, pictures, signs, player/monster display meshes, and objects outside Level 4's hospital bounds remain non-blocking.

## Risks / Trade-offs

- [Broad proxies block a clear route] -> Validate entry overlap and immediate movement directions after placement.
- [A valid prop is excluded] -> The proxy group is scene-owned and can be extended after visual review without reassembling art.
