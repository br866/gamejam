# Checkpoint Audio

## Requirements

### Requirement: Successful checkpoint commits receive audio confirmation

The formal checkpoint system SHALL post `Play_CheckpointSFX` from the checkpoint
carpet GameObject only when human and dog recovery anchors are successfully
committed.

#### Scenario: a player enters an eligible checkpoint for the first time

- **WHEN** either player enters a checkpoint whose prerequisites are complete
- **AND** the owning level controller accepts the checkpoint anchors
- **THEN** the checkpoint becomes complete
- **AND** `Play_CheckpointSFX` is posted once from that checkpoint carpet

#### Scenario: a player enters an unavailable checkpoint

- **WHEN** the owning level is unavailable or prerequisites are incomplete
- **THEN** no checkpoint audio is posted

#### Scenario: a completed checkpoint is crossed again

- **WHEN** either player re-enters an already completed checkpoint
- **THEN** no additional checkpoint audio is posted

### Requirement: Every placed formal checkpoint carpet is configured

All formal checkpoint carpets placed in Levels 2, 3, 4, 4.5, and 5 SHALL
reference the generated `Play_CheckpointSFX` Wwise Event object.

#### Scenario: a formal level checkpoint scene is loaded

- **WHEN** any formal level from Level 2 through Level 5 loads its checkpoint
- **THEN** the checkpoint Event reference is valid and can load its AutoBank
