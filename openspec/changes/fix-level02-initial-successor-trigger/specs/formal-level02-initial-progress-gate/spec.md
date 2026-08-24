## Purpose

Prevents a direct Level 2 startup from completing that level before players deliberately reach its designated successor checkpoint.

## ADDED Requirements

### Requirement: Initial Level 2 placement does not advance the route

The formal route SHALL place both players outside the Level 2 successor checkpoint trigger when `FormalLevel02` is the initial loaded level, so the Level 2 to Level 3 shared transition door remains closed at startup.

#### Scenario: Direct Level 2 startup

- **WHEN** the persistent formal route starts directly at `FormalLevel02`
- **THEN** neither initial player placement activates the successor checkpoint or requests progression to `FormalLevel03`

### Requirement: Intentional Level 2 completion remains available

The formal route SHALL retain the Level 2 successor checkpoint as a valid progression point after players intentionally enter its trigger area.

#### Scenario: Players reach the successor checkpoint during play

- **WHEN** an eligible player enters the Level 2 successor checkpoint after the level has started
- **THEN** the existing Level 2 to Level 3 progression request is made
