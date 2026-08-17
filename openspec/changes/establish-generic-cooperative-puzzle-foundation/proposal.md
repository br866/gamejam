## Why

Level-specific plans repeatedly need character-restricted triggers, cooperative occupancy, prerequisite-gated route progression, resettable gates, and enemies constrained outside safe spaces. The current components cover fragments of these behaviors but encode role checks inconsistently or rely on raw collider counts, making each new level require bespoke logic.

## What Changes

- Add a reusable character-eligibility contract for trigger-driven puzzle components so triggers can accept the human, dog, either character, or both required characters explicitly.
- Add reusable progression prerequisites and reset behavior for gates, plates, checkpoints, and exits without embedding a specific level's object references or ordering.
- Define a reusable bounded-enemy navigation and safe-zone contract that keeps enemies within an assigned traversable region and prevents capture inside configured safe zones.
- Add component-level validation coverage for missing references, role eligibility, cooperative occupancy, reset transitions, and safe-zone enforcement.
- Keep the existing `implement-formal-level02-mechanics` change as a level-specific consumer that supplies scene objects, world bounds, and route ordering after the generic layer is available.

## Capabilities

### New Capabilities
- `character-restricted-triggers`: Reusable role-aware trigger eligibility and cooperative occupancy behavior for puzzle interactions.
- `resettable-route-progression`: Reusable prerequisite-gated route state, gate control, checkpoint availability, and reset behavior.
- `bounded-enemy-safe-zones`: Reusable enemy movement and capture boundaries that preserve configured player safe zones.

### Modified Capabilities
- None.

## Impact

- Affects reusable puzzle, player-role, checkpoint, gate, monster, and navigation scripts under `Assets/MoMing/Scripts/` plus focused Unity tests.
- Does not configure `FormalLevel02`, migrate art, alter controls, or implement a specific level route.
- Provides configuration contracts consumed later by `implement-formal-level02-mechanics` and subsequent levels.
