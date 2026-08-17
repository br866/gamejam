## 1. Formal Level Runtime Management

- [ ] 1.1 Add a serialized formal route catalog with stable level ids, scene names, shared additive scene names, and route ordering.
- [ ] 1.2 Implement synchronous and callback-based asynchronous load/unload APIs for named formal levels with one ownership reconciliation path.
- [ ] 1.3 Implement next-level, previous-level, and direct-jump navigation, retaining predecessor and successor during transitions.
- [ ] 1.4 Add Inspector GM commands and development-only keyboard commands for next, previous, direct configured jump, load, unload, and reload operations.
- [ ] 1.5 Retain shared additive art scenes by explicit route references and unload them only when their reference count reaches zero.

## 2. Formal Level Contract

- [ ] 2.1 Define the required formal scene contract in runtime-facing constants or helpers without creating a second global manager.
- [ ] 2.2 Update `FormalLevelController` and related state registration so resettable scene-local state resets while permanent scene-local progress remains complete.
- [ ] 2.3 Ensure formal exits, checkpoints, and additive flow preserve the specified predecessor-retention and successor-commit lifecycle.
- [ ] 2.4 Align all six formal route scenes with the required controller, paired entrance anchors, visual content root, and collision root contract without altering art placement.

## 3. Reusable Formal Mechanics

- [ ] 3.1 Add a formal trigger eligibility component or helper for either-player, human-only, dog-only, and explicit formal physics-occupant policies.
- [ ] 3.2 Add reusable formal mechanism state behavior for permanent and resettable completion policies, registered with the owning level lifecycle.
- [ ] 3.3 Extend the formal door actuator with explicit reversible close/reset behavior, including collider and visual restoration.
- [ ] 3.4 Add an explicit formal marker or contract for resettable pushable physics occupants.

## 4. Level 01 Migration

- [ ] 4.1 Migrate the Level 01 key to the common human-only permanent interaction contract while preserving collection and exit-door behavior.
- [ ] 4.2 Migrate the Level 01 pedal to the common human-only permanent interaction contract while preserving its door behavior.
- [ ] 4.3 Migrate the Level 01 crate to the common resettable physics-occupant contract while preserving push and reset behavior.
- [ ] 4.4 Wire Level 01 prefabs and scene references to the common formal components, preserving existing visual and collision assets.

## 5. Contract Validation And Verification

- [ ] 5.1 Extend the formal editor validation suite with the authoritative route catalog, shared-scene references, and required-contract checks.
- [ ] 5.2 Add validation for duplicate formal player actors, entrance support, and blocked entrance capsule space while retaining checkpoint-anchor checks.
- [ ] 5.3 Add edit-mode tests for trigger eligibility, permanent/resettable state policy, reversible doors, Level 01 crate reset, and shared-scene retention.
- [ ] 5.4 Run the formal traversal and contract validation tests, then manually verify GM navigation, async callbacks, shared-art retention, Level 01 key, pedal, reset, checkpoint, and successor transition behavior in the Unity Editor.
