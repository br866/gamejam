# Fluorescent Light Audio Routing

## Requirements

### Requirement: Every formal pendant lamp is a spatial Wwise emitter

The runtime SHALL post `Play_Fluorescent_Light` once from every active formal
level model whose name identifies it as a `pendant_lamp`.

#### Scenario: a formal level loads additively

- **WHEN** the loaded scene contains active pendant-lamp models
- **THEN** each model receives one Wwise GameObject emitter
- **AND** the Event is posted from that model's transform position

#### Scenario: the router scans an already processed scene

- **WHEN** the same pendant lamp is discovered again
- **THEN** no duplicate emitter component or Event instance is created

#### Scenario: another lamp type is present

- **WHEN** a model is a wall lamp, floor lamp, or standalone Unity Light object
- **THEN** the fluorescent ceiling-lamp Event is not installed on it

### Requirement: Event AutoBank remains loadable

The runtime SHALL retain a serialized `AK.Wwise.Event` reference for
`Play_Fluorescent_Light` in a Resources settings asset.

#### Scenario: a lamp posts its Event

- **WHEN** the first pendant lamp becomes active
- **THEN** the Event reference is valid
- **AND** its Auto-Defined SoundBank can be loaded by the Wwise integration
