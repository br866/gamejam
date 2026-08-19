## Purpose

Defines reusable formal art model prefabs so one source model can be updated consistently wherever its verified identity is used across the formal route.

## ADDED Requirements

### Requirement: Modeled-object extraction coverage
Every formal GameObject that carries a `MeshFilter`, `SkinnedMeshRenderer`, or another renderable model component SHALL be extracted into an independent prefab unless it already has an independent prefab source. Objects without a model component SHALL NOT be extracted.

#### Scenario: Object without a model
- **WHEN** a formal GameObject has no renderable model component
- **THEN** it remains scene-owned and no model prefab is created for it

#### Scenario: Existing model prefab
- **WHEN** a formal modeled object already originates from an independent prefab
- **THEN** extraction reuses that prefab and does not create a duplicate

### Requirement: Scene replacement preservation
Replacing a modeled object with its extracted prefab SHALL preserve its world position, rotation, scale, material appearance, active state, and intended static collision behavior. Scene-specific gameplay components and references SHALL remain owned by the level scene and SHALL NOT be absorbed into a generic art prefab.

#### Scenario: Replace a modeled object
- **WHEN** a modeled object is replaced by its extracted prefab in a formal level
- **THEN** it remains visually and physically aligned with its original placement and retains any level-owned gameplay components outside the prefab

### Requirement: Protected non-model ownership
Collision roots, gameplay triggers, checkpoints, monsters, scene-specific door actuators, and cross-level shared-art layout ownership SHALL NOT be extracted as generic model prefabs. A model carried by a large architectural object MAY be extracted, but its scene-owned collision and gameplay configuration SHALL remain outside the prefab.

#### Scenario: Existing formal door prefab
- **WHEN** a formal door object already has an independent formal door prefab
- **THEN** the extraction process does not create another prefab for that door

### Requirement: Extraction validation
The project SHALL validate every extracted shared model prefab and each replacement instance for identity provenance, transform preservation, and broken references before accepting the extraction.

#### Scenario: Ambiguous model candidate
- **WHEN** an audit cannot establish a model's identity or safe ownership boundary
- **THEN** validation reports it as unresolved and the model remains unchanged
