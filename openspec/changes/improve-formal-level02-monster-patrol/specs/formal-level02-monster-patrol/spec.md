# Spec Delta: formal-level02-monster-patrol

## Purpose

Make the FormalLevel02 monster an active threat by wiring its patrol to the existing scene waypoints while preserving the established patrol, chase, safe-zone, and reset behavior.

## ADDED Requirements

### Requirement: Level02 monster patrols using scene waypoints

The FormalLevel02 monster SHALL patrol between the existing `L02_MonsterWaypointA` and `L02_MonsterWaypointB` transforms, referenced through prefab-instance overrides on the `L02_Content` monster.

#### Scenario: Monster becomes active on level start

- **WHEN** `FormalLevel02` loads and play begins
- **THEN** the monster patrols between the two wired waypoints instead of remaining disabled

### Requirement: Waypoint references stay valid

The Level02 monster's `MonsterPatrol.waypoints` SHALL contain only non-null transforms that belong to the `FormalLevel02` scene.

#### Scenario: Regression check on waypoint wiring

- **WHEN** the formal traversal validation suite runs
- **THEN** the Level02 monster has exactly two non-null waypoints parented in `FormalLevel02`

### Requirement: Existing threat behavior is preserved

The Level02 monster SHALL keep its existing detection, chase, safe-zone, catch, and reset behavior without script changes in this pass.

#### Scenario: Player is chased inside the monster room

- **WHEN** a player enters the monster's detection range inside its room
- **THEN** the monster chases that player using the existing `MonsterPatrol` logic

#### Scenario: Safe zone suppresses capture

- **WHEN** a player stands inside `L02_CooperativeSafeZoneTrigger`
- **THEN** the monster does not catch that player and drops chase per the existing safe-zone rules

#### Scenario: Restart restores patrol

- **WHEN** the player restarts Level02 with keypad 5
- **THEN** the monster returns to its patrol state at its initial position
