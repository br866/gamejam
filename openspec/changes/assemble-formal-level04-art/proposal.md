## Why

Formal Level 4 has not yet been separated from the prototype art scene, preventing it from being loaded through the formal player and scene-flow path.

## What Changes

- Create FormalLevel04 and a visual-only Level 4 content Prefab from the explicit Level 4 source groups.
- Exclude Level 4.5 source hierarchy and strip prototype runtime behavior from copied visuals.
- Add formal entry anchors and foundational collision so the persistent player pair can load and stand in the Level 4 area.

## Capabilities

### New Capabilities
- `formal-level04-art-assembly`: Defines visual-only assembly of the explicit prototype Level 4 environment into FormalLevel04.
- `formal-level04-traversal-foundation`: Defines basic grounded player entry and collision support for FormalLevel04.

### Modified Capabilities
- None.

## Impact

- Adds FormalLevel04, L04_Content, Level04 source manifest, and a Build Settings entry.
- Does not alter the source scene, Level 4.5, player ownership, or Level 4 mechanics.
