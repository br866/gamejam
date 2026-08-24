## Purpose

Defines reusable puzzle triggers that distinguish human and dog occupants and support explicit cooperative occupancy without relying on raw collider counts.

## ADDED Requirements

### Requirement: Explicit character eligibility
A reusable puzzle trigger SHALL declare which player roles are eligible to activate it: human only, dog only, either role, or both roles together. Colliders that do not resolve to an eligible player role SHALL not advance the trigger state.

#### Scenario: Ineligible character enters a restricted trigger
- **WHEN** a player role not accepted by a configured trigger enters its volume
- **THEN** the trigger state remains unchanged

#### Scenario: Eligible character enters a restricted trigger
- **WHEN** a configured eligible player role enters its trigger volume
- **THEN** the trigger records that role as present

### Requirement: Cooperative occupancy
A reusable cooperative trigger SHALL activate only when every configured required player role is present simultaneously. Repeated colliders belonging to the same role SHALL not count as additional participants.

#### Scenario: One required role is present
- **WHEN** only one of two required player roles occupies a cooperative trigger
- **THEN** the trigger remains inactive

#### Scenario: Both required roles are present
- **WHEN** every configured required player role occupies a cooperative trigger
- **THEN** the trigger activates once

### Requirement: Trigger state cleanup
A reusable puzzle trigger SHALL remove departed or destroyed occupants from its recorded state and SHALL expose its current activation state to dependent progression logic.

#### Scenario: Required participant leaves
- **WHEN** a required participant leaves a continuously evaluated trigger
- **THEN** the trigger no longer reports that role as present
