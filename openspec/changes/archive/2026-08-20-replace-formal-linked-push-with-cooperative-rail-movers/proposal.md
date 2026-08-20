## Why

The formal route's current Q linked mode only binds the dog to the human's position, while the Level 1 crate remains a freely sliding Rigidbody that the human can move alone. This does not create a readable cooperative physical puzzle and cannot safely support the planned Level 5 cabinet escape.

## What Changes

- **BREAKING** Replace formal-route Q position binding with explicit two-actor cooperative rail movers; legacy prototype Q behavior remains unchanged.
- Add a reusable rail mover with two configured interaction nodes, a single configurable movement axis, bounded travel, and deterministic reset.
- Require each actor to press F at their assigned node before a rail mover becomes movable.
- Keep both actors attached to their nodes while cooperation is active; the currently active actor's movement input drives the mover forward or backward along its rail.
- Update the Level 1 wooden crate from human-only free physics pushing to a two-actor rail mover while preserving its use as the physical step for the human-only key route.
- Reuse the same mechanism for the Level 5 medicine cabinet escape.

## Capabilities

### New Capabilities
- `formal-cooperative-rail-movers`: Explicit two-actor interaction-node, single-axis movement, bounded travel, cancellation, and reset behavior for heavy cooperative objects.

### Modified Capabilities
- `formal-route-gameplay-coverage`: Replace the Level 1 crate's free human push with cooperative rail movement while retaining the human-only key route outcome.

## Impact

- Affected runtime: formal player control, Level 1 crate behavior, and a new reusable cooperative rail mover component.
- Affected assets: `L01_MovableStep_WoodenCrate`, Formal Level 1 configuration, and Level 5 cabinet configuration when its escape segment is implemented.
- Prototype `PlayerManager`, `PushableBox`, and their Q linked mode remain unchanged.
