## Why

The SuperBreadMan whitebox cannot reliably support its intended route while geometry collision is inconsistently classified, player movement mixes transform and Rigidbody updates, and monsters move directly through their patrol and chase targets. The already-validated A* package provides a viable navigation base, so the whitebox needs a focused foundation before the paired art scene is aligned.

## What Changes

- Classify existing whitebox objects with dedicated navigation, actor, and trigger layers, without altering the route layout or adding scene objects.
- Replace every MeshCollider in the target whitebox scene with a BoxCollider derived from its source mesh bounds.
- Leave whitebox object Layer assignment under manual Unity Editor control, using the provided layer names as the navigation classification vocabulary.
- Provide an implementation-ready design and Unity Editor procedure for later interaction and navigation setup; do not add gameplay or navigation objects in this pass.
- Keep the paired art scene, route logic, UI, audio, models, materials, lighting, and broad level redesign out of scope for this whitebox foundation.

## Capabilities

### New Capabilities
- `whitebox-navigation-foundation`: Defines collision classification, stable actor movement, A* navigation graph behavior, and obstacle-aware monster patrol and chase in the SuperBreadMan whitebox.

### Modified Capabilities

- None.

## Impact

- Whitebox target: `UnityProject/Assets/MoMing/Scenes/Test/superbreadman.unity`.
- Project layer configuration: `UnityProject/ProjectSettings/TagManager.asset`.
- Whitebox collider conversion utility: `Assets/Editor/WhiteboxColliderTools.cs`.
- Future navigation dependency: the installed `com.arongranberg.astar` package and the established isolated A* smoke-test configuration as the implementation reference.
- The art scene at `Assets/Scenes/Test/superbreadman 1.unity` remains a later alignment task after this whitebox pass is verified.
