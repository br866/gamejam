# monster-wwise-footsteps Specification

## Purpose

Route all current formal monster variants through the authored positional Wwise Brutedoc footstep Event.

## ADDED Requirements

### Requirement: Moving monsters post positional Wwise footsteps

Every formal monster using `MonsterPatrol` SHALL post the configured Wwise Event on its own GameObject after travelling the active horizontal stride distance.

#### Scenario: Patrol stride is reached

- **WHEN** a patrolling monster travels at least the configured patrol stride
- **THEN** it posts `Play_Footstep_Brutedoc` once on that monster GameObject

#### Scenario: Chase stride is reached

- **WHEN** a normal or forced-chasing monster travels at least the configured chase stride
- **THEN** it posts `Play_Footstep_Brutedoc` once on that monster GameObject

#### Scenario: Monster is blocked or stationary

- **WHEN** a monster does not travel far enough even if navigation remains active
- **THEN** it does not post a footstep

### Requirement: All current monster variants share the hookup

The FormalLevel02 monster, both FormalLevel04 monsters, and the reusable Enemy Monster prefab SHALL reference the same Wwise Event asset.

#### Scenario: Any formal monster variant moves

- **WHEN** any of the three current formal monster visual variants moves far enough
- **THEN** the shared Brutedoc footstep playback path is available

### Requirement: Repositioning does not create false footsteps

The cadence origin SHALL be reset whenever `MonsterPatrol` resets its position.

#### Scenario: Level reset teleports a monster

- **WHEN** `ResetPatrol` returns a monster to its start position
- **THEN** that reposition does not count toward the next footstep
