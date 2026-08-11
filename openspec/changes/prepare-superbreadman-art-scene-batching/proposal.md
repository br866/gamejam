## Why

`Assets/Scenes/Test/superbreadman 1.unity` contains a large art-scene renderer population with no static batching preparation, while its repeated environment assets create an immediate low-risk draw-call reduction opportunity. Preparing only verified immobile art content now preserves later options for GPU instancing and mesh consolidation without prematurely restricting level iteration.

## What Changes

- Establish a measured rendering baseline for representative art-scene viewpoints.
- Classify existing art-scene renderers into static-batching candidates, explicitly excluded dynamic or interactive content, and items requiring investigation.
- Enable static batching only for verified immobile environment and decorative content, while retaining scene objects and prefab structure.
- Consolidate only demonstrably equivalent material references and redundant material slots to improve compatible renderer grouping without changing visual appearance.
- Remove unused animation import data from verified static scene-model FBXs.
- Validate that batching preparation does not regress the route, visual presentation, lighting, shadows, collision, or build health.
- Defer GPU instancing, mesh combination, Shader Graph changes, texture atlasing, UV changes, cross-appearance material unification, and prefab restructuring to later work.

## Capabilities

### New Capabilities
- `superbreadman-art-scene-batching`: Defines safe static-batching preparation, measurement, and regression boundaries for the SuperBreadMan art scene.

### Modified Capabilities

- None.

## Impact

- Target scene: `UnityProject/Assets/Scenes/Test/superbreadman 1.unity`.
- May update existing art-scene object static flags, references to verified equivalent material assets, redundant material-slot assignments, and eligible `Assets/SuperBreadMan/Scene Model/` FBX importer animation settings.
- Requires Unity Editor rendering metrics, Frame Debugger investigation, Play Mode route checks, and a standalone Windows build check.
- Does not change runtime scripts, game logic, mesh topology, material appearance or parameters, shaders, prefab hierarchy, transforms, colliders, navigation, UI, lighting setup, or GPU-instancing configuration.
