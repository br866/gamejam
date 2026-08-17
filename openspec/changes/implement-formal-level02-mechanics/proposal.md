## Why

Formal Level 2 now has an accepted visual layout and retains its prototype monster, gate, pressure markers, and footprint markers. These objects are not yet a verified playable route: their ownership, collision, navigation, character restrictions, progression, and reset behavior must be configured against the formal scene.

## What Changes

- Configure the retained Level 2 monster to patrol and chase only within its assigned room and to exclude the exit-side safe space.
- Configure the static footprint route so it is visible only while the dog is active and leads to a dog-only first pressure plate.
- Configure the first plate, cooperative second plate, gate state, checkpoint, and successor handoff as one Level 2 progression route.
- Validate environment collision, player traversal, monster navigation, and reset behavior in `FormalLevel02` without changing the source reference scene.
- Remove or replace retained prototype components only when they cannot satisfy the accepted Level 2 behavior.

## Capabilities

### New Capabilities
- `formal-level02-mechanics`: Defines the playable Level 2 route, including dog-only clues and first plate, monster-safe-space boundaries, cooperative progression, checkpoint, and level handoff.

### Modified Capabilities
- None.

## Impact

- Affects `Assets/MoMing/FormalLevels/FormalLevel02.unity`, the Level 2 content prefab and manifest, and existing monster, puzzle, checkpoint, and navigation components as needed.
- Reads `Assets/Scenes/Test/superbreadman 1.unity` only as a reference; it must remain unchanged.
- Requires Unity play-mode verification of both character routes, plate activation, monster boundaries, reset, checkpoint, and successor loading.
