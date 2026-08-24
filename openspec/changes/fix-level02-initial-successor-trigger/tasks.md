## 1. Level 2 Initial Placement

- [ ] 1.1 Move the two `FormalLevel02` initial respawn anchors to valid entrance positions outside the successor checkpoint trigger volume.
- [ ] 1.2 Preserve the successor checkpoint transform, registration setting, and empty prerequisite behavior for intentional completion.

## 2. Verification

- [ ] 2.1 Start the persistent route directly at `FormalLevel02` and verify neither player activates `SuccessorCheckpoint` or opens `ToLevel03` on load.
- [ ] 2.2 Verify that intentionally entering the Level 2 successor checkpoint still requests the Level 2 to Level 3 transition.
