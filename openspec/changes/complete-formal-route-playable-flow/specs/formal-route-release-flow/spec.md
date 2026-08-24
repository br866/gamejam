## Purpose

Defines the player-facing formal route from startup through final completion, including safe additive transitions and release acceptance independent of debug navigation.

## ADDED Requirements

### Requirement: Formal route startup
The shipped build SHALL present a player-facing startup path that enters the formal persistent route and loads Formal Level 1. The legacy prototype route SHALL NOT be required to begin or complete the formal route.

#### Scenario: Start a new formal route
- **WHEN** a player starts a new game from the build's first player-facing scene
- **THEN** the persistent formal systems initialize once and the player pair enters Formal Level 1 at its configured entrance anchors

### Requirement: Checkpoint-committed additive handoff
The formal route SHALL load a successor alongside its predecessor and retain the predecessor until a checkpoint in the successor activates. After that checkpoint activates, the route SHALL unload the predecessor and shared art no longer referenced by the retained route scenes without destroying persistent systems.

#### Scenario: Death before successor checkpoint
- **WHEN** players enter a successor level and reset before its checkpoint activates
- **THEN** the predecessor remains loaded and the route retains the state needed to recover according to the active transition lifecycle

#### Scenario: Commit successor checkpoint
- **WHEN** either player activates the successor checkpoint
- **THEN** the successor becomes the retained level, the predecessor unloads safely, and no unused shared art remains loaded

### Requirement: Complete formal route
The formal route SHALL progress through Level 1, Level 2, Level 3, Level 4, Level 4.5, and Level 5 in order through configured gameplay exits. Reaching the final Level 5 completion condition SHALL present an explicit player-facing completion state with a supported restart or return path.

#### Scenario: Complete the route without debug commands
- **WHEN** a player completes each formal level's configured gameplay requirements and crosses its exit
- **THEN** the next ordered formal level loads until Level 5 presents the final completion state

#### Scenario: Reach final completion
- **WHEN** players complete the Level 5 final door condition
- **THEN** the game presents final completion rather than attempting to load an unconfigured successor scene

### Requirement: Formal route release verification
The project SHALL provide automated validation and documented Play Mode acceptance covering build-scene availability, unique route identity, required scene contract elements, checkpoint-committed handoff, and the complete formal route.

#### Scenario: Validate route before release
- **WHEN** formal route release validation runs
- **THEN** it reports every invalid scene, catalog entry, build reference, lifecycle failure, or incomplete route handoff before release acceptance
