## 1. Formal Scene Foundation

- [x] 1.1 Create the persistent-scene ownership boundary for players, camera, UI, audio, and global game flow.
- [x] 1.2 Create an additive Level 1 scene with a level controller, spawn points, checkpoint configuration, gameplay root, and level-content root.
- [ ] 1.3 Define and verify the handoff contract that keeps Level 1 loaded until a successor-level checkpoint is active, then clears persistent references before Level 1 unloads.

## 2. Level 1 Art Assembly

- [x] 2.1 Build the Level 1 architecture prefab from the selected structural art, using semantic formal names and preserving source traceability without retaining legacy hierarchy names.
- [x] 2.2 Build Level 1 set-dressing prefab groups from the selected decorative art, including the two stools, cabinets, medical cabinets, and metal lockers.
- [x] 2.3 Create independent Level 1 gameplay prefabs for `door5 (1)`, `door4 (1)`, `wooden_crate (1)`, `Pedal1 (1)`, and the selected checkpoint visual.
- [x] 2.4 Exclude the selected prototype `Gate` objects and their controller configuration, `Pedal2 (1)`, `PlayerSystem`, `boy`, `dog`, and `Level3/door2` from the Level 1 formal-content migration.

## 3. Level 1 Collision and Navigation

- [ ] 3.1 Author art-aligned primitive or compound collision for Level 1 floors, walls, the broken-wall route, and both doorways.
- [ ] 3.2 Add blocking collision to the selected decorative stools, cabinets, medical cabinets, and metal lockers; leave small decorative props non-blocking unless playtesting identifies a traversal need.
- [ ] 3.3 Configure the wooden crate as a human-pushable temporary physics object with stable collision, reset anchor, and no non-convex dynamic mesh collider.
- [ ] 3.4 Bake or author Level 1 monster navigation against the formal collision proxies and validate the intended player and monster access boundaries.

## 4. Level 1 Gameplay Vertical Slice

- [ ] 4.1 Implement the human-only crate-to-broken-wall step flow and verify the dog cannot use the resulting route to reach the key area.
- [ ] 4.2 Implement human-only key collection and configure the key as temporary progress that resets after death.
- [ ] 4.3 Configure `door5 (1)` as the permanent mechanism-controlled door and `door4 (1)` as the permanent key-controlled exit door.
- [ ] 4.4 Configure `Pedal1 (1)` as the formal mechanism visual with its explicit interaction boundary and required actor eligibility.
- [ ] 4.5 Configure the Level 1 checkpoint so either character establishes the shared respawn position and it persists through death.
- [ ] 4.6 Verify death resets temporary Level 1 state while preserving the checkpoint, completed mechanism, and opened doors.

## 5. Lifecycle Validation

- [ ] 5.1 Additively load a successor-level test scene while Level 1 remains active and verify players can still recover to the Level 1 checkpoint before successor checkpoint activation.
- [ ] 5.2 Activate the successor checkpoint, transfer or clear all persistent references to Level 1 objects, and unload the Level 1 scene without destroying players, camera, UI, or audio.
- [ ] 5.3 Playtest the full Level 1 vertical slice for collision alignment, crate placement stability, human and dog route eligibility, door persistence, checkpoint reset behavior, and clean scene unloading.

## 6. Deferred Scope

- [ ] 6.1 Record any future dog cooperation for the Level 1 crate puzzle as a separate design change; do not add it to this vertical slice.
- [ ] 6.2 Defer Level 2 through Level 5 art migration and all unresolved Level 5 checkpoint, safe-space, camera, and completion decisions to subsequent scoped work.
