## Why

`SuccessorCheckpoint` in `FormalLevel02` currently treats any player overlap as a route-advance request. Direct Level 2 startup places players in that volume, bypassing the intended pedal, cooperative safe-zone, and E-key door-opening sequence.

## What Changes

- Bind every formal checkpoint to one owning formal level and register that checkpoint only once for its owning level.
- Make checkpoint registration a Level-local save/respawn concern; it SHALL NOT open a transition door or request route advancement.
- Configure the Level 2 progression path: press the pedal, have both players enter `L02_CooperativeSafeZoneTrigger`, then use E in that safe zone to open and advance through the L2-to-L3 door.

## Capabilities

### New Capabilities

- `formal-level-bound-checkpoints`: Gives each formal checkpoint one owning level and one registration opportunity without allowing it to advance the route.

### Modified Capabilities

- None.

## Impact

- `FormalCheckpoint`, formal level/flow registration state, and checkpoint scene configuration.
- Level 2 cooperative door gating, including its safe-zone E interaction; no spawn-anchor relocation or route catalog changes.
