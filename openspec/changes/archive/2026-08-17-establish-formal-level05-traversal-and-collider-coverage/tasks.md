## 1. Anchor And Collider Audit

- [x] 1.1 Inventory Level 5 renderer bounds and classify physical obstacles versus visual-only content.
- [x] 1.2 Add foundational floor and boundary collision for the Level 5 area.
- [x] 1.3 Add separate human and dog entry/respawn anchors in a clear supported region.
- [x] 1.4 Add scene-owned static collision proxies for valid architecture and substantial fixed props.
- [x] 1.5 Keep visual-only, overhead, small, player/monster, and mechanic visuals non-blocking.

## 2. Validation And Handoff

- [x] 2.1 Validate both anchors for floor support, player-capsule overlap, and immediate movement clearance.
- [x] 2.2 Direct-load FormalLevel05 through FormalPersistent and verify exactly one grounded human/dog pair. (Runtime verification confirmed.)
- [x] 2.3 Keep FormalLevel05 as the active direct-test startup scene and record collision coverage and exclusions.
