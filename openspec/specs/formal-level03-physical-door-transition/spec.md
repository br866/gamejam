# formal-level03-physical-door-transition Specification

## Purpose

Makes Level 3's cooperative exit a physical shared-door transition into Level 4 while preserving explicit GM commands for immediate level changes.

## Requirements

### Requirement: Cooperative Level 3 exit preloads without teleporting players

The system SHALL open the Level 3-to-Level 4 shared door after the existing Level 3 cooperative exit prerequisites and preload Level 4 without moving either player or changing the active formal level.

#### Scenario: Both players complete the Level 3 exit trigger

- **WHEN** the existing Level 3 cooperative exit trigger is completed by both players
- **THEN** the L3-to-L4 door opens, Level 4 loads additively, and both players remain at their current world positions in Level 3

### Requirement: Physical Level 4 arrival confirms the transition

The system SHALL set Level 4 as the active formal level only after both players enter the existing Level 4 arrival area beyond the opened shared door.

#### Scenario: Both players cross into Level 4

- **WHEN** the preloaded Level 4 arrival area contains both players
- **THEN** the system marks Level 4 as current without repositioning either player and performs normal Level 3 predecessor cleanup

#### Scenario: Only one player crosses into Level 4

- **WHEN** only one player has entered the preloaded Level 4 arrival area
- **THEN** the current formal level remains Level 3 and neither player is repositioned

### Requirement: GM direct transitions remain immediate during Level 3 physical traversal

The system SHALL preserve existing GM direct level-change commands as immediate transitions using the established player-placement behavior.

#### Scenario: GM changes levels while the L3-to-L4 physical transition is pending

- **WHEN** a tester uses a GM direct-transition command before both players confirm Level 4 arrival
- **THEN** the pending L3-to-L4 physical transition is discarded and the GM command performs its normal immediate level change
