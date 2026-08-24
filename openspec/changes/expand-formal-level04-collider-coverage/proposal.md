## Why

Formal Level 4 currently has only a foundational floor and outer boundaries, so retained walls, gates, furniture, and fixed props can be walked through. It needs practical static collision coverage without changing the user-curated visual layout.

## What Changes

- Add scene-owned Collider proxies for valid retained Level 4 architecture, gates, doors, furniture, and substantial fixed props.
- Keep visual-only hints, small decoration, player/monster display meshes, and known cross-level Plate visuals non-blocking.
- Preserve current Level 4 entry anchors, floor support, boundary collision, and direct-test startup configuration.

## Capabilities

### New Capabilities
- `formal-level04-collider-coverage`: Defines broad static physical coverage and non-blocking exceptions for Formal Level 4.

### Modified Capabilities
- None.

## Impact

- Affects FormalLevel04 scene-owned collision only.
- Does not modify L04_Content Renderer transforms, source scenes, player controls, or Level 4 mechanics.
