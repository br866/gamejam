## Purpose

Complete Formal Level 3 visual coverage from nearby source-scene art while preventing known Level 2 and Level 4 content from crossing into the Level 3 assembly.

## ADDED Requirements

### Requirement: Bounded nearby visual inclusion
The project SHALL consider every source visual object within the selected Level 3 bounds expanded by eight Unity units for Formal Level 3 visual assembly.

#### Scenario: Nearby supporting art is found
- **WHEN** a visual source object lies within the expanded selection bounds and is not a Level 2/Level 4 object or a same-position duplicate
- **THEN** the object is included in the Formal Level 3 visual assembly at its source world Transform.

### Requirement: Level 2 and Level 4 exclusion
The project SHALL exclude source visual objects attributed to Level 2 or Level 4, and objects already represented by Formal Level 3 at the same world position. A similar object from another level or shared hierarchy SHALL remain eligible when its world position differs.

#### Scenario: Nearby object belongs to another level
- **WHEN** a nearby visual object has Level 2 or Level 4 attribution
- **THEN** the object is not added to Formal Level 3 and its reason is recorded.

### Requirement: Runtime-free nearby art
The project SHALL copy accepted nearby visuals without source runtime scripts, colliders, rigidbodies, audio, navigation, player, trigger, or mechanic components.

#### Scenario: Nearby interactive visual is copied
- **WHEN** an accepted nearby source visual has prototype behavior components
- **THEN** only its visual components and world Transform are present in Formal Level 3.
