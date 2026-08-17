## Purpose

Defines the playable Formal Level 2 route using the accepted art layout and retained prototype objects while preserving the source scene as reference material.

## ADDED Requirements

### Requirement: Dog-guided first activation
Formal Level 2 SHALL show its static footprint route only while the dog is the active character. The route SHALL lead the dog to a first pressure plate that cannot be activated by the human and that advances the Level 2 route when the dog activates it.

#### Scenario: Dog follows the first route
- **WHEN** the dog is the active character in Formal Level 2 before the first plate is complete
- **THEN** the Level 2 footprint route is visible and the dog can activate the first plate

#### Scenario: Human cannot use the first plate
- **WHEN** the human enters the first plate trigger
- **THEN** the first plate does not advance the Level 2 route

### Requirement: Monster-safe route boundary
Formal Level 2 SHALL constrain the Level 2 monster to its assigned patrol and chase region. The exit-side safe space and route boundary SHALL be unreachable by the monster, and a player inside that safe space SHALL not be captured by it.

#### Scenario: Player escapes into the safe space
- **WHEN** the monster is pursuing a player and that player enters the configured Level 2 safe space
- **THEN** the monster does not enter the safe space or capture the player there

#### Scenario: Monster remains in its assigned region
- **WHEN** the monster patrols or chases during Formal Level 2
- **THEN** its movement remains within the configured Level 2 monster region

### Requirement: Cooperative route unlock
Formal Level 2 SHALL require both player characters to activate the second pressure plate together after the first plate has advanced the route. The cooperative activation SHALL open the configured route gate and preserve the opened state until a level reset.

#### Scenario: One character reaches the second plate
- **WHEN** only one player character occupies the second plate
- **THEN** the route gate remains closed

#### Scenario: Both characters activate the second plate
- **WHEN** both player characters occupy the second plate after the first plate is complete
- **THEN** the route gate opens and remains open until the level resets

### Requirement: Level 2 completion handoff
Formal Level 2 SHALL save a checkpoint after the cooperative route unlock and SHALL hand the player pair to the configured successor level only after they reach the Level 2 exit trigger.

#### Scenario: Reset after the checkpoint
- **WHEN** a player is caught or the level resets after the Level 2 checkpoint is reached
- **THEN** both players return to the Level 2 checkpoint and the configured mechanism state resets consistently

#### Scenario: Reach the Level 2 exit
- **WHEN** both players reach the configured Level 2 exit after cooperative progression is complete
- **THEN** the successor level handoff is invoked
