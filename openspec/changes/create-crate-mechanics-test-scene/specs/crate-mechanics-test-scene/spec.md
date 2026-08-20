## Purpose

Provides a small, repeatable Unity environment for validating formal wooden-crate movement without dependencies on the production level route.

## ADDED Requirements

### Requirement: Standalone crate interaction environment
The project SHALL provide a standalone test scene containing the formal player actors, one formal movable wooden crate, and a walkable ground surface.

#### Scenario: Open the crate test scene
- **WHEN** a developer opens the crate mechanics test scene
- **THEN** the human actor, dog actor, wooden crate, and ground surface are present without loading a formal route scene

### Requirement: Runnable formal crate interaction
The test scene SHALL configure the reused actors and crate so the existing player controls and crate interaction can be exercised in Play Mode.

#### Scenario: Exercise crate movement
- **WHEN** the scene enters Play Mode and the human approaches the crate interaction point
- **THEN** the developer can use the existing crate engagement and movement controls on the crate

### Requirement: Continuous crate movement
An engaged human SHALL be able to move the wooden crate continuously along its selected movement axis without a fixed scripted travel limit.

#### Scenario: Move beyond the prior travel range
- **WHEN** an engaged human continues moving the crate in one direction
- **THEN** the crate continues moving along the selected axis beyond its previous fixed range

### Requirement: Stable backward crate movement
The crate interaction SHALL use the attached idle state during backward movement rather than play the human `Pull` animation.

#### Scenario: Move the crate backward
- **WHEN** an engaged human moves the crate in the backward direction
- **THEN** the crate moves while the human remains at the interaction point in the attached idle state

### Requirement: Isolated formal-route content
The test scene SHALL not be included in the enabled build route and SHALL not alter formal level scene content.

#### Scenario: Inspect build scenes
- **WHEN** the build scene list is inspected after the test scene is created
- **THEN** the crate test scene is absent from the enabled formal route
