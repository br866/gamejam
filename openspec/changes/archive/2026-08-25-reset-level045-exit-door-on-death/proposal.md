## Why

During Level 4.5 recovery, an already opened exit door to Level 5 remains open after the player dies. This leaves temporary escape progress active across a failed attempt and lets the player bypass the intended retry.

## What Changes

- Reset the Level 4.5-to-Level 5 exit door to its closed state whenever recovery occurs in either Level 4.5 or Level 5.
- Reset the corresponding door interaction so the closed door can be opened again after recovery.

## Capabilities

### New Capabilities
- `formal-level045-exit-door-recovery`: Defines reset behavior for the Level 4.5-to-Level 5 exit door during death recovery.

### Modified Capabilities
- None.

## Impact

- Level 4.5 recovery flow in `FormalGameFlowController`.
- The shared-art Level 4.5-to-Level 5 door and the Level 4.5 crate-exit trigger state.
