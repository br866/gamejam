## Purpose

Defines unified ground collision for the Formal route: one large authoritative ground surface at a single editable height that supersedes all legacy floor colliders without disabling them, plus tooling that audits coverage.

## ADDED Requirements

### Requirement: Authoritative ground surface
The Formal persistent scene SHALL contain exactly one `FormalGroundVolume` whose collider top defines the walkable floor height for the entire route. The volume's surface height SHALL be editable via a single serialized field without touching collider numbers.

#### Scenario: Height edit propagates
- **WHEN** the volume's height field is changed in the inspector
- **THEN** the collider's walkable top moves to the new world Y and stays a thin slab extending downward

#### Scenario: Single instance covers route
- **WHEN** any Formal level combination is loaded additively
- **THEN** exactly one active ground volume exists in `FormalPersistent` and its collider footprint covers every level's walkable area

### Requirement: Legacy colliders remain untouched
The unified volume SHALL provide walkable support by physically overlapping legacy floor colliders. No retagging, disabling, or removal of legacy colliders SHALL be performed.

#### Scenario: Legacy proxies coexist
- **WHEN** legacy floor colliders exist alongside the active volume
- **THEN** they remain enabled on their original layers, and gameplay support is provided by the authoritative volume

### Requirement: NavGround disable semantics
The disable tool SHALL disable colliders only on GameObjects assigned to the `NavGround` layer, and SHALL skip any GameObject carrying a `FormalGroundVolume`. The operation SHALL be journaled for rollback.

#### Scenario: Selective disable
- **WHEN** the tool runs with legacy floor proxies retagged to NavGround
- **THEN** those proxies are disabled while walls, furniture, and volumes on other layers remain untouched

#### Scenario: Rollback
- **WHEN** the rollback command runs after a disable pass
- **THEN** every collider disabled by the most recent pass is re-enabled

### Requirement: Coverage audit
The audit tool SHALL report, for all loaded scenes: walkable upward-facing surfaces lying outside the active ground volume's bounds, and art surfaces whose top deviates from the volume's surface height beyond tolerance.

#### Scenario: Leak detection
- **WHEN** an area's art floor exists but lies outside the volume bounds
- **THEN** the audit lists it as a hole with scene and position

#### Scenario: Misalignment detection
- **WHEN** an art surface top differs from the configured surface height by more than tolerance
- **THEN** the audit lists it as misaligned with both heights

### Requirement: Multi-volume height sync
The height-copy tool SHALL apply the first selected volume's surface height to all other selected volumes.

#### Scenario: Copy height
- **WHEN** multiple volumes are selected and the tool runs
- **THEN** all selected volumes share the first-selected surface height
