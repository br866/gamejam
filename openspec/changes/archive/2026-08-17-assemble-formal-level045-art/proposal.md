## Why

The selected Level 4.5 corridor art remains only in the prototype source scene and cannot yet be reviewed as a standalone formal route segment.

## What Changes

- Create FormalLevel045 and L045_Content from the user's current visual selection.
- Preserve selected visual world transforms with a per-Renderer global-transform copy.
- Strip prototype runtime behavior and exclude only the prototype player system.

## Capabilities

### New Capabilities
- `formal-level045-art-assembly`: Defines user-selected, visual-only assembly of the Level 4.5 route segment.

### Modified Capabilities
- None.

## Impact

- Adds FormalLevel045, L045_Content, source manifest, and Build Settings entry.
- Does not modify source art, player ownership, collision, or Level 4.5 mechanics.
