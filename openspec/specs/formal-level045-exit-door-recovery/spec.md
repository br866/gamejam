# formal-level045-exit-door-recovery Specification

## Purpose

Ensures that a failed Level 4.5 escape attempt restores its exit-door challenge rather than retaining an opened route to Level 5.

## Requirements

### Requirement: Level 4.5 or Level 5 recovery closes their shared exit door
When recovery is requested while either Level 4.5 or Level 5 is the active formal level, the system SHALL close the shared Level 4.5-to-Level 5 exit door before resuming player control.

#### Scenario: Player dies after opening the exit door in Level 4.5
- **WHEN** the Level 4.5 exit door has been opened and a death recovery is requested before the route advances to Level 5
- **THEN** the player respawns in Level 4.5 with the exit door closed

### Requirement: Level 4.5 exit interaction resets with recovery
When Level 4.5 recovery closes its Level 5 exit door, the system SHALL restore the exit interaction so that the player can open the door again after satisfying its normal conditions.

#### Scenario: Player retries the Level 4.5 escape
- **WHEN** recovery has closed a previously opened Level 4.5 exit door
- **THEN** the player can satisfy the normal exit conditions and open the door again

### Requirement: Level 5 recovery closes the shared exit door
The system SHALL close the Level 4.5-to-Level 5 exit door when recovery is requested after the Level 5 handoff has completed.

#### Scenario: Player dies after arrival in Level 5
- **WHEN** a player dies after the Level 5 handoff has completed
- **THEN** Level 5 recovery closes the shared Level 4.5-to-Level 5 exit door
