## Purpose

Provide a supported, non-overlapping two-character respawn point and basic corridor collision for Formal Level 4.5.

## ADDED Requirements

### Requirement: Level 4.5 respawn anchors
The system SHALL provide separate human and dog respawn anchors on supported collision in the Formal Level 4.5 corridor.

#### Scenario: Formal players respawn in Level 4.5
- **WHEN** the Level 4.5 controller places or resets the formal player pair
- **THEN** the human and dog are placed at separate supported positions without blocking overlap.

### Requirement: Level 4.5 foundational corridor collision
The system SHALL provide enabled non-trigger floor and boundary collision covering the Formal Level 4.5 corridor.

#### Scenario: Player moves within Level 4.5
- **WHEN** a formal player actor moves through the Level 4.5 corridor
- **THEN** the actor remains supported by floor collision and cannot leave through its outer corridor boundaries.
