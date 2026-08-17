## Why

Formal Level 3 has no scene or art assembly, while the current source-scene selection contains a substantial Level 3 candidate group mixed with a small number of Level 4, shared, player, interaction, and already-migrated Level 2 objects. A source-preserving classification pass is needed before any third-level art can be safely assembled.

## What Changes

- Classify the current 270-object source-scene selection into explicit Level 3 candidates, formal Level 1/2 duplicates, later-level objects, player/runtime objects, and unresolved shared candidates.
- Assemble only confirmed Level 3 visual content into a new `FormalLevel03` scene and Level 3-owned content Prefab.
- Preserve source world transforms and record source identities, exclusions, and unresolved candidates in a Level 3 manifest.
- Do not configure gameplay, collision, navigation, player spawning, or mechanics in this art-assembly change.

## Capabilities

### New Capabilities
- `formal-level03-art-assembly`: Defines source-preserving, non-duplicating Level 3 visual assembly from a mixed source selection.

### Modified Capabilities
- None.

## Impact

- Affects `Assets/MoMing/FormalLevels/FormalLevel03.unity`, Level 3-owned Prefabs and manifest assets.
- Reads the source prototype scene and existing formal Level 1/2 art only as references.
- Does not modify the source scene, runtime scripts, game flow, controls, collision, navigation, or Level 2 content.
