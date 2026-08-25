## Why

When a human attaches to a stationary `FormalPushableCrate`, a residual horizontal Rigidbody velocity is restored after every attachment snap. The actor is pulled back to its interaction point and then immediately slides away in the physics step, producing visible X/Z jitter even though the crate is still.

## What Changes

- Ensure an attached actor has no independent X/Z Rigidbody velocity after mover-point synchronization, while preserving its vertical velocity for gravity and grounding.
- Clear residual horizontal velocity when a human first attaches to a pushable crate.
- Keep the existing crate movement, collision, vertical physics, and push/pull animation behavior unchanged.

## Capabilities

### New Capabilities

- None.

### Modified Capabilities

- `crate-push-movement`: Attached players remain at their selected interaction points without horizontal jitter while the crate is stationary.

## Impact

- Updates `FormalPlayerActor` mover-point synchronization and `FormalPushableCrate` engagement behavior.
- Requires Unity Play Mode verification in FormalLevel01 and the existing crate mechanics test scene where available.
