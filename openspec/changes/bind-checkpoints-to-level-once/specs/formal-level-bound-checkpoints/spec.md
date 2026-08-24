## Purpose

Ensures formal checkpoints are registered only with their owning level and cannot bypass that level's intentional cooperative transition gate.

## ADDED Requirements

### Requirement: Each checkpoint has one owning level registration

The formal route SHALL associate each formal checkpoint with exactly one formal level and SHALL register an associated checkpoint at most once during that level's loaded lifetime.

#### Scenario: Level 2 loads directly

- **WHEN** the persistent formal route starts directly at `FormalLevel02`
- **THEN** the Level 2 checkpoint is registered only for `FormalLevel02`, and any initial player overlap does not create another registration

#### Scenario: Checkpoint is encountered repeatedly

- **WHEN** players enter the same checkpoint trigger more than once while its owning level remains loaded
- **THEN** the checkpoint retains its single registration for that level

### Requirement: Checkpoints do not advance the route

The formal route SHALL treat a checkpoint as level-local checkpoint state and SHALL NOT request route advancement or open a transition door because a checkpoint is registered or entered.

#### Scenario: Players spawn in the Level 2 successor checkpoint

- **WHEN** direct Level 2 startup places one or both players in the `SuccessorCheckpoint` trigger
- **THEN** the L2-to-L3 transition door remains closed and no route-advance request is made by that checkpoint

### Requirement: Level 2 door uses the cooperative interaction gate

The L2-to-L3 shared transition door SHALL open and begin the normal L2-to-L3 progression only through the Level 2 cooperative sequence: the pedal is pressed, both players occupy `L02_CooperativeSafeZoneTrigger`, and the player uses E in that safe zone.

#### Scenario: Cooperative gate completes

- **WHEN** the pedal requirement is satisfied, both players are inside the cooperative safe zone, and the player uses E there
- **THEN** the L2-to-L3 shared transition door opens and the normal Level 3 transition begins

#### Scenario: Checkpoint alone is entered

- **WHEN** a player enters a Level 2 checkpoint without completing the cooperative interaction gate
- **THEN** the L2-to-L3 shared transition door remains closed

#### Scenario: Cooperative condition completes without E

- **WHEN** the pedal requirement is satisfied and both players enter the cooperative safe zone without using E
- **THEN** the L2-to-L3 shared transition door remains closed
