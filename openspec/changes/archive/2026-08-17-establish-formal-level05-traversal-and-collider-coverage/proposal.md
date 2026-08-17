## Why

Formal Level 5 is currently art-only, so persistent formal players have no supported entry point and can pass through retained walls and fixed environment objects.

## What Changes

- Add separate human and dog Level 5 entry/respawn anchors on supported collision.
- Add foundational floor and boundary collision for the selected Level 5 area.
- Add broad scene-owned static Collider proxies for valid walls, doors, partitions, furniture, and substantial fixed props.
- Keep visual-only and mechanic-only content non-blocking.

## Capabilities

### New Capabilities
- `formal-level05-traversal-foundation`: Defines grounded two-character entry and foundational physical support for Formal Level 5.
- `formal-level05-collider-coverage`: Defines broad static collision and non-blocking exceptions for Formal Level 5.

### Modified Capabilities
- None.

## Impact

- Affects FormalLevel05 scene-owned collision and anchor objects.
- Does not modify L05_Content Renderer transforms, player ownership, gameplay mechanics, or exit behavior.
