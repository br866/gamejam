## Purpose

Provide predictable physics and navigation-obstacle behavior for formal-level art while ensuring visual-only decoration has no physical presence.

## ADDED Requirements

### Requirement: Normalized art keeps renderer and collider meshes in sync
The system SHALL keep the MeshCollider of any normalized leaf-mesh node pointing at the same mesh asset its MeshFilter renders, unless an approved collision proxy is used whose world-space bounds match the renderer's world-space bounds. Collision coverage SHALL match visible geometry after scale normalization.

#### Scenario: Player collides with normalized irregular art
- **WHEN** a player actor moves against a normalized leaf-mesh model that retains its MeshCollider
- **THEN** the collider's world-space coverage matches the renderer's world-space bounds within tolerance.

#### Scenario: Normalization repoints existing colliders
- **WHEN** scale normalization replaces a node's rendered mesh with a baked copy while a MeshCollider remains on the node
- **THEN** the collider's mesh reference is updated to the baked copy (or an approved proxy) in the same pass.

### Requirement: Selection-scoped sync audit detects and repairs stale collider meshes
The system SHALL provide an editor audit that, for a user-selected hierarchy, reports every node holding both a MeshFilter and a MeshCollider whose collider mesh diverges from the renderer's current mesh or is absent, classifying intentional collision proxies separately from broken pairs. A repair action SHALL reassign only detected violations to the renderer's current mesh, recording proper overrides on nested prefab instances, and SHALL NOT modify transforms, layers, materials, or unselected objects.

#### Scenario: Audit reports violations without modifying anything
- **WHEN** the report-only audit runs on a selection containing a node whose collider mesh no longer matches its rendered mesh
- **THEN** the node is reported as broken with both mesh references and world-bound sizes listed, and no component value changes.

#### Scenario: Repair restores sync on nested prefab instances
- **WHEN** the repair action fixes a violation found inside a nested prefab instance
- **THEN** the fix persists as an instance-level `m_Mesh` override and the collider's world bounds come to match the renderer's.

#### Scenario: Intentional proxy is left untouched
- **WHEN** a node's collider uses a different mesh whose world-space bounds match the renderer within tolerance
- **THEN** the audit classifies it as a proxy and repair does not modify it.
