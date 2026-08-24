# formal-level04-through-level05-physical-door-transition Specification

## Purpose

Ensures the final two shared-door boundaries use the same physical traversal model as earlier formal levels while preserving Level 4.5's retained predecessor pursuit.

## Requirements

### Requirement: Level 4 exit begins a physical transition to Level 4.5
The system SHALL, after the existing Level 4 exit requirements are satisfied, open the Level 4-to-Level 4.5 shared door and preload Level 4.5 without moving either player or changing the current formal level.

#### Scenario: Level 4 cooperative exit completes
- **WHEN** both players complete the configured Level 4 exit condition
- **THEN** the L4-to-L4.5 shared door opens, Level 4.5 loads additively, and both players retain their current world positions in Level 4

### Requirement: Physical Level 4.5 arrival commits without unloading Level 4
The system SHALL mark Level 4.5 as current only after both players enter its arrival area beyond the shared door, without repositioning either player, and SHALL retain Level 4 for the configured Level 4.5 pursuit behavior.

#### Scenario: Both players enter Level 4.5
- **WHEN** the preloaded Level 4.5 arrival area contains both players
- **THEN** Level 4.5 becomes the current formal level without player repositioning and Level 4 remains loaded for pursuit

#### Scenario: Only one player enters Level 4.5
- **WHEN** only one player has entered the preloaded Level 4.5 arrival area
- **THEN** the current formal level remains Level 4 and neither player is repositioned

### Requirement: Level 4.5 exit begins a physical transition to Level 5
The system SHALL route every configured Level 4.5 exit completion path through physical shared-door traversal, including both cooperative-actuator and crate-door completion paths.

#### Scenario: Level 4.5 cooperative exit completes
- **WHEN** the configured Level 4.5 cooperative exit condition is completed
- **THEN** the L4.5-to-L5 shared door opens, Level 5 loads additively, and both players retain their current world positions in Level 4.5

#### Scenario: Level 4.5 crate-door exit completes
- **WHEN** the configured Level 4.5 crate-door exit condition is completed
- **THEN** the L4.5-to-L5 shared door opens and Level 5 preloads without direct player transfer

### Requirement: L05 checkpoint commits Level 5 recovery and releases retained Level 4
The system SHALL mark Level 5 as the current recovery level when L05_Checkpoint activates after the L4.5-to-L5 physical traversal begins, without repositioning either player. It SHALL unload the retained Level 4 scene at that point, retain Level 4.5, and use the activated L05 checkpoint for later recovery.

#### Scenario: L05 checkpoint activates during physical arrival
- **WHEN** L05_Checkpoint is activated while Level 5 is the pending physical successor
- **THEN** Level 5 becomes the recovery level without moving players, the retained Level 4 scene unloads with its monsters, and Level 4.5 remains loaded

### Requirement: Physical Level 5 arrival seal confirms full traversal
The system SHALL allow both players entering its existing arrival area beyond the preloaded L4.5-to-L5 shared door to complete the physical arrival seal without repositioning players or unloading Level 4.5. L05_Checkpoint has already established Level 5 as the recovery level.

#### Scenario: Both players enter Level 5
- **WHEN** the preloaded Level 5 arrival area contains both players
- **THEN** neither player is repositioned, Level 5 remains the current recovery level, and Level 4.5 remains loaded

### Requirement: Level 4.5 recovery preserves the retained Level 4 pursuit scene
The system SHALL, when recovering Level 4.5 before L05_Checkpoint releases Level 4, preserve the retained Level 4 scene and its monsters. If that retained scene is unexpectedly not loaded, the system SHALL reload it before restarting the Level 4.5 pursuit sequence.

#### Scenario: Player dies in Level 4.5 pursuit
- **WHEN** the player dies or restarts while Level 4.5 is current and Level 4 is retained
- **THEN** the player recovers in Level 4.5, Level 4 remains loaded, and the pursuit sequence restarts

### Requirement: GM direct transitions remain independent
The system SHALL preserve keypad GM direct level transitions as immediate jumps using the established player-placement behavior, even when a physical shared-door transition is pending.

#### Scenario: GM transition interrupts a pending physical traversal
- **WHEN** a tester invokes keypad 2, 6, or 8 while either final shared-door transition is pending
- **THEN** the pending physical transition is discarded and the requested GM transition performs its normal immediate placement
