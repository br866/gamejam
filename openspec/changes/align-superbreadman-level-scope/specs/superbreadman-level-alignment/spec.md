## ADDED Requirements

### Requirement: Paired standalone scene coverage

The project SHALL treat `Assets/Scenes/Test/superbreadman.unity` as the whitebox reference scene and `Assets/Scenes/Test/superbreadman 1.unity` as the art scene for one shared level.

The scenes SHALL be playable independently from the Unity Editor and SHALL NOT require integration into the legacy `Start` to `End` scene sequence for this change.

#### Scenario: Play the whitebox version independently

- **WHEN** a developer opens `Assets/Scenes/Test/superbreadman.unity` and enters Play Mode
- **THEN** the level starts without requiring another game scene to load first

#### Scenario: Play the art version independently

- **WHEN** a developer opens `Assets/Scenes/Test/superbreadman 1.unity` and enters Play Mode
- **THEN** the level starts without requiring another game scene to load first

### Requirement: One-to-one main route

The whitebox and art scenes SHALL represent the same main route. Required gameplay stages, gate order, checkpoint placement, monster encounter, sequence gate, and final exit SHALL correspond one-to-one between the two scene versions.

The main route SHALL progress through Level1, Level2, Level3, Level4, Level4.5, and Level5 before the final exit.

#### Scenario: Complete the required room order

- **WHEN** a player follows the intended route in either target scene
- **THEN** the player progresses through Level1, Level2, Level3, Level4, Level4.5, and Level5 in that order before reaching the exit

#### Scenario: Compare paired scenes

- **WHEN** a developer compares the whitebox and art scenes
- **THEN** each required main-route interaction has a corresponding interaction at the same point in the route

### Requirement: Required gameplay coverage

The main route in each target scene SHALL require character switching, a separation that creates anxiety pressure, linked-mode box interaction, human-only interaction, dog-specific ability use, a monster threat, checkpoint activation, and sequence-gate completion.

#### Scenario: Exercise core dual-character play

- **WHEN** a player completes the main route
- **THEN** completion requires switching between the human and dog, separating them, using linked mode with a box, and using a human-only interaction

#### Scenario: Exercise risk and recovery systems

- **WHEN** a player completes the main route
- **THEN** the route includes dog-specific ability use, a monster threat, at least one activated checkpoint, and a sequence-gate interaction

### Requirement: Recoverable failure

An anxiety failure or monster capture in either target scene SHALL respawn the characters at the most recently activated existing checkpoint. If no checkpoint has been activated, the scene's existing start positions SHALL be used.

#### Scenario: Fail after a checkpoint

- **WHEN** the player activates an existing checkpoint and later reaches maximum anxiety or is caught by a monster
- **THEN** both characters respawn using that checkpoint

#### Scenario: Fail before a checkpoint

- **WHEN** the player reaches maximum anxiety or is caught before activating a checkpoint
- **THEN** both characters respawn at the scene's existing start positions

### Requirement: Keyboard-only validation

This change SHALL validate the existing keyboard controls only: WASD, Tab, Q, E, F, Space, LeftShift, and Escape. Controller support is out of scope.

#### Scenario: Complete the route with keyboard controls

- **WHEN** a player uses the existing keyboard control scheme
- **THEN** the player can exercise every required main-route interaction

### Requirement: Existing pause and audio integration

The target scenes SHALL retain their existing pause, restart, return-to-menu, and settings entry points where already present. The scenes SHALL retain available core-interaction audio integration points, but actual audio assets and sound-quality verification are not required for this change.

#### Scenario: Pause during play

- **WHEN** the player presses Escape in a scene with the existing pause UI configured
- **THEN** the player can continue, restart the current scene, return to the existing start menu, and access the existing audio settings entry point

#### Scenario: Audio assets are unavailable

- **WHEN** a configured interaction has no assigned audio clip
- **THEN** the interaction continues without an audio-related runtime error

### Requirement: Scene-configuration-only implementation

The first implementation pass SHALL only alter existing objects in the two target scenes through component field references, component parameter values, tags, layers, active states, and component enabled states.

The pass SHALL NOT add, copy, delete, move, or rotate scene objects. It SHALL NOT modify runtime scripts, player controls, collision, navigation, UI, audio assets, models, materials, lighting, or existing documentation.

#### Scenario: Repair an existing gate link

- **WHEN** an existing main-route gate is blocked by a missing or incorrect controller reference
- **THEN** the implementation may update the existing component field reference without changing object structure or transforms

#### Scenario: Encounter a non-configurable blocker

- **WHEN** the cause of a main-route failure requires code, collision, navigation, UI, assets, transforms, or object creation/deletion
- **THEN** the blocker is recorded for a later change instead of being changed in this pass

### Requirement: Preserve unrelated content

All existing documentation SHALL remain in place and no existing document SHALL be deleted for this change. `Assets/MoMing/Scenes/Test/superbreadman.unity` and main-route-external scene content SHALL remain outside this change's implementation and acceptance scope.

#### Scenario: Review unrelated scene content

- **WHEN** a developer finds an object or area outside the defined main route
- **THEN** it is preserved and does not prevent acceptance unless it directly blocks the main route
