# formal-wwise-music-state-sync Specification

## Purpose

Synchronize formal-route gameplay threat and anxiety state into the authored Wwise interactive music State Groups while keeping one continuous `Play_MusicMode` event alive across additive levels and restarts.

## ADDED Requirements

### Requirement: Monster chase drives MusicMode

The formal music controller SHALL set `MusicMode` to `Combat` while any active loaded `MonsterPatrol` reports `IsChasing`; otherwise it SHALL set `MusicMode` to `Explore`.

#### Scenario: Level without monsters

- **WHEN** the current formal level contains no active monster
- **THEN** Wwise `MusicMode` is `Explore`

#### Scenario: Any monster detects a player

- **WHEN** at least one active monster enters chase state
- **THEN** Wwise `MusicMode` changes to `Combat`

#### Scenario: All monsters stop chasing

- **WHEN** no active loaded monster remains in chase state
- **THEN** Wwise `MusicMode` returns to `Explore`

### Requirement: Formal anxiety drives AnxietyLevel

The controller SHALL map `FormalAnxietyState.Normalized` to `AnxietyLevel` Low, Mid, or High using serialized thresholds with defaults 0.45 and 0.75.

#### Scenario: Anxiety crosses a band threshold

- **WHEN** normalized anxiety crosses from one configured band into another
- **THEN** the corresponding Wwise `AnxietyLevel` State is applied once

### Requirement: Initial States precede music playback

The controller SHALL apply the current `MusicMode` and `AnxietyLevel` before posting the configured `AkAmbient` event.

#### Scenario: Formal route begins

- **WHEN** `FormalPersistent` initializes and `FormalAnxietyState` is available
- **THEN** both State Groups have valid values before `Play_MusicMode` is posted

### Requirement: Additive levels and restart remain synchronized

The controller SHALL discover monsters added or removed by additive scene operations and SHALL re-evaluate both State Groups after formal resets or state-instance changes.

#### Scenario: Enter a level containing monsters

- **WHEN** an additive level containing `MonsterPatrol` components loads
- **THEN** those monsters participate in Combat detection without recreating the music event

#### Scenario: Death restart resets gameplay state

- **WHEN** the player restarts and anxiety/monster patrol state resets
- **THEN** the Wwise States return to the values represented by the reset runtime state without reposting duplicate music

