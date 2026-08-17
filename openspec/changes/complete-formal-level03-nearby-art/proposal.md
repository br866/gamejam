## Why

Formal Level 3 remains visually incomplete because its first migration was restricted to explicit Level 3 hierarchy groups, while the user's current selection identifies a larger third-level area and nearby supporting art. The larger candidate set must be incorporated without importing second- or fourth-level content.

## What Changes

- Scan visual source objects inside and immediately adjacent to the current selection bounds.
- Add every nearby visual object to `L03_Content` while excluding only Level 2, Level 4, and same-position duplicates already in Formal Level 3.
- Preserve world transforms and strip source runtime behavior from newly copied visual objects.
- Extend the Level 3 manifest with the scan bounds, inclusion counts, exclusions, and source identities.

## Capabilities

### New Capabilities
- `formal-level03-nearby-art-completion`: Defines bounded nearby-art inclusion with only Level 2, Level 4, and same-position duplicate exclusions for completing Formal Level 3 visuals.

### Modified Capabilities
- None.

## Impact

- Affects `L03_Content.prefab`, `FormalLevel03.unity`, and `Level03SourceManifest.md`.
- Reads the prototype source scene and existing formal level content as reference.
- Does not modify source scene objects, player systems, collision, navigation, or level mechanics.
