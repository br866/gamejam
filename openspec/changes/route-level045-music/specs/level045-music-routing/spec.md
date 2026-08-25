# Level 4.5 Music Routing

## Requirements

### Requirement: the long corridor owns the special track

The formal audio system SHALL play `Play_Level5_Music` only while
`FormalGameFlowController.CurrentLevelScene` is `FormalLevel045`.

#### Scenario: the corridor scene is only preloaded

- **WHEN** `FormalLevel045` is loaded additively but the current route level is still Level 4
- **THEN** normal gameplay music continues
- **AND** the corridor track does not start

#### Scenario: the player commits arrival into the corridor

- **WHEN** the current route level changes to `FormalLevel045`
- **THEN** normal gameplay music is stopped
- **AND** `Play_Level5_Music` is posted once

### Requirement: leaving the corridor restores normal music

#### Scenario: Level 5 is preloaded while the player remains in the corridor

- **WHEN** `FormalLevel05` is loaded but the current route level remains `FormalLevel045`
- **THEN** the corridor track continues

#### Scenario: arrival into the square room is committed

- **WHEN** the current route level changes from `FormalLevel045` to `FormalLevel05`
- **THEN** `Stop_Level5_Music` is posted
- **AND** normal gameplay music resumes with the current Wwise States

### Requirement: lifecycle controls the active soundtrack

#### Scenario: the player dies in the corridor

- **WHEN** the death-audio system requests gameplay music to stop
- **THEN** the corridor track is stopped instead of leaking under the death screen

#### Scenario: the corridor level restarts

- **WHEN** music restart is requested while `FormalLevel045` is current
- **THEN** the corridor track restarts from its beginning

