## Purpose

Allow developers to launch a selected formal level with the persistent player actors through the same additive game-flow path used during normal formal-level progression.

## ADDED Requirements

### Requirement: Configurable initial formal level
The system SHALL allow the persistent formal game-flow scene to select its initial formal level through serialized editor configuration.

#### Scenario: Developer selects Formal Level 2
- **WHEN** the configured initial formal level is `FormalLevel02` and the persistent formal scene enters Play Mode
- **THEN** the game flow loads Formal Level 2 additively and places the existing human and dog actors at its configured entrance anchors.

### Requirement: Single persistent player pair
The system SHALL retain a single persistent formal human/dog actor pair when a configured formal level is loaded directly.

#### Scenario: Formal Level 2 starts directly
- **WHEN** Formal Level 2 is selected as the initial formal level
- **THEN** no duplicate human or dog actor is created in the Level 2 scene.
