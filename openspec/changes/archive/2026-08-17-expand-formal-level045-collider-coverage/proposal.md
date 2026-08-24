## Why

Formal Level 4.5 currently has only floor and boundary collision, leaving most retained corridor walls, doors, partitions, furniture, and fixed obstacles non-solid.

## What Changes

- Add broad scene-owned Collider proxies for Level 4.5 static architecture, doors, walls, partitions, furniture, and substantial fixed props.
- Keep decorative, overhead, small, and mechanic-only visuals non-blocking.
- Preserve validated Level 4.5 respawn anchors and foundational corridor support.

## Capabilities

### New Capabilities
- `formal-level045-collider-coverage`: Defines broad static physical coverage and non-blocking exceptions for Formal Level 4.5.

### Modified Capabilities
- None.

## Impact

- Affects FormalLevel045 scene-owned collision only.
- Does not modify L045 art transforms, source scenes, formal player ownership, or Level 4.5 mechanics.
