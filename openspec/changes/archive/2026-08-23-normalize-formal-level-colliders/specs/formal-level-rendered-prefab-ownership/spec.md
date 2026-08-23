## Purpose

Keep formal-level rendered content reusable and consistently configured by requiring scene-visible models to be owned by Prefab assets.

## ADDED Requirements

### Requirement: Rendered formal scene objects are Prefab-owned
The system SHALL represent each rendered GameObject in a formal-level scene as a Prefab instance, except for explicitly documented engine-owned presentation objects.

#### Scenario: Formal scene content audit
- **WHEN** a formal-level scene is audited for rendered GameObjects
- **THEN** each rendered object resolves to a Prefab instance or an approved documented exception.

### Requirement: Prefab ownership preserves existing behavior
The system SHALL preserve the rendered object's transform, renderer configuration, Layer, Collider configuration, and runtime references when converting a direct formal scene object into a Prefab instance.

#### Scenario: Converted scene object loads
- **WHEN** a formal-level scene containing a converted Prefab-owned object loads
- **THEN** the object renders and behaves equivalently to the prior direct scene object.
