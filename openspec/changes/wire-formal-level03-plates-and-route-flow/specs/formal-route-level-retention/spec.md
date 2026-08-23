## Purpose

Define how forward route progression cleans up stale levels: which levels remain loaded while playing a given formal level, and when older levels' doors close and scenes unload.

## ADDED Requirements

### Requirement: Arrival cleanup of stale levels

When the active route level advances to a new level, the route flow SHALL close every door located inside the previous two level scenes and SHALL unload every level scene older than the direct predecessor. The direct predecessor SHALL stay loaded while players are inside the new level.

#### Scenario: Arriving two levels ahead retires the oldest level

- **WHEN** players arrive at the next-next level of the route
- **THEN** all doors inside the previous two level scenes are closed and the oldest of those scenes is unloaded

#### Scenario: Direct predecessor remains available

- **WHEN** players are anywhere inside the newly active level
- **THEN** the direct predecessor level remains loaded so actors still inside it do not fall out of the world

### Requirement: Shared-art pruning follows retention

Shared-art scenes SHALL stay loaded while they serve the active level or its retained predecessor, and SHALL unload once they serve neither, using the existing unused-shared-art pruning behavior.

#### Scenario: Exclusive shared art of a retired level unloads

- **WHEN** a level scene is unloaded by arrival cleanup
- **THEN** any shared-art scene that served only that retired level and its neighbors outside the retention window unloads as well

### Requirement: Reset path unaffected

Restarting or resetting the current level SHALL keep the existing behavior, including closing the shared transition door, unloading the pending predecessor, and resetting the current level's temporary state.

#### Scenario: Restarting from a retained predecessor

- **WHEN** the player restarts the current level while its predecessor is still retained
- **THEN** the predecessor unloads, the shared transition door closes immediately, and the current level resets as it does today
