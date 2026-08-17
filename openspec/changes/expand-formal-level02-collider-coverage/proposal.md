## Why

Level 2 currently has only route-critical collision proxies, leaving most copied visible objects non-interactive for physical testing. It also cannot be launched directly with the formal player actors because they are owned by `FormalPersistent`, which always starts Level 1.

## What Changes

- Add broad Collider coverage to migrated Level 2 architecture, furniture, doors, monsters, and fixed props, while keeping visual-only route hints non-blocking.
- Add an editor-configurable initial formal level so `FormalPersistent` can launch Level 2 with the existing persistent player spawner and game-flow path.
- Preserve the additive ownership model: `FormalLevel02` does not instantiate or own duplicate player actors.

## Capabilities

### New Capabilities
- `formal-level-collider-coverage`: Defines broad physics-collider coverage and non-blocking exceptions for migrated formal-level visuals.
- `formal-level-test-entry`: Defines configurable direct startup of a selected formal level through the persistent player and game-flow scene.

### Modified Capabilities
- None.

## Impact

- Affects `FormalLevel02.unity`, `L02_Content.prefab`, `FormalPersistent.unity`, and formal game-flow configuration.
- Adds no dependencies and does not implement Level 2 puzzle progression, monster behavior, doors, or exit logic.
