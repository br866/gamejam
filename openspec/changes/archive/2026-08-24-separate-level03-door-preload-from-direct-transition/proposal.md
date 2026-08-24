## Why

Level 3's cooperative exit trigger currently requests a direct route advance, which immediately transfers both players into Level 4 as soon as the L3-to-L4 door opens. The transition needs to preserve physical traversal through the shared door, matching the verified Level 2-to-Level 3 behavior.

## What Changes

- Change the Level 3 cooperative exit from an immediate route advance to opening the shared door and preloading Level 4 without moving either player.
- Confirm Level 4 only after both players physically enter its existing arrival area, then perform normal Level 3 cleanup without repositioning them.
- Preserve GM direct level-change commands as immediate transitions, including cancellation of an unfinished physical L3-to-L4 transition.
- Apply any serialized configuration only as a Level 3 scene-instance override; do not manually modify prefab assets.

## Capabilities

### New Capabilities

- `formal-level03-physical-door-transition`: Defines physical cooperative progression from Level 3 into Level 4 while retaining immediate GM transitions.

### Modified Capabilities

- None.

## Impact

- `FormalActuatorTrigger` and Level 3's cooperative exit configuration.
- Existing physical-transition and level-entry-seal handling in `FormalGameFlowController` and `FormalLevelEntrySeal`.
- The `FormalLevel03` scene instance and Level 4's existing arrival seal.
