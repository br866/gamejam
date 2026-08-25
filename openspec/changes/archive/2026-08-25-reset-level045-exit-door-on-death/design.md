## Context

The Level 4.5 exit is a shared-art `FormalDoor`, so it is not collected by the Level 4.5 controller’s scene-local temporary-state reset. Its scene-local `FormalCrateDoorTrigger` also retains an internal completion flag after opening, so merely closing the shared door would leave the retry unable to open it again.

## Goals / Non-Goals

**Goals:**

- Close the shared Level 4.5-to-Level 5 transition door during either Level 4.5 or Level 5 recovery.
- Perform that closure before the recovered players resume the stage.
- Keep the route handoff itself unchanged while resetting the shared door on later Level 5 recovery.

**Non-Goals:**

- Reclassify every shared transition door as temporary.
- Change checkpoint, pursuit, or Level 5 recovery behavior.

## Decisions

The route flow will explicitly close the named Level 4.5-to-Level 5 transition door when either `FormalLevel045` or `FormalLevel05` is the active recovery owner. When `FormalLevel045` owns recovery, it will additionally reset only that scene's crate-exit trigger completion state, allowing normal re-opening during a Level 4.5 retry.

This narrow route-level reset is preferred to changing `FormalDoor` from permanent to temporary, which would risk resetting completed doors in other route handoffs.

## Risks / Trade-offs

- [The exit is opened while a handoff is in flight] → Guard the reset by the active-level identity so only Level 4.5 or Level 5 recovery can close it.
- [The shared door or crate-exit trigger cannot be found] → Emit a targeted diagnostic and leave the remaining recovery path operational.

## Migration Plan

1. Add the Level 4.5-specific exit-door close operation to recovery.
2. Compile and test death after opening the Level 5 exit, then verify a normal completed handoff is unchanged.
3. Revert the targeted recovery operation to restore prior behavior.
