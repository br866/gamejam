## Purpose

Defines reusable cooperative-mechanic behavior for formal levels without depending on prototype player tags, controllers, or global reset events.

## ADDED Requirements

### Requirement: Formal trigger eligibility
Formal mechanic triggers SHALL support eligibility for either formal player, human only, dog only, or a supported physics occupant. Formal player eligibility SHALL be determined from the persistent formal player role rather than the prototype `Player` tag or prototype player controller.

#### Scenario: Human-only trigger
- **WHEN** the dog enters a human-only formal trigger
- **THEN** the trigger does not progress or execute its action

#### Scenario: Any-player trigger
- **WHEN** either the shared human or dog enters an any-player formal trigger
- **THEN** the trigger executes its configured action once according to its configured state policy

#### Scenario: Physics occupant trigger
- **WHEN** a supported resettable physics object occupies a formal occupancy trigger
- **THEN** the trigger can treat the object as a valid occupant without requiring it to be a player

### Requirement: Formal mechanism state policy
A formal mechanic SHALL declare whether its completed state is permanent for the active level session or resettable on formal level reset. Resettable mechanics SHALL restore their initial state when the owning level resets; permanent mechanics SHALL retain their completed state.

#### Scenario: Resettable mechanic
- **WHEN** a resettable formal mechanic has progressed and the owning level resets
- **THEN** its interaction state and controlled environment return to their initial state

#### Scenario: Permanent mechanic
- **WHEN** a permanent formal mechanic has completed and the owning level resets
- **THEN** its completion and controlled environment remain in the completed state

### Requirement: Formal environment actuators
Formal mechanics SHALL drive environment actuators through a common open and close contract. A blocking door actuator SHALL disable its blocking collider while open and restore its closed blocking behavior when a resettable interaction resets it.

#### Scenario: Opening a door
- **WHEN** a completed formal mechanic opens a linked blocking door
- **THEN** the door begins its configured opening visual behavior and no longer blocks player traversal

#### Scenario: Resetting a reversible door
- **WHEN** the owning level resets a resettable mechanic linked to a door
- **THEN** the door returns to its configured closed visual state and again blocks player traversal

### Requirement: Level 01 mechanic compatibility
The Level 01 key, mechanism pedal, exit door, and pushable crate SHALL use the formal mechanic foundation while preserving their observed roles: only the human collects the key and activates the pedal, the corresponding door opens, and the crate returns to its initial transform on formal reset.

#### Scenario: Level 01 human progression
- **WHEN** the human collects the Level 01 key or enters the Level 01 mechanism pedal
- **THEN** its associated door opens and the dog alone cannot complete that interaction

#### Scenario: Level 01 crate reset
- **WHEN** a player moves the Level 01 crate and the formal level resets
- **THEN** the crate has zero linear and angular velocity and is restored to its initial position and rotation
