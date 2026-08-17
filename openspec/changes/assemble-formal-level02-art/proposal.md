## Why

The formal route has a Level 2 scene but no art assembly, while the source art scene contains a mixed selection of Level 1 and later-route content. A scoped migration is needed to establish Level 2's large-scale art layout without damaging the source scene or duplicating Level 1's formal content.

## What Changes

- Assemble a Level 2 art-content prefab and instantiate it in `FormalLevel02`.
- Classify the current source-scene bulk selection into Level 1 duplicates, Level 2 candidates, later-level candidates, prototype-only objects, and unresolved content.
- Exclude all content already represented by Level 1's formal prefabs from the Level 2 migration while preserving `superbreadman 1.unity` unchanged.
- Arrange the accepted Level 2 art as large-scale environment and set-dressing groups only; defer gameplay interactions, navigation, collision, and runtime behavior.

## Capabilities

### New Capabilities
- `formal-level02-art-assembly`: Defines the source-preserving, non-duplicating art assembly boundary for the formal Level 2 scene.

### Modified Capabilities
- None.

## Impact

- Affects `Assets/MoMing/FormalLevels/FormalLevel02.unity` and new Level 2 content prefabs or manifests under `Assets/MoMing/FormalLevels/`.
- Reads `Assets/Scenes/Test/superbreadman 1.unity` and Level 1's source manifest as reference only.
- Does not alter gameplay scripts, controls, collision, navigation, UI, audio, lighting, materials, or the source art scene.
