## Purpose

Defines reliable entrance, checkpoint, and exit anchors for both formal player actors so each level can begin, reset, and hand off from physically valid world positions.

## ADDED Requirements

### Requirement: Explicit two-character entrance anchors
Each formal level under active development SHALL define separate human and dog entrance anchors located on supported, walkable level geometry. Loading that level SHALL place each formal player actor at its corresponding entrance anchor.

#### Scenario: Load a formal level
- **WHEN** a formal level is loaded without an active checkpoint
- **THEN** the human and dog are placed at their respective entrance anchors without starting inside an obstacle or unsupported space

### Requirement: Explicit two-character checkpoint anchors
Each formal checkpoint SHALL define separate human and dog respawn anchors. Activating a checkpoint SHALL record those checkpoint anchors as the reset destination rather than reusing the level entrance anchors.

#### Scenario: Reset after checkpoint activation
- **WHEN** a formal level resets after a checkpoint has activated
- **THEN** both player actors return to the checkpoint's configured human and dog anchors

### Requirement: Controlled exit anchor
Each formal level exit SHALL be located after a verified walkable route and SHALL not move player actors to the successor level until its configured completion condition is satisfied.

#### Scenario: Reach an eligible exit
- **WHEN** the required players reach an eligible formal level exit
- **THEN** the game loads the configured successor level and places both players at that successor's entrance anchors
