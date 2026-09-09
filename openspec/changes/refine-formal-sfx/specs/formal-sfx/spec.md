## ADDED Requirements
### Requirement: Footstep coverage
All formal player and monster actors SHALL have valid footstep events; each role family SHALL provide at least 12 variations without immediate repetition.
#### Scenario: Formal patrol or chase
- **WHEN** a formal monster travels
- **THEN** its existing distance trigger emits spatial footsteps.
### Requirement: Controlled ambience
Formal gameplay scenes SHALL add restrained spatial air and metal sounds at suitable existing objects and release playback on disable/unload.
#### Scenario: Scene exit
- **WHEN** the sound source is disabled or unloaded
- **THEN** its playing sounds stop.
### Requirement: Reversible local delivery
The change SHALL preserve music and provide original backups, asset provenance, measurements and validation notes without remote synchronization.
#### Scenario: Review
- **WHEN** the user checks the local version
- **THEN** they can compare original/processed sounds and inspect each change.

### Requirement: Motion and mix consistency
SFX SHALL track actual path travel and mechanism motion, use spatial attenuation for world interactions, and provide a usable volume curve without modifying music.
#### Scenario: High-frame-rate door motion
- **WHEN** a door rotates in small per-frame increments
- **THEN** its movement sound plays and closing motion produces a stop cue.
#### Scenario: SFX volume control
- **WHEN** SFX volume changes from 100 to 50 to 0
- **THEN** the authored midpoint attenuation is -6 dB and zero mutes the SFX path.
#### Scenario: Lamp density
- **WHEN** many fluorescent props are nearby
- **THEN** only the configured nearest lamp emitters are selected, with a default cap of three.
