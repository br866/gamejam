## Purpose

Normalize extreme transform scales on static art meshes through a journal-logged, per-scene reversible batch process that preserves world appearance and collider behavior.

## ADDED Requirements

### Requirement: Normalization preserves world-space appearance
Baking a node's scale into its mesh SHALL keep the mesh's world-space bounds and silhouette unchanged; only the transform scale value changes.

#### Scenario: Crate visual normalized
- **WHEN** a leaf mesh with extreme scale is normalized
- **THEN** the node's local scale becomes 1 and its rendered world bounds match the pre-normalization bounds within a small tolerance.

### Requirement: Colliders are compensated
Nodes carrying Box, Sphere, Capsule, or Mesh colliders SHALL have those colliders adjusted during normalization so contact volumes remain equivalent in world space.

#### Scenario: Box collider stays aligned after normalization
- **WHEN** a scaled leaf node with a box collider is normalized
- **THEN** the collider's world-space volume before and after normalization matches.

### Requirement: Batch process journals every change
The batch normalizer SHALL record, for every node it touches: scene name, node path, original scale, original mesh reference, and the baked asset path it created, written to a journal file before the scene is saved.

#### Scenario: Journal enables rollback
- **WHEN** a normalization batch completes on a scene
- **THEN** a journal file exists listing each changed node with enough information to restore the original scale and mesh reference.

### Requirement: Per-scene rollback
The toolchain SHALL provide a rollback command that restores all nodes listed in a scene's most recent normalization journal to their recorded original state, including deleting orphaned baked assets.

#### Scenario: Rollback after a bad batch
- **WHEN** rollback is executed for a scene that was just normalized
- **THEN** every touched node regains its original scale and mesh reference and the batch's baked assets are removed.

### Requirement: Unsafe nodes are skipped
The batch normalizer SHALL NOT modify nodes that have children, skinned meshes, particle systems, or trail renderers, and SHALL list skipped nodes in the journal.

#### Scenario: Skinned character untouched by batch
- **WHEN** the batch runs over a scene containing skinned character models
- **THEN** those characters' transforms and rigs are unchanged and appear in the journal as skipped.
