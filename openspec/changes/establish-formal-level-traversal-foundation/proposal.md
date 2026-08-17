## Why

Formal levels cannot be meaningfully tested for collision or player walking until the human and dog begin from verified, supported world positions and reset to verified checkpoint positions. Formal Level 2 currently spawns below its accepted art environment, while the existing formal checkpoint logic resets players to their entrance spawns instead of checkpoint-specific anchors.

## What Changes

- Define explicit human and dog entrance, checkpoint, and exit anchors for each formal level under active development.
- Correct formal checkpoint storage so reset uses the configured checkpoint anchors rather than the original entrance anchors.
- Add grounded-spawn, overlap, blocker, trigger, and route-segment validation for formal player actors starting from entrance or checkpoint anchors.
- Add Level 1 and Level 2 collision foundations required to support their verified anchors and basic walking routes.
- Keep puzzle progression, monster behavior, character-specific triggers, and later-level business logic out of scope.

## Capabilities

### New Capabilities
- `formal-level-traversal-anchors`: Defines explicit two-character entrance, checkpoint, and exit anchors for formal levels and their reset semantics.
- `formal-level-walk-validation`: Defines the collision and traversal checks that prove both formal player actors can stand and walk between approved anchors.

### Modified Capabilities
- None.

## Impact

- Affects `Assets/MoMing/FormalLevels/FormalLevel01.unity`, `FormalLevel02.unity`, formal level runtime scripts, and focused Unity tests or validation tooling.
- Does not change source art scenes, Level 2 puzzle business logic, monster behavior, or game controls.
- Establishes the safe starting point for later per-level gameplay work and broader art migration.
