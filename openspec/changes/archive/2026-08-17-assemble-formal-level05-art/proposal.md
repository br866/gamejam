## Why

Level 5 is the final remaining selected visual segment of the formal route and still exists only in the prototype scene.

## What Changes

- Create FormalLevel05 and L05_Content from the current user selection.
- Preserve selected visual world transforms using flattened per-Renderer copies.
- Exclude only prototype player-system renderers and strip all prototype runtime behavior.

## Capabilities

### New Capabilities
- `formal-level05-art-assembly`: Defines user-selected visual-only assembly of Formal Level 5.

### Modified Capabilities
- None.

## Impact

- Adds FormalLevel05, L05_Content, source manifest, and Build Settings entry.
- Does not add Level 5 collision, player spawning, mechanics, or exit behavior.
