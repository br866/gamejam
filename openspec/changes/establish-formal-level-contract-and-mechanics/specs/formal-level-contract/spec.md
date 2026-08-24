## Purpose

Defines the scene-level contract and runtime lifecycle that make every formal route scene playable with the shared persistent player pair.

## ADDED Requirements

### Requirement: Formal playable level structure
Each formal playable level scene SHALL provide one level identity, separate human and dog entrance anchors, a visual content root, and a collision root. The level scene SHALL NOT contain another formal player pair or persistent game-flow controller.

#### Scenario: Loading a compliant formal level
- **WHEN** the persistent formal flow loads a compliant level scene
- **THEN** the shared human and dog are placed at that scene's separate entrance anchors and no duplicate formal players are created

#### Scenario: Scene contract is incomplete
- **WHEN** a formal playable level is validated without any required level identity, entrance anchor, visual content root, or collision root
- **THEN** validation reports the missing scene-contract element before the level is accepted

### Requirement: Formal level reset lifecycle
The formal level SHALL reset only resettable scene-local state and reposition the shared player pair at the latest activated checkpoint, or at the entrance anchors when no checkpoint is active. Progress explicitly marked permanent for the current level session SHALL remain complete after a reset.

#### Scenario: Reset before a checkpoint
- **WHEN** a formal level resets before any checkpoint has activated
- **THEN** resettable local state is restored and both players return to their respective entrance anchors

#### Scenario: Reset after a checkpoint
- **WHEN** a formal level resets after a checkpoint has activated
- **THEN** resettable local state is restored and both players return to that checkpoint's respective respawn anchors

#### Scenario: Permanent progress after a reset
- **WHEN** a player resets a formal level after completing a permanent interaction
- **THEN** the completed interaction remains complete for the active level session

### Requirement: Formal route transition lifecycle
The formal route SHALL load a successor level alongside the active level, place the shared player pair at the successor entrance, and retain the predecessor until a checkpoint in the successor activates. It SHALL unload the predecessor only after that successor checkpoint activation.

#### Scenario: Entering a successor level
- **WHEN** a player activates a formal level exit with a configured successor
- **THEN** the successor is loaded, becomes active, and both shared players are placed at its entrance anchors

#### Scenario: Protecting the route during entry
- **WHEN** the successor level has loaded but its checkpoint is not yet activated
- **THEN** the predecessor level remains loaded

#### Scenario: Committing successor progress
- **WHEN** a checkpoint in the loaded successor level activates
- **THEN** the predecessor level is unloaded and the successor remains active
