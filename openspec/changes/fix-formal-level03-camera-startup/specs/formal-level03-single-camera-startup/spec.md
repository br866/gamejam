## Purpose

Ensure Formal Level 3 starts with one active main camera that follows the persistent formal player pair rather than a competing prototype camera.

## ADDED Requirements

### Requirement: Single formal main camera
The system SHALL leave only the persistent formal follow camera enabled and tagged `MainCamera` when Formal Level 3 loads through FormalPersistent.

#### Scenario: Formal Level 3 starts
- **WHEN** FormalPersistent loads FormalLevel03 additively
- **THEN** exactly one enabled camera is tagged `MainCamera` and it has a CameraFollow target assigned to the formal human actor.

### Requirement: Prototype camera does not render in formal Level 3
The system SHALL keep FormalLevel03's scene-local prototype camera from rendering during formal runtime loading.

#### Scenario: Formal Level 3 is active
- **WHEN** FormalLevel03 is loaded by the formal game flow
- **THEN** its scene-local prototype camera component is disabled.
