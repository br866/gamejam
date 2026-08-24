# dog-wwise-footsteps Specification

## Purpose

Route formal Dog footsteps through the authored Wwise Event while retaining the Human cadence and Wwise-authored randomization.

## ADDED Requirements

### Requirement: Dog movement triggers the Dog Wwise Event

The formal player actor SHALL post the configured Dog footstep Event on the formal Dog GameObject when the grounded Dog reaches its Dog-specific distance threshold.

#### Scenario: Grounded Dog reaches the next step interval

- **WHEN** the Dog is grounded and moves at least `dogWalkFootstepDistance` from the previous step position
- **THEN** `Play_Footstep_Dog` is posted once on the Dog GameObject

#### Scenario: Dog is airborne or below the interval

- **WHEN** the Dog is not grounded or has not moved the configured distance
- **THEN** no Dog footstep Event is posted

### Requirement: Character Events remain role-specific

The Human actor SHALL post only its Human Event, and the Dog actor SHALL post only its Dog Event.

#### Scenario: Both formal actors move

- **WHEN** either formal actor reaches its own step interval
- **THEN** the Event selected by that actor's role is posted

### Requirement: Randomization remains authored in Wwise

Unity SHALL post one footstep Event per trigger and SHALL NOT select or randomize individual Dog samples.

#### Scenario: A Dog step is triggered

- **WHEN** Unity posts `Play_Footstep_Dog`
- **THEN** the Wwise Random Container chooses the sample

### Requirement: Missing setup is diagnosable

The controller SHALL emit at most one warning per component instance when the role-specific Event reference is missing.

#### Scenario: A step occurs with no valid role Event

- **WHEN** a footstep threshold is reached without a valid Event reference
- **THEN** playback is skipped and one warning identifies the affected role
