## Why

Formal-level art currently relies heavily on MeshColliders, including small decorative models that should have no physical presence. This increases physics cost, produces inconsistent player blocking, and makes the A* navigation obstacle set difficult to reason about.

## What Changes

- Normalize collider responsibility across `Assets/MoMing/FormalLevels/` using the existing navigation Layers.
- Replace eligible navigation-relevant MeshColliders with bounds-aligned BoxColliders.
- Remove all Collider components from visual-only small props so they have no physics or navigation presence.
- Require complex or irregular models that need physical blocking to be reviewed rather than automatically approximated by one oversized box.
- Convert the remaining direct rendered formal-scene object into a Prefab instance.

## Capabilities

### New Capabilities
- `formal-level-collider-normalization`: Defines collider, Layer, and navigation-obstacle responsibilities for formal-level assets.
- `formal-level-rendered-prefab-ownership`: Ensures rendered formal-level content is owned by reusable Prefab assets rather than direct scene objects.

### Modified Capabilities
- None.

## Impact

- Formal level Prefabs, shared-art Prefabs, and the formal level scenes.
- A* GridGraph obstacle detection, which already reads only `NavStatic` and `NavDynamic`.
- Formal-level physics, player traversal, camera obstruction, and dynamic-prop interactions.
