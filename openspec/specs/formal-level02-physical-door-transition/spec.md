# formal-level02-physical-door-transition Specification

## Purpose

Makes Level 2's cooperative shared-door progression physical while retaining explicit GM controls for immediate route changes.

## Requirements

### Requirement: Cooperative door preloads without teleporting players

The system SHALL open the Level 2-to-Level 3 shared door after the existing cooperative prerequisites and human E interaction, then load Level 3 without moving either player or changing the active level.

#### Scenario: Human opens the cooperative door

- **WHEN** the dog pedal and two-player safe-zone prerequisites are complete and the human presses E in the L2 door interaction area
- **THEN** the L2-to-L3 door opens, Level 3 is loaded additively, and both players remain at their current world positions

### Requirement: Physical Level 3 arrival confirms the transition

The system SHALL set Level 3 as the active formal level only after both players enter the Level 3 arrival area beyond the opened shared door.

#### Scenario: Both players cross into Level 3

- **WHEN** the preloaded Level 3 arrival area contains both players
- **THEN** the system marks Level 3 as current without repositioning either player and performs normal predecessor cleanup

#### Scenario: Only one player crosses into Level 3

- **WHEN** only one player has entered the preloaded Level 3 arrival area
- **THEN** the current formal level remains Level 2 and neither player is repositioned

### Requirement: GM direct transitions remain immediate

The system SHALL preserve keypad 2, keypad 6, and keypad 8 as GM commands that directly change levels using the existing player-placement behavior.

#### Scenario: GM advances while a physical transition is pending

- **WHEN** a tester uses a GM direct-transition command before both players confirm Level 3 arrival
- **THEN** the pending physical transition is discarded and the GM command performs its normal immediate level change
