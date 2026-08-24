# Delta Spec: formal-persistent-camera-bootstrap

## Purpose

Guarantees that the Formal route always renders through a deliberately configured follow camera placed in FormalPersistent, so gameplay never shows the factory-default blue clear color produced by dynamically spawned cameras, and URP post-processing is consistently applied.

## ADDED Requirements

### Requirement: FormalPersistent SHALL contain a pre-placed main camera

The FormalPersistent scene SHALL contain exactly one enabled camera tagged `MainCamera` at load time, configured with `ClearFlags = SolidColor` and a black background color.

#### Scenario: Scene loads with its own camera

- **WHEN** FormalPersistent is loaded (alone or additively with FormalLevel scenes) and play mode starts
- **THEN** an enabled `MainCamera`-tagged camera already exists in the loaded scenes before player control initializes

#### Scenario: Background is black

- **WHEN** the game view is rendered through this camera in areas outside any room geometry
- **THEN** the rendered background is solid black, not blue or skybox-rendered

### Requirement: Runtime bootstrap SHALL reuse the pre-placed camera instead of spawning one

When the formal player control initializes and a follow-camera component is present in the loaded scenes, it SHALL attach to and drive that existing camera for the active actor, and SHALL NOT create a replacement camera object.

#### Scenario: Follow camera found at startup

- **WHEN** formal play mode starts with the pre-placed camera present
- **THEN** the camera follows the active player actor and no additional camera GameObject is created during startup

#### Scenario: Exactly one enabled camera after startup

- **WHEN** the formal route is playing
- **THEN** there is exactly one enabled `MainCamera`-tagged camera rendering the game view

### Requirement: Post-processing volume SHALL be present in FormalPersistent

FormalPersistent SHALL contain a global URP volume referencing the same profile used by the superbreadman art scenes, so post-processing effects apply while playing the formal route.

#### Scenario: Effects visible in play mode

- **WHEN** the formal route is played through the pre-placed camera
- **THEN** post-processing effects from the shared profile (bloom, vignette, film grain, color adjustments, tonemapping, white balance, chromatic aberration, shadows/midtones/highlights) are applied to the rendered image

### Requirement: Player system prefab contents SHALL NOT be duplicated into FormalPersistent

Copying camera setup into FormalPersistent SHALL NOT introduce additional player actors or UI canvas objects from the source prefab; the formal flow retains sole ownership of spawning players.

#### Scenario: Hierarchy stays clean

- **WHEN** the camera and volume are added to FormalPersistent
- **THEN** no Human/Dog actor objects or UI Canvas from the source player-system prefab exist as new root objects in FormalPersistent
