## Why

The formal route has its scenes, shared art, persistent actors, and portions of its runtime foundation, but a shipped build still starts the legacy two-level route and the six formal levels cannot yet be completed in sequence. Consolidate the approved gameplay contract into one playable route so the project has an end-to-end, testable game flow rather than disconnected scene assemblies and debug navigation.

## What Changes

- Connect the player-facing startup path to the formal persistent scene and define a final completion route after Level 5.
- Restore the approved additive transition lifecycle: retain a predecessor until the successor checkpoint commits progress, then safely unload it and release unused shared art.
- Normalize and validate the six formal scene contracts, route identifiers, checkpoints, exits, shared-art catalog, and build-scene registration.
- Complete reusable role-aware, cooperative, prerequisite, reset, and bounded-enemy/safe-zone mechanics required by multiple formal levels.
- Configure and verify the approved Level 1 through Level 5 puzzle, checkpoint, death/reset, exit, and handoff behavior without treating debug jumps as gameplay completion.
- Implement the Level 5 controlled escape, final-room sequence, and an explicit player-facing completion presentation.
- Add automated and manual release verification for a clean build-first-scene playthrough of the formal route.
- Defer presentation polish not required for a coherent route, including the full anxiety system, complete HUD/pause experience, audio integration, and optional hint presentation.

## Capabilities

### New Capabilities
- `formal-route-release-flow`: Player-facing bootstrap, six-level route completion, safe additive handoff, final completion, and release acceptance.
- `formal-route-gameplay-coverage`: Required reusable mechanics and level-specific gameplay coverage for the approved Level 1 through Level 5 route.

### Modified Capabilities
- `formal-level05-traversal-foundation`: Extend Level 5 traversal acceptance from scene setup to the controlled escape and final completion route.

## Impact

- Affected runtime: `Assets/MoMing/Scripts/LevelRuntime/*`, player control/camera integration, enemy navigation, and reusable formal puzzle components.
- Affected Unity assets: `FormalPersistent`, all six formal level scenes, shared art scenes, formal gameplay prefabs, final UI/scene assets as needed, and `ProjectSettings/EditorBuildSettings.asset`.
- Affected tests: formal scene-contract, route lifecycle, mechanism, monster-boundary, and build-first-scene Play Mode validation.
- Existing legacy prototype scenes and their `GameManager`, `PlayerManager`, and `Puzzle/*` path remain available as reference and are not migrated by this change.
