## 1. Level-Bound Checkpoint Registration

- [x] 1.1 Add explicit owning-level configuration and per-loaded-level one-time registration for every `FormalCheckpoint`.
- [x] 1.2 Configure all formal checkpoint instances with their containing formal level and validate missing or mismatched ownership.
- [x] 1.3 Keep checkpoint activation limited to local checkpoint/respawn state; remove its route-advance and transition-door side effects.

## 2. Level 2 Cooperative Door Regression

- [x] 2.1 Clear the Level 2 pedal and safe zone's direct route-advance configuration, then attach and configure the safe-zone E interaction with pedal and safe-zone prerequisites.
- [ ] 2.2 Boot directly into `FormalLevel02` and verify initial `SuccessorCheckpoint` overlap neither advances the route nor opens `ToLevel03`.
- [ ] 2.3 Verify checkpoint re-entry remains non-progressing while its owning Level 2 registration is active.
- [ ] 2.4 Verify only pressing the pedal, placing both players in `L02_CooperativeSafeZoneTrigger`, then using E opens the L2-to-L3 door and begins transition.

## 3. Verification

- [x] 3.1 Add or update EditMode coverage for level ownership, one-time registration, and checkpoint non-progression.
- [ ] 3.2 Run relevant Unity checks, inspect the Console for errors, and validate the OpenSpec change.
