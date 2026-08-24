## Why

Level 2's cooperative E interaction currently calls the same direct route-advance path used by GM keypad 2, 6, and 8 controls. Opening the shared L2-to-L3 door therefore immediately teleports both players to Level 3 instead of allowing them to cross the opened doorway.

## What Changes

- Separate normal Level 2 door opening from GM direct level transitions.
- When the cooperative L2 door is opened with E, preload Level 3 additively without repositioning either player.
- Mark Level 3 as active only after both players physically enter a Level 3 arrival area beyond the shared door.
- Preserve keypad 2, 6, and 8 as explicit GM direct-transition controls.

## Capabilities

### New Capabilities

- `formal-level02-physical-door-transition`: Defines the physical L2-to-L3 traversal sequence independently from GM level jumps.

### Modified Capabilities

- None.

## Impact

- `FormalDoorInteraction`, `FormalGameFlowController`, and the Level 2/3 transition scene setup.
- Level 2 cooperative door behavior and focused runtime tests.
