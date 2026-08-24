## Purpose

Define explicit, physically supported two-character entrance, checkpoint, and provisional exit anchors for the newly assembled Formal Level 3 scene.

## ADDED Requirements

### Requirement: Supported Level 3 entrance anchors
Formal Level 3 SHALL provide separate HumanSpawn and DogSpawn transforms positioned above supporting non-trigger floor collision with no initial blocking capsule overlap.

#### Scenario: Level 3 starts through the formal flow
- **WHEN** Formal Level 3 is loaded additively through FormalPersistent
- **THEN** the human and dog are placed at separate supported entrance anchors without falling or overlapping a blocker.

### Requirement: Separate Level 3 checkpoint anchors
Formal Level 3 SHALL provide separate human and dog checkpoint respawn anchors on supported geometry.

#### Scenario: Level 3 checkpoint reset is requested
- **WHEN** the Level 3 checkpoint state is activated and the level resets
- **THEN** the human and dog return to their own configured checkpoint anchors.

### Requirement: Provisional exit anchor
Formal Level 3 SHALL provide a supported provisional exit anchor for baseline route validation without enabling final exit progression.

#### Scenario: Baseline route reaches the provisional endpoint
- **WHEN** both player capsules traverse the approved baseline route
- **THEN** they can reach the provisional endpoint without a fall or unintended blocker.
