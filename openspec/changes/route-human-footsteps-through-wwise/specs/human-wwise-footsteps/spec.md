# human-wwise-footsteps Specification

## Purpose

Route formal Human player footsteps through the authored Wwise event while preserving formal movement and leaving other character audio backends unchanged.

## ADDED Requirements

### Requirement: Existing cadence triggers the Human Wwise event

The formal player actor SHALL post the configured Human Wwise footstep Event on the formal Human GameObject when the grounded distance threshold is reached.

The walking and sprinting thresholds SHALL represent one half of their respective animation cycle distance at the configured formal movement speeds, so two footsteps occur per animation loop without producing footsteps when movement is blocked.

#### Scenario: Grounded Human reaches the next step interval

- **WHEN** the Human is grounded and moves at least `footstepDistance` from the previous step position
- **THEN** the configured Wwise Event is posted once on the Human GameObject

#### Scenario: Human is airborne or below the interval

- **WHEN** the Human is not grounded or has not moved the configured distance
- **THEN** no Human footstep Event is posted

### Requirement: Missing setup is diagnosable

The player controller SHALL emit at most one warning per component instance when the Human Wwise Event reference is missing.

#### Scenario: Human reaches a step with no Event assigned

- **WHEN** a Human footstep trigger occurs without a valid Wwise Event reference
- **THEN** playback is skipped and one warning identifies the missing assignment

### Requirement: Other character paths remain unchanged

The Human migration SHALL NOT add playback to the formal Dog actor or alter the legacy `PlayerController` footstep behavior.

#### Scenario: Dog reaches the next step interval

- **WHEN** the formal actor role is Dog
- **THEN** the formal Human Wwise Event is not posted
