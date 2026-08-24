## Purpose

Defines a reliable, player-visible recovery lifecycle for the shared formal player pair across every formal route level and its active substage state.

## ADDED Requirements

### Requirement: Shared per-level respawn destination

Every formal route level, including Level 4.5, SHALL provide a separate initial `HumanRespawnAnchor` and `DogRespawnAnchor` pair and a separate pair for each checkpoint. The formal route SHALL NOT use `HumanSpawn` or `DogSpawn` as player-placement inputs. Before a checkpoint activates, initial entry and failure SHALL use the active level's initial respawn pair. After either character activates a checkpoint, either character's subsequent anxiety failure or monster capture SHALL use that checkpoint's respective respawn pair.

#### Scenario: Initial entry or failure before a checkpoint
- **WHEN** the player pair enters a level or either character reaches an anxiety failure or is captured before the active level checkpoint activates
- **THEN** the shared player pair uses the active level's separate initial `HumanRespawnAnchor` and `DogRespawnAnchor`

#### Scenario: Either character commits a checkpoint
- **WHEN** either the human or dog activates an active-level checkpoint
- **THEN** subsequent failures return the human and dog to that checkpoint's respective respawn anchors

### Requirement: Ground-resolved player placement

When placing a player pair at any selected respawn anchor pair, the formal route SHALL use each anchor's XZ coordinates only and SHALL determine the actor's vertical placement by finding valid ground directly below that XZ position. Trigger volumes SHALL NOT qualify as ground. The route SHALL apply the actor's foot-placement convention to the resolved ground position.

#### Scenario: Place players at an anchor above ground
- **WHEN** an initial entry or recovery selects human and dog respawn anchors above valid ground
- **THEN** each actor is placed at its own anchor XZ with its feet on the valid ground below, regardless of either anchor's Y coordinate

#### Scenario: Missing ground below an anchor
- **WHEN** no valid ground is found below a selected respawn anchor's XZ position
- **THEN** the route refuses that placement and reports a configuration error without falling back to the anchor's Y coordinate

### Requirement: Temporary and permanent level progress recovery

On a failure, the active formal level SHALL restore its resettable level-local state while retaining the active checkpoint and all progress marked permanent for the active level session. Temporary progress includes collected keys, movable-object placement, temporary monster state, and resettable escape-stage state. Permanent progress includes completed mechanisms, opened doors, and activated checkpoints.

#### Scenario: Recover after mixed progress
- **WHEN** a failure occurs after a player collects a key, moves a resettable object, completes a mechanism, opens its resulting door, and activates a checkpoint
- **THEN** the key and resettable object restore while the completed mechanism, opened door, and checkpoint remain active

#### Scenario: Recover from monster capture
- **WHEN** a monster captures either character in an active formal level
- **THEN** both characters recover at the currently selected respawn destination and resettable monster state is restored without clearing permanent current-level progress

### Requirement: Uncommitted successor recovery

When a successor level is loaded alongside its predecessor but the successor checkpoint has not yet activated, a failure SHALL recover the player pair using the successor's current respawn destination without unloading the predecessor or discarding route state required before the successor checkpoint handoff.

#### Scenario: Fail after crossing a level boundary
- **WHEN** the player pair enters a loaded successor level and fails before activating its checkpoint
- **THEN** both characters recover at the successor's initial respawn-anchor pair while the predecessor remains available to the route lifecycle

### Requirement: Level 5 escape recovery

During the controlled Level 5 escape, a failure SHALL return both characters to the Level 5 checkpoint and restore the resettable escape-stage state, including monster state, medicine-cabinet placement, escape exit state, and controlled escape mode. Permanent final-room progress already completed outside the resettable escape stage SHALL remain complete.

#### Scenario: Fail during the controlled escape
- **WHEN** either character is captured during the Level 5 controlled escape
- **THEN** both characters respawn at the Level 5 checkpoint and the monsters, cabinet, escape exit, and controlled escape mode return to their initial escape-stage state

### Requirement: Complete-route recovery acceptance

The formal route SHALL provide validation evidence for Level 1, Level 2, Level 3, Level 4, Level 4.5, and Level 5 showing pre-checkpoint and post-checkpoint recovery, temporary-state restoration, permanent-progress retention, and a clean runtime console.

#### Scenario: Validate route recovery
- **WHEN** formal route recovery acceptance runs for every route stage
- **THEN** it reports any missing anchor, failed relocation, invalid reset classification, broken checkpoint selection, or runtime error before the route is accepted
