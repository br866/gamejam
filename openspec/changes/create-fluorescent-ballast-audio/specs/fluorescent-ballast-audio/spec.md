# Fluorescent Ballast Audio

## Requirements

### Requirement: Ballast variations share an old electrical identity

The one-shots SHALL share a subdued 50 Hz mains foundation, transformer-like
harmonics, and unstable electrical buzz without sharp alarm-like transients or
musical pitch prominence.

#### Scenario: Wwise selects a ballast variation

- **WHEN** an environmental light event selects any rendered variation
- **THEN** the sound reads as the same old fluorescent fixture family
- **AND** begins and ends smoothly enough for intermittent one-shot playback

### Requirement: Wwise controls environmental behavior

The rendered assets SHALL contain the sound body only and SHALL NOT encode long
silent random intervals.

#### Scenario: environmental timing is tuned

- **WHEN** the designer adjusts random playback behavior
- **THEN** interval, selection, pitch, and positioning can be configured in Wwise
  without rerendering the source audio

### Requirement: Continuous ballast bed loops cleanly

The continuous ballast bed SHALL be a ten-second mono Sound SFX whose electrical
components return to the same phase at the loop boundary, without embedded fades
or silent gaps.

#### Scenario: Wwise loops the continuous ballast bed

- **WHEN** the Sound SFX reaches its end and Wwise returns to its first sample
- **THEN** the 50 Hz mains body and high-frequency transformer texture continue
  without an audible boundary click or periodic fade
