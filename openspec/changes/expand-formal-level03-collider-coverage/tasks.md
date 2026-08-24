## 1. Collider Inventory And Coverage

- [x] 1.1 Inventory the manually retained Level 3 renderers and classify blocking versus visual-only objects.
- [x] 1.2 Add or enable appropriate Collider coverage for retained static architecture, walls, doors, furniture, and substantial fixed props.
- [x] 1.3 Keep visual guidance, particle effects, and small non-obstructive decoration non-blocking.
- [x] 1.4 Preserve the existing Level 3 floor, boundary, anchor, and route collision volumes while expanding coverage.

## 2. Direct Formal Level Test Entry

- [x] 2.1 Configure Formal Level 3 as the initial scene only for an editor Play Mode verification without adding player actors to Level 3.
- [x] 2.2 Restore Formal Level 1 as the persisted default initial scene after direct-entry verification.

## 3. Validation And Handoff

- [x] 3.1 Validate human and dog entrance and checkpoint anchors for support and blocking overlap after collider coverage expands.
- [x] 3.2 Validate entrance-to-checkpoint and checkpoint-to-provisional-exit traversal for both actors after coverage expands.
- [x] 3.3 Enter Play Mode through FormalPersistent with Level 3 selected and verify exactly one grounded human/dog actor pair is spawned.
- [x] 3.4 Inspect the Unity Console, preserve the source art scene, and record collider coverage, exclusions, and direct-test procedure.
