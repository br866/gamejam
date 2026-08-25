# Tasks: route-checkpoint-audio

## 1. Runtime

- [x] 1.1 Post `Play_CheckpointSFX` only after a checkpoint successfully commits.
- [x] 1.2 Prevent repeated trigger entries from replaying the one-shot.

## 2. Scene Configuration

- [x] 2.1 Bind the Event on the Level 2 checkpoint prefab.
- [x] 2.2 Bind the Event on the Level 3, 4, 4.5, and 5 checkpoint objects.
- [x] 2.3 Preserve Level 1's initial-spawn fallback without inventing a new carpet.

## 3. Verification

- [x] 3.1 Verify all five placed formal checkpoint carpets reference the Event.
- [ ] 3.2 Verify scripts compile and existing checkpoint tests still pass.
