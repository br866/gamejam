# Level 3 Pressure Plate Audio

## Requirements

### Requirement: Successful Level 3 physical Pedals play the common press Event

Each of the five physical Pedal instances in Formal Level 3 SHALL post
`Play_PressurePlate` when its configured actor requirement first completes.
The Event SHALL use the same completion transition that starts the Pedal press
animation.

#### Scenario: eligible actor completes a Level 3 Pedal

- **WHEN** the Pedal's configured actor requirement becomes satisfied
- **THEN** the Pedal starts its existing down-press animation
- **AND** `Play_PressurePlate` is posted once from that Pedal

#### Scenario: ineligible actor enters a Level 3 Pedal

- **WHEN** an actor rejected by that Pedal's requirement enters its trigger
- **THEN** the Pedal does not start its down-press animation
- **AND** `Play_PressurePlate` is not posted

