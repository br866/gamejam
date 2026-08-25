# Gameplay Audio Loop Lifecycle

## Requirements

### Requirement: Gameplay suspension stops active crate loops

Both formal crate movement implementations SHALL stop an active Wwise push loop
when Unity gameplay simulation becomes suspended.

#### Scenario: pause menu opens while pushing

- **WHEN** the pause menu suspends gameplay while a crate push loop is active
- **THEN** the crate posts its configured stop Event
- **AND** the loop does not continue beneath the menu

#### Scenario: another blocking screen opens while pushing

- **WHEN** a tutorial, death screen, or generic zero-timescale panel suspends gameplay
- **THEN** the same shared lifecycle condition stops the crate loop
- **AND** no screen-specific crate condition is required

### Requirement: Resume does not revive stale movement audio

A crate loop stopped by gameplay suspension SHALL remain stopped after gameplay
resumes until the crate actually moves again.

#### Scenario: gameplay resumes after a pause

- **WHEN** gameplay returns to an active simulation state
- **THEN** the old crate loop remains stopped
- **AND** a new Play Event is posted only after valid crate displacement resumes

