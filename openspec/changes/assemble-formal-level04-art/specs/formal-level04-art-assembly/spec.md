## Purpose

Make the explicit Level 4 prototype art available in a standalone formal scene without carrying prototype runtime behavior or adjacent Level 4.5 content.

## ADDED Requirements

### Requirement: Level 4 visual assembly
The system SHALL assemble enabled Renderer-bearing objects from explicit `floor/Level4`, `Item/Level4`, and `Static Scene/Level4` source hierarchy into FormalLevel04 while preserving their effective world transforms.

#### Scenario: Formal Level 4 is opened
- **WHEN** FormalLevel04 is loaded
- **THEN** the copied Level 4 environment renders in its source world placement.

### Requirement: Level 4.5 separation
The system SHALL exclude objects explicitly under the prototype `Level4.5` hierarchy from FormalLevel04 visual assembly.

#### Scenario: Level 4 scene loads
- **WHEN** FormalLevel04 is loaded
- **THEN** the separate Level 4.5 corridor is not included as Level 4 visual content.

### Requirement: Visual-only copied content
The system SHALL remove prototype scripts, colliders, rigidbodies, audio, navigation, and gameplay behavior from the copied Level 4 art content.

#### Scenario: Formal Level 4 content is inspected
- **WHEN** a copied visual object is inspected
- **THEN** it retains only rendering-related components required to display the source art.
