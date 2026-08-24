## Why

Formal Level 3 has an uncovered six-unit floor-collider gap between Floor_CenterNorth and Floor_CenterWestSouth. A player entering this connector region can lose ground support despite the intended continuous environment.

## What Changes

- Add a scene-owned floor Collider that exactly fills the gap between the two existing center floor volumes.
- Preserve both existing floor volumes, all visual objects, and the current FormalLevel03 direct-test startup configuration.
- Validate support and crossing over the repaired connector.

## Capabilities

### New Capabilities
- `formal-level03-center-floor-connector`: Defines continuous player floor support across the center north-to-south gap in Formal Level 3.

### Modified Capabilities
- None.

## Impact

- Affects the Level 3 scene-owned collision root only.
- Does not change art, player behavior, checkpoints, or mechanics.
