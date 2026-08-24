## Purpose

Defines reusable progression state that gates route objects behind explicit prerequisites and returns dependent state to a deterministic configuration on reset.

## ADDED Requirements

### Requirement: Prerequisite-gated progression
A reusable route progression step SHALL advance only after all configured prerequisite steps are complete. A dependent gate, checkpoint, or exit SHALL remain unavailable until its prerequisite step is complete.

#### Scenario: Prerequisite is incomplete
- **WHEN** a player reaches a dependent route object before its prerequisite is complete
- **THEN** the route object remains unavailable and does not advance progression

#### Scenario: Prerequisite completes
- **WHEN** every prerequisite for a route object becomes complete
- **THEN** the configured dependent route object becomes available

### Requirement: Persistent route unlock until reset
A completed progression step SHALL preserve its unlocked state for the active level attempt even when participating players leave the trigger, unless the step is explicitly configured as continuous.

#### Scenario: Player leaves a completed non-continuous step
- **WHEN** the occupant leaves a completed non-continuous progression trigger
- **THEN** the completed route state remains available

### Requirement: Deterministic reset
A level reset SHALL restore every registered reusable progression step, dependent gate, and availability state to its configured initial state without retaining stale collider occupancy or completion state.

#### Scenario: Reset after route unlock
- **WHEN** the level resets after one or more progression steps complete
- **THEN** each registered step and dependent route object returns to its configured initial state
