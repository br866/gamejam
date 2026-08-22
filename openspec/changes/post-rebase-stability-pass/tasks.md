# Tasks

## 1. Character rig alignment (Humanoid Option A)

- [x] 1.1 Flip player model FBX (`Push_and_Walk_Forward`) to Humanoid; regenerate avatar
- [x] 1.2 Play-mode check: human Walk/Idle deform the skeleton (bone positions change); compare hip height vs pre-change
- [x] 1.3 Flip monster3 Walking FBX to Humanoid; assign generated avatar to `Monster3Model` Animator in FormalLevel02
- [x] 1.4 Play-mode check: monster patrol/chase states deform the skeleton (flight-recorder bone sampling)
- [x] 1.5 Verify Jump/Pull `.anim` clips still animate on the humanoid avatar; fix locally if Pull regresses
- [x] 1.6 Fallback gate: NOT NEEDED - humanoid auto-mapping succeeded (avatar isHuman=True valid=True)

## 2. Crate engagement gating + cabinet variant

- [x] 2.1 Gate `FormalPushableCrate`: kinematic in Awake and on Cancel/reset; dynamic only while engaged
- [x] 2.2 Add `axisMode` enum (Auto/PlusX/MinusX/PlusZ/MinusZ) and `travelLimit` fields; Auto preserves current behavior
- [x] 2.3 Play-mode test scene verification: disengaged crate immovable under body contact; engaged push works; wall still blocks with IsBlocked set
- [x] 2.4 Regression check: L01 rail mover path unaffected (FormalPlayerControl dual-type search)

## 3. Scale normalization batch tooling

- [ ] 3.1 Extend `FormalScaleNormalizer` with per-scene JSON journal (inventory, per-node records, skipped nodes)
- [ ] 3.2 Implement rollback command restoring latest journal state and deleting created baked assets
- [ ] 3.3 Run batch on CrateMechanicsTest; verify world bounds unchanged and rollback round-trips
- [ ] 3.4 Run batch on FormalLevel045 (emptiest formal scene); play-check traversal unaffected
- [ ] 3.5 Batch remaining formal scenes one at a time (L01→L05, shared art scenes last), verifying each before proceeding
- [ ] 3.6 Prefab-level pass on SharedModels props with scale overrides inventoried first; review divergent instances

## 4. L05 physical separation

- [ ] 4.1 Author `L05_InteriorWalls` whitebox colliders splitting corridor/final hall and right/left rooms around existing door positions
- [ ] 4.2 Flood-fill connectivity probe for both L05 monsters' graphs after walls; adjust nav areas or patrol waypoints if pockets appear
- [ ] 4.3 Play-mode route check: doors gate passages; full L05 chain (checkpoint → cabinet → right plate → left plate → route complete) still passes

## 5. Closing

- [ ] 5.1 Full regression suite (20 tests) green
- [ ] 5.2 Update tasks.md checkboxes as work proceeds; commit per milestone
