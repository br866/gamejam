## ADDED Requirements
### Requirement: Visual identity
Formal monster audio SHALL follow the active model and have clearly distinct material/transient/rhythm characteristics grounded in inspected art.
#### Scenario: Scene model override
- **WHEN** a formal scene overrides a monster visual model
- **THEN** its sound family matches the active visual model rather than its object name.
### Requirement: Scope protection
Dog footsteps, UI and music SHALL remain unchanged from this iteration baseline; delivery SHALL remain local and reversible.
#### Scenario: Delivery
- **WHEN** final validation runs
- **THEN** protected sources and Wwise objects match their baseline and all new audio has provenance.
### Requirement: Formal-level coverage
The change SHALL include an asset-grounded cue matrix, authored Wwise events and working Unity playback for selected missing diegetic cues.
#### Scenario: Audible interaction
- **WHEN** a supported action occurs in formal gameplay
- **THEN** the corresponding spatial sound starts and any sustained sound stops with its lifecycle.
