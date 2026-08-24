## Purpose

Provide stable two-character entry and respawn support within the selected Formal Level 5 environment.

## ADDED Requirements

### Requirement: Level 5 grounded entry anchors
The system SHALL provide separate human and dog entry anchors on enabled supporting collision in Formal Level 5.

#### Scenario: Formal Level 5 loads through persistent flow
- **WHEN** FormalPersistent loads FormalLevel05
- **THEN** one human and one dog actor are placed at separate grounded anchors without blocking overlap.

### Requirement: Level 5 foundational physical boundary
The system SHALL provide enabled non-trigger floor and outer boundary collision for the selected Formal Level 5 environment.

#### Scenario: Player moves in Level 5
- **WHEN** a formal player actor moves within the Level 5 area
- **THEN** the actor remains supported by floor collision and cannot leave through its outer boundaries.
