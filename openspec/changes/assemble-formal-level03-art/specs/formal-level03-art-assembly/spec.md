## Purpose

Define how a mixed prototype-scene selection is classified and assembled into an independently owned Formal Level 3 visual layout without duplicating prior levels or modifying the source scene.

## ADDED Requirements

### Requirement: Source-scene preservation and classification
The project SHALL preserve `Assets/Scenes/Test/superbreadman 1.unity` while classifying the current selected source objects for Formal Level 3 assembly.

#### Scenario: Mixed selection contains earlier or later content
- **WHEN** the selected source objects include Level 1, Level 2, Level 4, player, interaction, or unresolved content
- **THEN** those objects remain unchanged in the source scene and are excluded or recorded as unresolved rather than automatically assembled into Formal Level 3.

### Requirement: Confirmed Level 3 visual assembly
The project SHALL assemble confirmed Level 3 visual content into Formal Level 3 with source world position, rotation, and scale preserved.

#### Scenario: Level 3 source object is accepted
- **WHEN** a selected source visual object is confirmed by Level 3 attribution and does not duplicate existing formal level content at the same world position
- **THEN** Formal Level 3 contains the visual object at its matching world Transform.

### Requirement: Auditable migration boundary
The project SHALL record the Level 3 source identities, world-transform verification, explicit exclusions, and unresolved mixed-selection candidates in a Level 3 manifest.

#### Scenario: Future review needs to identify a migrated object
- **WHEN** a reviewer inspects a Formal Level 3 visual object or excluded source candidate
- **THEN** the Level 3 manifest provides its source identity or recorded classification reason.
