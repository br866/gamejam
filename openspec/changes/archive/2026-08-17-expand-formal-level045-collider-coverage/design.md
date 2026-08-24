## Context

FormalLevel045 has 156 visual renderers and only five foundational colliders. It contains a long corridor with walls, doors, wall tiles, waiting chairs, stretchers, pushable boxes, and other fixed props.

## Goals / Non-Goals

**Goals:**
- Add practical physical coverage for static Level 4.5 environment objects.
- Keep respawn anchors clear and retain the foundation floor and boundaries.

**Non-Goals:**
- Add player, enemy, pickup, switch, checkpoint, or exit logic.
- Make lights, signs, small decoration, or future mechanic visuals blocking.

## Decisions

### Use scene-owned proxy colliders

Proxies are added under `L045_CollisionRoot/L045_BroadColliderCoverage` from retained renderer world bounds. They do not alter L045_Content components or transforms.

### Exclude non-obstructive and overhead visual roles

Wall/door/tile/partition/furniture/large fixed props receive collision. Lamps, signs, pictures, bottles, buttons, overhead objects, small clutter, and narrow visual hints remain non-blocking.

## Risks / Trade-offs

- [Broad proxy blocks spawn] -> Exclude anchor neighborhoods and validate capsule overlap after creation.
- [Some valid props lack collision] -> Add them in a targeted follow-up based on visual review rather than blocking the corridor indiscriminately.
