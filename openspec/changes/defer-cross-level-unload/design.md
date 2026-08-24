## Context

Formal route levels are loaded additively and share art scenes at transitions. `pendingUnloadScene` already represents a predecessor kept for checkpoint fallback, but `NotifySuccessorCheckpointActivated` currently unloads it immediately. The shared Level01-to-Level02 door is owned by `FormalSharedArt_L01_L02` and must remain open while both adjacent route levels are active.

## State Model

```text
Level01 active
  predecessor = none
  transition door = closed

Key collected
  door = open permanently
  load Level02
  predecessor = FormalLevel01
  transition confirmed = false

Level02 successor checkpoint reached
  predecessor remains loaded
  transition door remains open
  transition confirmed = true

Restart Level02
  close transition door immediately
  unload FormalLevel01
  clear predecessor state
  reset Level02 temporary/permanent states
```

## Decisions

### Key owns transition initiation

The key resolves the flow controller and asks it to open the current-to-successor transition door before loading the successor. The key does not hold a cross-scene `FormalDoor` reference.

### Flow controller owns shared-door lookup and cleanup

The flow controller searches the currently loaded shared-art scenes for the transition door associated with the current route edge. It opens the door before successor loading and closes it during restart cleanup. This keeps scene ownership out of gameplay pickups.

### Checkpoint confirms arrival but does not unload

`FormalCheckpoint` continues to set the current level checkpoint and notify the flow controller. The flow controller records arrival confirmation, but predecessor unloading is deferred until the current level is restarted.

### Restart handles predecessor cleanup

When restarting a level with a pending predecessor, the flow controller closes the transition door, unloads the predecessor level, clears the pending state, and then resets the current level. Shared art remains loaded because it is still required by the current level.

## Risks

- A transition door may not be found if the shared-art scene is not loaded; loading the required shared-art scenes precedes door resolution.
- A generic door search could select the wrong door; the lookup must use the route edge and the door's successor relationship/name contract.
- Restart during an in-progress load must be ignored by the existing operation guard.
