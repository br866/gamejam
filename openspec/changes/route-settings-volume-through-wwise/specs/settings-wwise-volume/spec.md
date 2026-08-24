# settings-wwise-volume Specification

## Purpose

Let the Unity settings menu control Wwise Music and SFX bus volume while preserving embedded cutscene video audio as the only Unity-side playback exception.

## ADDED Requirements

### Requirement: Settings sliders drive global Wwise volume parameters

The Music and SFX sliders SHALL set the global `MusicVolume` and `SFXVolume` Wwise Game Parameters respectively.

#### Scenario: Player changes Music volume

- **WHEN** the player moves the Music slider from 0 to 1
- **THEN** Unity sends the corresponding global `MusicVolume` value from 0 to 100

#### Scenario: Player changes SFX volume

- **WHEN** the player moves the SFX slider from 0 to 1
- **THEN** Unity sends the corresponding global `SFXVolume` value from 0 to 100

### Requirement: Saved volume applies without opening Settings

Saved Music and SFX volume values SHALL be applied after the Wwise sound engine initializes.

#### Scenario: Game starts with saved settings

- **WHEN** Wwise initialization completes and the settings panel has not been opened
- **THEN** both global volume Game Parameters reflect the saved PlayerPrefs values

### Requirement: Cutscene embedded audio remains available

Cutscene videos SHALL send embedded audio directly to platform output while Unity's normal AudioSource system remains disabled.

#### Scenario: A configured cutscene starts

- **WHEN** `CutscenePlayer` starts a video containing an enabled audio track
- **THEN** its audio output mode is Direct and the embedded audio can play without enabling Unity AudioSources
