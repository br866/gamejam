## Purpose

Provide reliable physical presence for the manually curated Formal Level 3 environment while preserving its verified dual-character entry and traversal route.

## ADDED Requirements

### Requirement: Curated Level 3 physical coverage
The system SHALL provide enabled, non-trigger 3D Collider coverage for retained Formal Level 3 static architecture, walls, doors, furniture, and other substantial fixed props that can physically obstruct a player.

#### Scenario: Player approaches a retained fixed object
- **WHEN** a formal player actor moves into retained Level 3 architecture, furniture, a closed door, or another substantial fixed prop
- **THEN** the actor is blocked by an enabled non-trigger Collider instead of passing through the visible object.

### Requirement: Visual-only Level 3 objects remain non-blocking
The system SHALL keep visual-only route hints, particle effects, and small non-obstructive decoration from physically blocking formal player actors.

#### Scenario: Player crosses visual guidance
- **WHEN** a formal player actor moves across retained Level 3 visual guidance or small decoration
- **THEN** the visual does not physically block the actor.

### Requirement: Level 3 dual-character entry remains traversable
The system SHALL preserve supported, non-overlapping human and dog entrance and checkpoint anchors and the approved baseline route in Formal Level 3 after collider coverage expands.

#### Scenario: Level 3 starts after collider expansion
- **WHEN** Formal Level 3 is loaded through the persistent formal game flow
- **THEN** exactly one human/dog actor pair is placed on supporting collision and can move along the approved baseline route without collision blockage.
