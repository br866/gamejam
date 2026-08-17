## Context

Floor_CenterNorth ends at world `z=-7.015`; Floor_CenterWestSouth begins at `z=-13.110`; both cover world x `-75.870..-59.810` at floor height y `9.80`.

## Goals / Non-Goals

**Goals:**
- Fill only the measured center-floor gap with a scene-owned BoxCollider.
- Match adjacent floor height and overlap edges slightly to prevent precision seams.

**Non-Goals:**
- Resize or move existing floors, change art, or alter routes outside the identified gap.

## Decisions

### Add a dedicated connector volume

The connector will use the shared x span, y height, and thickness of its adjacent floors, extending slightly into both existing volumes to eliminate physics seams.

Alternative considered: resize an adjacent floor. Rejected because a dedicated named connector preserves the original floor region definitions and localizes the repair.

## Risks / Trade-offs

- [Small seam remains at the edges] -> Overlap the new volume slightly with both adjacent volumes.
