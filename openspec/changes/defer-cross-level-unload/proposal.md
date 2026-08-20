## Why

The first-level key currently advances to Level02 without opening the shared transition door, while the flow controller unloads the prior level as soon as the successor checkpoint is reached. The intended route keeps both levels and the shared door available until the player restarts Level02.

## What Changes

- Open the shared Level01-to-Level02 door when the Level01 key is collected.
- Load `FormalLevel02` immediately after opening the door.
- Keep `FormalLevel01` loaded while Level02 is active and until Level02 restart.
- Treat the Level02 successor checkpoint as arrival confirmation only; it must not unload Level01 or close the shared door.
- When Level02 is restarted, unload Level01 and close the shared Level01-to-Level02 door before resetting Level02.
- Preserve the existing shared-art scene while either adjacent level requires it.

## Capabilities

### New Capabilities

- `cross-level-transition-lifecycle`: Defines shared-door opening, delayed predecessor unload, arrival confirmation, and restart cleanup for adjacent formal levels.

### Modified Capabilities

- None.

## Impact

- `FormalHumanKey`, `FormalGameFlowController`, `FormalCheckpoint`, and `FormalDoor` lifecycle coordination.
- Formal Level01/Level02 shared-art loading and scene unloading.
- Editor regression tests for key pickup, door state, checkpoint confirmation, and restart cleanup.
