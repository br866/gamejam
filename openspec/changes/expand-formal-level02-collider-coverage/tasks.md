## 1. Collider Inventory And Coverage

- [x] 1.1 Inventory all migrated Level 2 Renderers and classify each as blocking or visual-only.
- [x] 1.2 Add or enable appropriate Collider coverage for static architecture, walls, doors, monsters, furniture, and substantial fixed props.
- [x] 1.3 Keep footprint markers, particle effects, and other visual-only objects non-blocking.
- [x] 1.4 Preserve existing Level 2 floor, boundary, anchor, and route collision volumes while applying broad coverage.

## 2. Direct Formal Level Test Entry

- [x] 2.1 Confirm `FormalPersistent` exposes its initial formal level as serialized editor configuration.
- [x] 2.2 Configure Formal Level 2 as the initial scene for an editor Play Mode verification without adding player actors to Level 2.
- [x] 2.3 Restore Formal Level 1 as the persisted default initial scene after direct-entry verification.

## 3. Validation And Handoff

- [x] 3.1 Validate human and dog entrance and checkpoint anchors for support and blocking overlap after collider coverage expands.
- [x] 3.2 Validate direct entrance-to-checkpoint and checkpoint-to-provisional-exit traversal for both actors, including non-blocking visual hints.
- [x] 3.3 Enter Play Mode through FormalPersistent with Level 2 selected and verify exactly one grounded human/dog actor pair is spawned.
- [x] 3.4 Inspect the Unity Console, preserve the source art scene, and record collider coverage, exclusions, and the direct-test procedure.
