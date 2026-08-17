## Why

The manually curated Level 3 art now represents the intended layout, but most retained static visuals still have no physical presence. Broad collision is needed so the existing human and dog actors can be spawned and tested against the visible environment without changing the curated art selection.

## What Changes

- Add broad enabled non-trigger 3D Collider coverage to retained Level 3 static architecture, walls, furniture, doors, and other substantial fixed props.
- Preserve the established Level 3 floor, boundary, wall-proxy, spawn, checkpoint, and provisional-exit route foundation.
- Keep visual-only guidance and small decoration non-blocking.
- Verify direct `FormalPersistent` startup into `FormalLevel03` creates one grounded human/dog pair that can traverse the approved baseline route.

## Capabilities

### New Capabilities
- `formal-level03-collider-coverage`: Defines physical coverage and non-blocking exceptions for the manually curated Formal Level 3 visuals.

### Modified Capabilities
- None.

## Impact

- Affects `FormalLevel03.unity`, its instantiated content, and `Level03SourceManifest.md`.
- Does not alter source art, Level 3 visual selection, player ownership, puzzle mechanics, navigation, or exit behavior.
