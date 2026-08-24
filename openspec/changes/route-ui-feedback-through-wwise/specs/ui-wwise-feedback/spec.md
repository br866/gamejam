# ui-wwise-feedback Specification

## Purpose

Provide consistent Wwise hover and click feedback across the project's Unity UI without per-button authoring or changes to existing button actions.

## ADDED Requirements

### Requirement: Loaded UI buttons receive feedback

The game SHALL install Wwise UI feedback handlers on every Unity UI `Button` under each loaded scene, including buttons whose GameObjects are initially inactive.

#### Scenario: A scene containing inactive menus loads

- **WHEN** a scene finishes loading
- **THEN** active and inactive descendant Buttons receive exactly one feedback handler

#### Scenario: A runtime button appears after scene load

- **WHEN** a Button is instantiated after the initial scene scan
- **THEN** the periodic unscaled-time refresh installs one feedback handler without duplicating existing handlers

### Requirement: Pointer hover posts the authored Hover Event

An interactable Button SHALL post `Play_UI_Hover` once when the pointer enters it.

#### Scenario: Pointer enters an enabled Button

- **WHEN** the pointer enters an active and interactable Button
- **THEN** the configured Hover Event is posted once on the shared UI emitter

#### Scenario: Pointer enters a disabled Button

- **WHEN** the pointer enters a disabled or non-interactable Button
- **THEN** no Hover Event is posted

### Requirement: Valid activation posts the authored Click Event

An interactable Button SHALL post `Play_UI_Click` for a valid left pointer press or UI Submit action. Posting on press ensures scene-changing buttons do not destroy their emitter before feedback begins.

#### Scenario: Player presses an enabled Button

- **WHEN** Unity dispatches a left-button pointer press to an active and interactable Button
- **THEN** the configured Click Event is posted once without replacing the Button's existing callback

#### Scenario: Player submits a selected Button

- **WHEN** Unity dispatches Submit to an active and interactable selected Button
- **THEN** the configured Click Event is posted once

