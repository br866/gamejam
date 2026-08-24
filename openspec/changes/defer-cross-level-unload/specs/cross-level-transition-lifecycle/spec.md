## Purpose

Keep adjacent formal levels connected by a shared door while delaying predecessor cleanup until the successor level is restarted.

## ADDED Requirements

### Requirement: Key pickup opens the shared transition before loading the successor
When the Level01 human key is collected by the Human actor, the system SHALL open the shared Level01-to-Level02 door before loading `FormalLevel02`.

#### Scenario: Human collects the Level01 key
- **WHEN** the Human actor enters the key trigger
- **THEN** the shared Level01-to-Level02 door is open and the successor scene `FormalLevel02` begins loading.

### Requirement: The predecessor remains loaded after successor load
After `FormalLevel02` is loaded from the Level01 transition, the system SHALL keep `FormalLevel01` loaded until Level02 restart.

#### Scenario: Level02 has loaded
- **WHEN** the successor scene becomes active
- **THEN** `FormalLevel01` remains loaded and the shared Level01-to-Level02 door remains open.

### Requirement: Successor checkpoint confirms arrival without cleanup
When the player reaches the Level02 successor checkpoint, the system SHALL record arrival confirmation without unloading `FormalLevel01` or closing the shared transition door.

#### Scenario: Player reaches the Level02 save point
- **WHEN** the Level02 checkpoint trigger accepts the players
- **THEN** the predecessor remains loaded and the shared door remains open.

### Requirement: Restart closes the transition and unloads the predecessor
When the player restarts Level02 while `FormalLevel01` is pending unload, the system SHALL close the shared Level01-to-Level02 door, unload `FormalLevel01`, clear the pending predecessor state, and reset Level02.

#### Scenario: Player restarts Level02
- **WHEN** Level02 restart is requested after the Level01 transition
- **THEN** the shared door is closed, `FormalLevel01` is unloaded, and Level02 reset state is applied.
