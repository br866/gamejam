## Purpose

Provide practical collision for retained Formal Level 4 static environment objects while keeping visual-only and cross-level content from blocking player movement.

## ADDED Requirements

### Requirement: Level 4 fixed-obstacle collision
The system SHALL provide enabled non-trigger Collider coverage for retained Formal Level 4 architecture, gates, doors, furniture, and other substantial fixed obstacles.

#### Scenario: Player approaches a fixed Level 4 object
- **WHEN** a formal player actor moves into retained Level 4 architecture, a gate, furniture, or a substantial fixed prop
- **THEN** the actor is blocked by scene-owned non-trigger collision.

### Requirement: Level 4 visual-only content remains non-blocking
The system SHALL keep route hints, small decoration, player and monster display meshes, prototype pad/plate visuals, and known cross-level visual objects from physically blocking formal player actors.

#### Scenario: Player crosses non-blocking visual content
- **WHEN** a formal player actor moves through a Level 4 visual-only object
- **THEN** the object does not physically block the actor.

### Requirement: Level 4 entry remains valid
The system SHALL retain grounded, non-overlapping human and dog Level 4 entry anchors after broad collider coverage is added.

#### Scenario: Formal Level 4 loads after collider expansion
- **WHEN** FormalPersistent loads FormalLevel04
- **THEN** the human and dog pair spawn on supporting collision without initial blocking overlap.
