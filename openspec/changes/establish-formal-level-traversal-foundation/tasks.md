## 1. Anchor And Reset Semantics

- [x] 1.1 Inventory Level 1 and Level 2 entrance, checkpoint, exit, floor, boundary, and blocking-collider world positions.
- [x] 1.2 Add or configure separate human and dog respawn anchors for each formal checkpoint under active development.
- [x] 1.3 Update formal level reset behavior to use active checkpoint anchors and entrance anchors as the no-checkpoint fallback.
- [x] 1.4 Add focused tests for initial placement, checkpoint activation, reset after checkpoint, and reset before checkpoint.

## 2. Level 1 Traversal Baseline

- [x] 2.1 Validate Level 1 entrance anchors against ground support and blocking overlap using both formal player capsules.
- [x] 2.2 Validate the approved Level 1 entrance-to-checkpoint and checkpoint-to-exit route segments against floor, wall, furniture, gate, and trigger collision.
- [x] 2.3 Record validated Level 1 anchor positions, route segments, and collision exceptions.

## 3. Level 2 Traversal Baseline

- [x] 3.1 Select supported HumanSpawn and DogSpawn positions from the accepted Level 2 art environment and replace the current near-origin defaults.
- [x] 3.2 Add Level 2 floor, boundary, and major fixed-blocker collision required to support its entrance, checkpoint, and provisional exit route.
- [x] 3.3 Add or configure separate Level 2 checkpoint anchors on supported geometry without wiring Level 2 puzzle progression.
- [x] 3.4 Validate human and dog placement plus each approved Level 2 anchor-to-anchor segment for grounding, overlap, blockers, trigger pass-through, and falls.

## 4. Verification And Handoff

- [x] 4.1 Run Unity EditMode checks for anchor support, collider configuration, and reset positions.
- [x] 4.2 Run Unity PlayMode walking checks from entrance and checkpoint anchors for both player actors.
- [x] 4.3 Inspect the Unity Console, verify source-scene preservation, and record anchor and traversal evidence for later Level 1 and Level 2 mechanism changes.
