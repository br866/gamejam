## Purpose

Provide predictable physics and navigation-obstacle behavior for formal-level art while ensuring visual-only decoration has no physical presence.

## ADDED Requirements

### Requirement: Formal-level collider responsibilities follow Layer contracts
The system SHALL reserve `NavStatic` for enabled non-trigger Collider coverage on fixed obstacles and `NavDynamic` for enabled non-trigger BoxCollider coverage on door leaves whose route-blocking state can change. Only these Layers SHALL contribute colliders to formal-level A* obstacle detection.

#### Scenario: Navigation scans a formal level
- **WHEN** the formal-level navigation graph scans its obstacle colliders
- **THEN** it treats enabled colliders on `NavStatic` and `NavDynamic` as obstacles and excludes colliders on all other Layers.

### Requirement: Visual-only formal props have no physical presence
The system SHALL remove all Collider components from visual-only small formal props that do not obstruct traversal, interaction, or route solving.

#### Scenario: Player crosses visual-only decoration
- **WHEN** a formal player actor moves through a visual-only small prop
- **THEN** the prop does not block movement, generate physics contacts, trigger events, camera obstruction hits, or navigation obstacles.

### Requirement: Simple blocking models use box collision
The system SHALL use a BoxCollider aligned to the source model bounds for each simple rectangular formal-level model that must physically block actors.

#### Scenario: Player approaches a simple fixed obstacle
- **WHEN** a formal player actor approaches a wall, cabinet, locker, bed, desk, or other simple retained obstacle
- **THEN** a bounds-aligned non-trigger BoxCollider blocks the actor without requiring a MeshCollider.

### Requirement: Complex blocking models are explicitly reviewed
The system SHALL not replace a complex, irregular, or strongly concave blocking model with one oversized automatic BoxCollider when that approximation materially changes the traversable space.

#### Scenario: Irregular model requires collision
- **WHEN** a pipe, wheelchair, articulated door, or another irregular formal-level model must block player traversal
- **THEN** the asset receives a reviewed compound BoxCollider arrangement or remains excluded from automatic conversion until its physical role is decided.

### Requirement: Approved complex static obstacles retain mesh collision
The system SHALL retain the existing MeshCollider on an approved complex static obstacle when its mesh shape is required to preserve the intended traversable space. The obstacle SHALL use `NavStatic` so it remains an A* navigation obstacle.

#### Scenario: Player approaches the broken wall
- **WHEN** a player or navigation scan encounters the retained formal-level `broken_wall` obstacle
- **THEN** its enabled non-trigger MeshCollider remains in place on `NavStatic` and preserves the wall's intended collision shape.

### Requirement: Door frames have no collision
The system SHALL remove Collider components from formal-level door-frame and door-jamb models. The corresponding door leaf SHALL own its dynamic physical blocking collider.

#### Scenario: Player approaches a formal doorway
- **WHEN** a player approaches a doorway while its door leaf is open or absent
- **THEN** the door frame does not create invisible physical blocking and only the `NavDynamic` door leaf can block the opening.

### Requirement: Mechanism detection colliders are trigger-only and Prefab-owned
The system SHALL represent formal mechanism detection volumes, including pedals, pressure plates, buttons, checkpoints, and exit volumes, with an explicit Prefab-owned Collider whose `isTrigger` value is true. The visual model SHALL not be the source of physical blocking for these mechanisms.

#### Scenario: Player crosses a mechanism device
- **WHEN** a player actor enters a pedal, pressure plate, button, checkpoint, or exit detection volume
- **THEN** the mechanism receives its trigger callback without physically stopping the actor.

#### Scenario: A mechanism needs physical blocking
- **WHEN** a mechanism also represents a solid object that intentionally blocks traversal
- **THEN** its non-trigger blocking Collider is separate from the trigger detection Collider and the object is classified as a retained physical obstacle rather than a trigger-only mechanism.
