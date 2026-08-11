## Why

The SuperBreadMan whitebox and art scenes need an agreed, traceable gameplay contract before either scene is repaired. Without a shared scope, scene work can accidentally change controls, collision, art, or unrelated prototype content.

## What Changes

- Define the two target scenes as paired versions of the same standalone level.
- Define the required Level1 through Level5 main-route sequence and its gameplay coverage.
- Define acceptance expectations for a playable route, failure recovery, keyboard input, pause behavior, and available audio integration points.
- Constrain the first implementation pass to existing scene-object configuration only.
- Preserve all existing documentation and exclude the duplicate MoMing test scene from this change.

## Capabilities

### New Capabilities
- `superbreadman-level-alignment`: Defines the paired whitebox and art scene contract, main-route requirements, and scene-configuration-only implementation boundary.

### Modified Capabilities

- None.

## Impact

- Target scenes: `UnityProject/Assets/Scenes/Test/superbreadman.unity` and `UnityProject/Assets/Scenes/Test/superbreadman 1.unity`.
- Future implementation may update existing scene object references, component fields, tags, layers, active states, and enabled states.
- Runtime scripts, object transforms, collision, navigation, UI, audio assets, and art assets are not editable in the first implementation pass.
- Existing MoMing documentation remains preserved as background reference.
