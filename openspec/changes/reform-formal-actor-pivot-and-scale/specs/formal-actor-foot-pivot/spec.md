## Purpose

Defines the foot-pivot convention for Formal route player actors (human and dog): the actor root sits at the feet, the physics capsule sits above the root, and all point-based placements (spawn, checkpoint, crate grip, rail group) coincide a dedicated attach anchor with the authored point instead of the root.

## ADDED Requirements

### Requirement: Actor root at feet
Each Formal player actor root SHALL represent the character's feet-center. The capsule collider SHALL be positioned entirely above the root such that the capsule bottom coincides with root height when standing on a surface.

#### Scenario: Standing on floor
- **WHEN** an actor stands grounded on a flat floor
- **THEN** the actor's root world Y equals the floor contact height

#### Scenario: Capsule alignment
- **WHEN** an actor is inspected while idle
- **THEN** the capsule collider spans upward from approximately the root position and does not extend below it

### Requirement: Mover attach point coincidence
Every placement that previously put the actor root at an authored point (spawn transform, checkpoint respawn anchor, crate interaction point, rail mover group point) SHALL instead place the actor so its `MoverAttachPoint` anchor coincides with that point.

#### Scenario: Spawn placement
- **WHEN** a level places actors at their spawn transforms
- **THEN** each actor's `MoverAttachPoint` lands exactly at the spawn transform position with spawn rotation applied to the actor

#### Scenario: Crate engagement
- **WHEN** an actor engages a pushable crate interaction point
- **THEN** the actor's `MoverAttachPoint` coincides with that interaction point and remains coincident while the crate moves

### Requirement: Camera focus anchor
The follow camera SHALL target the active actor's `FocusAnchor` child rather than the actor root, preserving prior focus framing; if the anchor is absent the camera SHALL fall back to the actor root.

#### Scenario: Framing preserved after pivot change
- **WHEN** gameplay starts with an actor standing where a previous-session actor stood
- **THEN** the camera focuses at the same world height as before the pivot change (anchor default local Y equals the legacy half-height)

### Requirement: Legacy offset equivalence
With `MoverAttachPoint` local Y set to +1 (legacy half-height), all placements produced by the coincidence rule SHALL produce identical actor body positions to the pre-change system for identical inputs.

#### Scenario: Regression equivalence
- **WHEN** any level scene is played before and after this change without editing its data
- **THEN** actor feet rest at the same world heights as the pre-change build

### Requirement: Body-scale unity
Each actor SHALL carry a dedicated `Body` child whose uniform local scale is the single sizing knob for both the visual model and the physics capsule. Anchors with fixed semantics (`FocusAnchor`, `MoverAttachPoint`) SHALL NOT be affected by body scaling. Scaling the body about the foot-pivot root SHALL keep the model's feet and the capsule bottom on the ground at every scale value.

#### Scenario: Single knob resizes mesh and collider
- **WHEN** an actor's Body scale is changed to any positive uniform value
- **THEN** the visual model and the capsule grow/shrink proportionally together while both remain grounded

#### Scenario: Fixed anchors unaffected
- **WHEN** the Body scale changes
- **THEN** the camera focus height and the mover attach offset keep their configured world values

#### Scenario: Body fit audit
- **WHEN** the body-fit audit tool runs
- **THEN** it reports each actor's measured model height, feet sink, capsule-to-model fit, and current Body scale without requiring manual measurement
