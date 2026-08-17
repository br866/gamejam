## Purpose

Provide reliable physical presence for migrated formal-level visuals while preserving the movement routes and non-blocking visual guidance required for player testing.

## ADDED Requirements

### Requirement: Migrated Level 2 physical coverage
The system SHALL provide enabled, non-trigger 3D Collider coverage for migrated Level 2 architecture, doors, monsters, furniture, and other substantial fixed props that can physically obstruct a player.

#### Scenario: Player approaches a migrated fixed object
- **WHEN** a formal player actor moves into migrated architecture, furniture, a closed door, or a monster visual
- **THEN** the actor is blocked by an enabled non-trigger Collider instead of passing through the visible object.

### Requirement: Visual-only objects remain non-blocking
The system SHALL keep visual-only route hints, particle effects, and small non-obstructive decoration from physically blocking formal player actors.

#### Scenario: Player crosses a footprint marker
- **WHEN** a formal player actor moves across a migrated footprint marker
- **THEN** the marker does not physically block the actor.

### Requirement: Existing traversal anchors remain valid
The system SHALL preserve a supported, non-overlapping human and dog entrance and checkpoint anchor in Formal Level 2 after collider coverage is expanded.

#### Scenario: Level 2 starts after collider expansion
- **WHEN** Formal Level 2 is loaded through the formal game flow
- **THEN** both formal player actors are placed on supporting collision without initial blocking overlap.
