# Death Cause Wwise Audio

## Requirements

### Requirement: Death playback selects the authored cause

The formal death flow SHALL set the Wwise `COD` State before posting death
music. Anxiety deaths SHALL select `Anxiety`; monster-caught deaths SHALL select
`Eliminated`.

#### Scenario: anxiety reaches its fatal threshold

- **WHEN** `FormalDeathScreen` is triggered with `DeathCause.Anxiety`
- **THEN** gameplay music is stopped
- **AND** `COD/Anxiety` is selected
- **AND** the common death stinger and death music are posted once

#### Scenario: a monster catches the player

- **WHEN** `FormalDeathScreen` is triggered with `DeathCause.Caught`
- **THEN** gameplay music is stopped
- **AND** `COD/Eliminated` is selected
- **AND** the common death stinger and death music are posted once

### Requirement: Death audio does not leak into recovery

Death music and any still-playing death stinger SHALL stop before the existing
restart or title navigation continues.

#### Scenario: restart from the death screen

- **WHEN** the player selects Restart
- **THEN** death music and its active stinger stop
- **AND** the existing reset path starts gameplay music from the beginning

#### Scenario: return to title from the death screen

- **WHEN** the player selects Return to Main Menu
- **THEN** death music and its active stinger stop before scene loading

### Requirement: Missing authoring references fail visibly but safely

The runtime SHALL not throw when an Event reference is missing and SHALL emit a
single diagnostic warning for each missing Event.

