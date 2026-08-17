## Purpose

Defines reusable enemy constraints that keep movement and capture inside a configured hostile region while honoring explicit player safe zones.

## ADDED Requirements

### Requirement: Bounded enemy movement
A reusable enemy boundary configuration SHALL constrain patrol targets, chase destinations, and resulting movement to its configured hostile region.

#### Scenario: Target is outside the hostile region
- **WHEN** an enemy selects or is assigned a destination outside its hostile region
- **THEN** the enemy does not move outside the configured region

### Requirement: Safe-zone capture exclusion
A reusable enemy boundary configuration SHALL support one or more safe zones. An enemy SHALL not pursue into a safe zone and SHALL not capture a player while that player is inside a configured safe zone.

#### Scenario: Pursued player enters a safe zone
- **WHEN** a pursued player enters a configured safe zone
- **THEN** the enemy abandons or stops pursuit before entering the safe zone

#### Scenario: Player remains in a safe zone
- **WHEN** a player is inside a configured safe zone within nominal capture range
- **THEN** the enemy does not capture that player

### Requirement: Navigation boundary consistency
Enemy navigation SHALL use the same hostile-region and safe-zone constraints as enemy chase logic so pathfinding does not create a route through a safe zone.

#### Scenario: Navigation requests a safe-zone route
- **WHEN** an enemy navigation request would cross a configured safe zone
- **THEN** the generated route does not enter the safe zone
