## 0. Scope And Evidence

- [x] 0.1 Record the six-level common/business capability matrix and boundary rules in `capability-matrix.md`.
- [ ] 0.2 Record Level 03, Level 04, Level 04.5, and Level 05 business relationships before implementing their specific mechanics.

## 1. Formal Level Runtime Management

- [x] 1.1 Add a serialized formal route catalog with stable level ids, scene names, shared additive scene names, and route ordering, replacing the current adjacent-level hardcoding.
- [ ] 1.2 Implement synchronous and callback-based asynchronous load/unload APIs for named formal levels with one ownership reconciliation path. The current public synchronous methods still start coroutines and need a Unity-safe synchronous completion path.
- [x] 1.3 Implement next-level, previous-level, and direct-jump navigation, retaining only the target level after a transition and unloading all other loaded formal levels.
- [x] 1.4 Add development keyboard commands: keypad 2 loads the next level, keypad 8 loads the previous level, keypad 5 resets the current level, and keypad 6 opens doors in the current level plus its shared scene with the next level. Inspector commands and direct configured-jump shortcuts remain deferred.
- [x] 1.5 Retain shared additive art scenes by explicit route references and unload them only when their reference count reaches zero.

## 2. Formal Level Contract

- [ ] 2.1 Define the required formal scene contract in runtime-facing constants or helpers without creating a second global manager.
- [ ] 2.2 Update `FormalLevelController` and related state registration so resettable scene-local state resets while permanent scene-local progress remains complete.
- [ ] 2.3 Ensure formal exits, checkpoints, and additive flow preserve the specified predecessor-retention and successor-commit lifecycle.
- [ ] 2.4 Align all six formal route scenes with the required controller, paired entrance anchors, visual content root, and collision root contract without altering art placement.

## 3. Reusable Formal Mechanics

- [x] 3.1 Add a formal trigger eligibility component or helper for either-player, human-only, dog-only, and explicit formal physics-occupant policies.
- [x] 3.2 Add reusable formal mechanism state behavior for permanent and resettable completion policies, registered with the owning level lifecycle.
- [x] 3.3 Extend the formal door actuator with explicit reversible close/reset behavior, including collider and visual restoration.
- [x] 3.4 Add an explicit formal marker or contract for resettable pushable physics occupants.

## 4. Common Consumer Proof

- [x] 4.0 Convert the two Level 01 formal door prefabs to pivot-door structures; leave door frames and static architecture outside the movable door leaf prefab.
- [ ] 4.1 Migrate the Level 01 key to the common human-only permanent interaction contract while preserving collection and exit-door behavior.
- [ ] 4.2 Migrate the Level 01 pedal to the common human-only permanent interaction contract while preserving its door behavior.
- [ ] 4.3 Migrate the Level 01 crate to the common resettable physics-occupant contract while preserving push and reset behavior.
- [ ] 4.4 Configure Level 02 as the second consumer of common role-aware triggers, progression state, actuators, reset, checkpoint, and exit contracts without putting its route graph into common code.
- [ ] 4.5 Wire Level 01 and Level 02 scene references to the common formal components, preserving existing visual and collision assets.

## 5. Contract Validation And Verification

- [ ] 5.1 Extend the formal editor validation suite with the authoritative route catalog, shared-scene references, and required-contract checks.
- [ ] 5.2 Add validation for duplicate formal player actors, entrance support, and blocked entrance capsule space while retaining checkpoint-anchor checks.
- [ ] 5.3 Add edit-mode tests for trigger eligibility, permanent/resettable state policy, reversible doors, Level 01 crate reset, and shared-scene retention.
- [ ] 5.4 Run the formal traversal and contract validation tests, then manually verify GM navigation, async callbacks, shared-art retention, Level 01 key, pedal, reset, checkpoint, and successor transition behavior in the Unity Editor.
