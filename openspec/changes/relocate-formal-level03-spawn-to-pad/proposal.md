## Why

Formal Level 3 currently spawns the player pair at the north-side traversal route, away from the intended Pad area. The player pair needs to begin near the retained Pad so direct Level 3 testing starts at the relevant local interaction area.

## What Changes

- Move the existing Formal Level 3 human and dog spawn anchors to supported, non-overlapping positions adjacent to the retained Pad.
- Preserve the existing player ownership, collider coverage, Pad visual, checkpoint, and exit behavior.
- Verify direct FormalPersistent startup creates one grounded human/dog pair near the Pad.

## Capabilities

### New Capabilities
- `formal-level03-pad-spawn`: Defines reliable adjacent spawn placement for both formal player actors near the Level 3 Pad.

### Modified Capabilities
- None.

## Impact

- Affects FormalLevel03 spawn-anchor transforms and its source manifest.
- Adds no dependencies and changes no mechanics, visuals, or player-prefab ownership.
