## Purpose

Restores normal two-character player control after the Level 4.5 pursuit segment has ended at the Level 5 checkpoint.

## ADDED Requirements

### Requirement: Level 5 restores dog switching
The system SHALL restore normal human/dog character switching when L05_Checkpoint establishes Level 5, without moving either player actor.

#### Scenario: Level 5 checkpoint is reached after pursuit
- **WHEN** L05_Checkpoint activates after the Level 4.5-to-Level 5 physical traversal
- **THEN** the player can switch control back to the dog and both actors retain their current world positions

#### Scenario: Level 4.5 pursuit remains human-controlled
- **WHEN** the player has not yet reached L05_Checkpoint during the Level 4.5 pursuit segment
- **THEN** the dog remains in forced follow mode and normal dog switching remains unavailable
