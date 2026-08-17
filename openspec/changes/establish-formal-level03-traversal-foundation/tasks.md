## 1. Anchors And Collision

- [x] 1.1 Inventory Level 3 floor, wall, boundary, and major fixed-prop world bounds.
- [x] 1.2 Add supported HumanSpawn, DogSpawn, checkpoint, and provisional exit anchors.
- [x] 1.3 Add Level 3 floor, boundary, wall, and major fixed-blocker collision without adding player actors.
- [x] 1.4 Configure the Level 3 checkpoint to use separate human and dog respawn anchors.

## 2. Validation

- [x] 2.1 Validate all anchors for floor support and blocking overlap with both formal player capsules.
- [x] 2.2 Validate entrance-to-checkpoint and checkpoint-to-provisional-exit routes for both actors.
- [x] 2.3 Verify visual-only objects and unintended props do not block the approved baseline route.

## 3. Direct Test And Handoff

- [x] 3.1 Test Formal Level 3 by setting `FormalPersistent` initial scene to exact name `FormalLevel03`.
- [x] 3.2 Verify exactly one grounded human/dog actor pair after additive Level 3 load.
- [x] 3.3 Restore the normal startup default, inspect Console, preserve the source scene, and record evidence in `Level03SourceManifest.md`.
