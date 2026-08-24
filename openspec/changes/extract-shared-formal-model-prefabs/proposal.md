## Why

Formal levels currently duplicate many independently placeable art models inside flattened level-content prefabs. Editing a recurring door, cabinet, bed, screen, trolley, or other reusable model requires finding and changing every copy, which makes visual iteration slow and inconsistent.

## What Changes

- Define a model audit that identifies every formal GameObject carrying a renderable model component and records whether it already has an independent prefab source.
- Extract every modeled formal object that does not already have an independent prefab into a reusable formal model prefab, including large architectural model groups.
- Replace qualifying duplicated model instances with references to the extracted prefabs while preserving world transforms, materials, active state, and required collision behavior.
- Do not extract objects without a model component or duplicate existing independent model prefabs, including the existing formal door prefabs.
- Preserve scene-owned collision roots, gameplay triggers, checkpoints, monsters, scene-specific door configuration, and shared-scene layout ownership around extracted visuals.
- Add editor validation that reports extracted prefab identity, references, transform preservation, and unresolved ambiguous candidates.

## Capabilities

### New Capabilities
- `formal-shared-model-prefabs`: Reusable formal art-model prefab extraction, identity rules, scene replacement behavior, and validation.

### Modified Capabilities
- None.

## Impact

- Affected assets: `Assets/MoMing/FormalLevels/Prefabs/`, `L01_Content` through `L05_Content`, and the five `FormalSharedArt_*` scenes where a qualifying model is used.
- Affected tooling: formal-art editor utilities and validation tests.
- No intended changes to gameplay behavior, scene route ownership, player controls, collision topology, or art placement.
