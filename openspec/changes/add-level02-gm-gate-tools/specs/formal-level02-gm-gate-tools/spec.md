## Purpose

Provides temporary in-game commands for diagnosing the Level 2 cooperative door and accelerating dog traversal during focused testing.

## ADDED Requirements

### Requirement: Level 2 gate status command

The runtime SHALL provide a GM command that writes the Level 2 cooperative door's current readiness and each unmet condition to the Unity Console.

#### Scenario: Gate is incomplete

- **WHEN** the GM gate-status command is used while the Level 2 cooperative door cannot open
- **THEN** the Console identifies whether the pedal, two-player safe-zone condition, E-interaction occupancy, or target door resolution is missing

#### Scenario: Gate is ready

- **WHEN** the GM gate-status command is used after every Level 2 cooperative prerequisite is satisfied
- **THEN** the Console reports that the E interaction is ready to open the L2-to-L3 door

### Requirement: Five-times dog speed GM toggle

The runtime SHALL toggle the dog's normal movement speed between its configured value and five times that value when the tester presses keypad 7.

#### Scenario: Enable accelerated dog speed

- **WHEN** the tester presses keypad 7 while the dog is available
- **THEN** the dog's movement speed becomes five times its configured normal speed and the Console reports the active multiplier

#### Scenario: Restore dog speed

- **WHEN** the tester presses keypad 7 again while acceleration is active
- **THEN** the dog's movement speed returns to its configured normal speed and the Console reports restoration
