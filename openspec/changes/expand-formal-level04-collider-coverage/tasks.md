## 1. Collider Audit And Coverage

- [x] 1.1 Inventory all Level 4 content Renderers by world bounds and classify blocking versus visual-only.
- [x] 1.2 Add scene-owned Collider proxies for valid static architecture, gates, doors, furniture, and substantial fixed props.
- [x] 1.3 Keep cross-level Plates, player/monster visuals, hints, pads, and small decoration non-blocking.
- [x] 1.4 Preserve the existing floor, boundaries, and entry anchors.

## 2. Validation And Handoff

- [x] 2.1 Validate human and dog entry anchors for floor support, overlap, and immediate movement clearance.
- [x] 2.2 Direct-load FormalLevel04 through FormalPersistent and verify one grounded human/dog pair.
- [x] 2.3 Keep FormalLevel04 as the direct-test startup scene and record final coverage and exclusions.
