## Purpose

Provide physical presence for Formal Level 5 architecture and substantial fixed obstacles while keeping visual-only content from blocking movement.

## Requirements

### Requirement: Level 5 fixed-obstacle collision
The system SHALL provide enabled non-trigger Collider coverage for retained Level 5 walls, doors, partitions, furniture, and substantial fixed obstacles.

#### Scenario: Player approaches a Level 5 obstacle
- **WHEN** a formal player actor moves into retained Level 5 architecture or a substantial fixed prop
- **THEN** the actor is blocked by scene-owned non-trigger collision.

### Requirement: Level 5 visual-only content remains non-blocking
The system SHALL keep lights, signs, small decorations, hints, player/monster display meshes, and mechanic-only visuals non-blocking.

#### Scenario: Player crosses visual-only Level 5 content
- **WHEN** a formal player actor moves through visual-only Level 5 content
- **THEN** the visual does not physically block the actor.
