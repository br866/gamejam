## Purpose

Provide physical presence for retained Formal Level 4.5 corridor architecture and fixed obstacles while preserving respawn and unobstructed movement from entry anchors.

## ADDED Requirements

### Requirement: Level 4.5 fixed-obstacle collision
The system SHALL provide enabled non-trigger Collider coverage for retained Level 4.5 walls, doors, partitions, furniture, and substantial fixed obstacles.

#### Scenario: Player approaches a Level 4.5 obstacle
- **WHEN** a formal player actor moves into retained Level 4.5 architecture or a substantial fixed prop
- **THEN** the actor is blocked by scene-owned non-trigger collision.

### Requirement: Level 4.5 visual-only content remains non-blocking
The system SHALL keep lights, signs, small decorative props, visual hints, and mechanic-only display objects non-blocking.

#### Scenario: Player crosses visual-only content
- **WHEN** a formal player actor moves through Level 4.5 visual-only content
- **THEN** the visual does not physically block the actor.

### Requirement: Level 4.5 respawn remains valid
The system SHALL retain supported, non-overlapping Level 4.5 human and dog respawn anchors after collider coverage expands.

#### Scenario: Players reset in Level 4.5
- **WHEN** formal players are placed at Level 4.5 respawn anchors
- **THEN** both actors are grounded without initial blocking overlap.
