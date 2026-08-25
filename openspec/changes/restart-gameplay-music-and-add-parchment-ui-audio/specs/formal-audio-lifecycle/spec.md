# formal-audio-lifecycle Specification

## Purpose

Keep persistent gameplay music aligned with level restarts and provide consistent parchment panel feedback.

## ADDED Requirements

### Requirement: Formal level restart restarts gameplay music

Every formal level reset SHALL stop the current gameplay music instance and start a new instance from its beginning after the configured stop fade.

#### Scenario: Player restarts from the pause menu

- **WHEN** the pause menu requests `ResetCurrentLevel`
- **THEN** `Stop_Gameplay_Music` is posted once
- **AND** the current MusicMode and AnxietyLevel States are reapplied
- **AND** `Play_Gameplay_Music` is posted once after the restart delay

#### Scenario: Another system resets the current level

- **WHEN** death, anxiety, a monster, or a debug shortcut requests `ResetCurrentLevel`
- **THEN** the same centralized music restart operation runs

### Requirement: Parchment panels use shared UI Events

Formal parchment-style Pause, Settings, and Tutorial/notice panels SHALL post the shared open Event when presented and the shared close Event when dismissed.

#### Scenario: Parchment panel opens

- **WHEN** an in-scope panel changes from closed to open
- **THEN** `Play_UI_Parchment_Open` is posted once

#### Scenario: Parchment panel closes

- **WHEN** an in-scope panel changes from open to closed
- **THEN** `Play_UI_Parchment_Close` is posted once

### Requirement: Death screen remains unchanged

The formal DeathScreen SHALL NOT post parchment UI Events in this change.
