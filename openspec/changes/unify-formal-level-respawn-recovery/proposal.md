## Why

The formal route has checkpoint and reset requirements, but the current implementation work is fragmented by level and does not yet establish one verified recovery path for every formal stage. A shared recovery contract is needed now so deaths cannot leave players, temporary objects, monsters, or route handoff state inconsistent.

## What Changes

- Add a shared formal-level respawn and recovery capability for Level 1 through Level 5, including Level 4.5 where it participates in the route.
- Standardize all initial-entry and reset destinations on separate `HumanRespawnAnchor` and `DogRespawnAnchor` pairs; remove the formal route's `HumanSpawn` and `DogSpawn` dependency.
- Position each actor from its selected respawn anchor's XZ only, resolving its Y by a downward query against valid ground rather than trusting the anchor height.
- Reset only temporary level-local state after anxiety failure or monster capture, while retaining current-level permanent progress and the active checkpoint.
- Define recovery behavior while a successor level is loaded but its checkpoint handoff has not committed.
- Define Level 5 escape as a resettable substage whose monsters, cabinet, exit, and controlled-mode state restore on death.
- Add per-level configuration, validation, and Play Mode acceptance tasks for the complete formal route.

## Capabilities

### New Capabilities

- `formal-level-respawn-recovery`: Defines consistent checkpoint selection, player relocation, resettable state restoration, permanent-progress retention, and route-handoff recovery across the formal route.

### Modified Capabilities

- None.

## Impact

- Affected runtime flow includes formal level controllers, checkpoint triggers, player death/reset handling, resettable level-local mechanics, monsters, movable objects, and additive scene transition state.
- Affected content includes Formal Level 1 through Formal Level 5 scene checkpoint and respawn-anchor configuration, including Formal Level 4.5 where applicable.
- Requires focused Unity Play Mode validation of both pre-checkpoint and post-checkpoint recovery for each route stage.
