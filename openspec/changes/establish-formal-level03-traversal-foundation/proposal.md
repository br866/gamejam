## Why

Formal Level 3 now has an audited visual assembly but no floor collision, player anchors, or route validation. Without a supported entrance and basic physical boundaries, the persistent formal player pair cannot safely test the new level.

## What Changes

- Define supported HumanSpawn, DogSpawn, checkpoint, and provisional exit anchors for Formal Level 3.
- Add scene-owned floor, boundary, wall, and substantial fixed-prop collision required for basic traversal.
- Add direct FormalPersistent startup verification for Formal Level 3 using the existing persistent player pair.
- Validate both player capsules at anchors and across approved baseline route segments.
- Keep Level 3 mechanisms, monster behavior, navigation, UI, and exit progression out of scope.

## Capabilities

### New Capabilities
- `formal-level03-traversal-anchors`: Defines supported Level 3 entrance, checkpoint, and provisional exit anchors.
- `formal-level03-walk-validation`: Defines Level 3 grounding, collision, and baseline route checks.

### Modified Capabilities
- None.

## Impact

- Affects `FormalLevel03.unity`, `L03_Content.prefab`, `FormalPersistent.unity`, and the Level 3 manifest.
- Reuses the already validated formal checkpoint/reset and persistent player-flow systems.
- Does not modify the source prototype scene or formal Level 1/2 art and mechanics.
