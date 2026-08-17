## Context

FormalLevel05 has an L05_Content visual Prefab with 112 Renderers and no collision or player anchor foundation. The selected Level 5 hospital area is approximately x `-234..-205`, z `-20..20`, with rendered floors near y `9.2`.

## Goals / Non-Goals

**Goals:**
- Establish safe human/dog entry and respawn anchors.
- Add scene-owned floor, boundaries, and broad static obstacle collision.

**Non-Goals:**
- Implement the final exit, progression, gates, pickups, enemies, or player control changes.
- Change the user-selected visual layout.

## Decisions

### Use scene-owned collision proxies

Floor, boundaries, and broad static object proxies live below `L05_CollisionRoot`; L05_Content remains visual-only. The proxy classifier keeps floor tiles, lights, signs, small details, and mechanic visuals non-blocking.

### Place anchors in a clear western-side floor region

Anchors are selected by live support and capsule-overlap checks, not by source object names. They remain independent from future Level 5 mechanic locations.

## Risks / Trade-offs

- [A broad proxy blocks entry] -> Exclude the anchor neighborhood and validate immediate movement after proxy creation.
- [A valid small prop lacks collision] -> Add targeted proxies only after visual review rather than blocking the route indiscriminately.
