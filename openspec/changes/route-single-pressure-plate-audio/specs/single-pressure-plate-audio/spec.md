# Single Pressure Plate Audio

## Requirements

### Requirement: A successful standalone Pedal completion plays its authored Event

A formal pressure-plate trigger with completion audio enabled SHALL post
`Play_PressurePlate` exactly once when it first becomes complete. This includes
the Level 2 cooperative single plate even though its visual is not driven by
`FormalPedalPress`.

#### Scenario: eligible actor completes a standalone plate

- **WHEN** the configured actor requirement becomes satisfied
- **THEN** the Pedal starts its press animation
- **AND** `Play_PressurePlate` is posted once from that Pedal

#### Scenario: ineligible actor enters

- **WHEN** an actor rejected by the Pedal requirement enters its trigger
- **THEN** the Event is not posted

#### Scenario: cooperative requirement is incomplete

- **WHEN** only one actor occupies a both-player Pedal
- **THEN** the Event is not posted until both actors satisfy the requirement

### Requirement: The Formal Level 3 multi-plate chain remains unscored

Every Pedal instance in the Formal Level 3 multi-plate chain SHALL opt out of
the standalone completion Event while retaining its existing trigger and visual
behavior.

#### Scenario: a Level 3 chain plate completes

- **WHEN** any of the five Level 3 chain Pedals completes
- **THEN** its existing actuator and press animation behavior continues
- **AND** `Play_PressurePlate` is not posted
