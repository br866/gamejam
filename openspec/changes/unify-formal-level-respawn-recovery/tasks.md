## 1. Recovery-Path Audit

- [x] 1.1 Inventory the formal player failure, checkpoint activation, level-controller, additive handoff, monster-capture, anxiety-failure, and existing reset paths.
- [x] 1.2 Record each Formal Level 1, Level 2, Level 3, Level 4, Level 4.5, and Level 5 initial/checkpoint `HumanRespawnAnchor` and `DogRespawnAnchor` pair, resettable objects, and permanent-progress owners.
- [x] 1.3 Identify and remove formal-route `HumanSpawn` and `DogSpawn` placement references and legacy checkpoint serialization.
- [x] 1.4 Identify any legacy or per-monster direct respawn path that bypasses the active formal level recovery owner.

## 2. Shared Recovery Lifecycle

- [x] 2.1 Implement one guarded recovery request path for anxiety failure and monster capture that selects the active formal level.
- [x] 2.2 Implement coordinated human/dog placement from the active checkpoint pair, with initial `HumanRespawnAnchor`/`DogRespawnAnchor` fallback before checkpoint activation.
- [x] 2.3 Implement XZ-only anchor placement that finds valid non-trigger ground below each anchor and aligns each actor's feet to that surface; report missing ground as configuration failure.
- [x] 2.4 Define and implement the level-local resettable-state contract while preserving checkpoint and permanent current-level progress.
- [x] 2.5 Restore resettable state and player readiness before input and threat evaluation resume; prevent duplicate concurrent recovery requests.

## 3. Route Handoff Recovery

- [x] 3.1 Ensure a loaded successor becomes the active recovery owner on entry while its predecessor remains retained until successor-checkpoint commitment.
- [ ] 3.2 Verify a failure before successor checkpoint activation returns players to successor initial respawn anchors without unloading or invalidating the predecessor.
- [ ] 3.3 Preserve existing checkpoint-committed predecessor cleanup and clear stale recovery references during the handoff.

## 4. Level Recovery Configuration

- [ ] 4.1 Configure and verify Level 1 checkpoint fallback, temporary key/crate reset, permanent mechanism/door retention, and monster reset behavior.
- [ ] 4.2 Configure and verify Level 2 checkpoint fallback, monster state, temporary route state, and permanent plate/door retention.
- [ ] 4.3 Configure and verify Level 3 checkpoint fallback, ordered-plate temporary state where applicable, and permanent route progress retention.
- [ ] 4.4 Configure and verify Level 4 checkpoint fallback, both monster states, split-route reset state, and permanent route progress retention.
- [ ] 4.5 Configure and verify Level 4.5 entrance/checkpoint recovery and all level-local resettable state.
- [ ] 4.6 Configure and verify Level 5 checkpoint recovery, including the controlled escape monsters, medicine cabinet, escape exit, and control-mode reset group while retaining completed final-room progress.

## 5. Validation

- [ ] 5.1 Add validation that reports missing formal initial/checkpoint respawn pairs, formal `HumanSpawn`/`DogSpawn` placement dependencies, invalid ground below anchor XZ, recovery ownership, and required reset registrations.
- [ ] 5.2 For every formal route stage, run Play Mode checks for anxiety failure and monster capture before and after checkpoint activation.
- [ ] 5.3 For every formal route stage, verify temporary progress resets and permanent checkpoint/mechanism/door progress persists after recovery.
- [ ] 5.4 Test successor-entry recovery before checkpoint commitment and confirm predecessor retention, safe later unload, and clean console output.
- [ ] 5.5 Run the Level 5 controlled-escape failure check and confirm escape state resets without clearing permanent final-room progress.
- [ ] 5.6 Run relevant automated tests and `openspec validate --all`; record the results and any remaining scene-specific blockers.
