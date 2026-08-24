## Purpose

Make pushable crates immovable scenery until a player formally engages them, and support fixed-axis cooperative variants such as the Level 5 cabinet.

## ADDED Requirements

### Requirement: Disengaged crates are immovable
A pushable crate that no player is engaged with SHALL be kinematic and unaffected by player collision, monster movement, or other physics contact.

#### Scenario: Player walks into a disengaged crate
- **WHEN** an unengaged player walks into a pushable crate
- **THEN** the crate does not move from its resting position.

#### Scenario: Monster passes near a disengaged crate
- **WHEN** the monster or another rigidbody collides with a disengaged crate
- **THEN** the crate remains stationary.

### Requirement: Engagement unlocks physics pushing
When a human player engages a crate, the crate SHALL become physics-driven and respond to input along its push axis, while walls and obstacles physically block it.

#### Scenario: Engaged push against open floor
- **WHEN** the engaged player holds movement toward the crate's push direction over flat ground
- **THEN** the crate slides along the axis at its configured speed.

#### Scenario: Engaged push against a wall
- **WHEN** the engaged player pushes the crate into a wall
- **THEN** the crate stops at wall contact without penetrating it and the crate reports a blocked state.

### Requirement: Cooperative gate for multi-pusher variants
A crate configured to require more than one pusher SHALL only move when the required number of distinct-role players are engaged simultaneously.

#### Scenario: Single player on a cooperative crate
- **WHEN** only the human is engaged with a crate requiring two pushers
- **THEN** the crate does not move regardless of input.

#### Scenario: Both characters on a cooperative crate
- **WHEN** both the human and dog are engaged with a crate requiring two pushers
- **THEN** the crate moves under the human's push input.

### Requirement: Fixed-axis variant
A crate MAY be configured with a fixed world-space push axis and optional travel limit; when configured, engagement position SHALL NOT change the axis and movement SHALL stop beyond the limit.

#### Scenario: Cabinet locked to one axis
- **WHEN** a cabinet variant with a fixed axis is pushed by an engaged cooperative pair
- **THEN** it travels only along the configured axis and never beyond its travel limit.
