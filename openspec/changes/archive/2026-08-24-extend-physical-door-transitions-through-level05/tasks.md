## 1. Generic physical-exit routing

- [x] 1.1 Extend the scene-owned physical exit policy so matching actuator and crate-door exits request successor preload and shared-door opening instead of direct route advancement.
- [x] 1.2 Keep unmatched exits on their existing direct behavior and preserve GM direct transitions as a separate immediate path that cancels pending physical traversal.
- [x] 1.3 Make retained-predecessor arrival confirmation complete the entry seal without closing or unloading the retained predecessor.

## 2. Level configuration

- [x] 2.1 Add a scene-owned Level 4 physical-exit policy targeting `FormalLevel045`; do not modify Prefab assets.
- [x] 2.2 Add a scene-owned two-player Level 4.5 entry seal that confirms arrival from Level 4 while retaining Level 4 for pursuit.
- [x] 2.3 Add a scene-owned Level 4.5 physical-exit policy targeting `FormalLevel05`, covering its actuator and crate-door exit paths without modifying Prefab assets.
- [x] 2.4 Release the retained Level 4 scene, but not Level 4.5, when `L05_Checkpoint` activates during the pending L4.5-to-L5 physical transition.
- [x] 2.5 Commit Level 5 recovery at `L05_Checkpoint` without repositioning players, and preserve or restore retained Level 4 during Level 4.5 recovery.

## 3. Verification

- [x] 3.1 Add focused edit-mode coverage for Level 4 and Level 4.5 policy resolution, including the crate-door route-producing exit.
- [x] 3.2 Add coverage for retained Level 4 arrival confirmation, L05 checkpoint recovery commit and retained-Level-4 cleanup, Level 4.5 recovery preservation, duplicate exit requests, and GM interruption.
- [x] 3.3 Run the focused tests and relevant OpenSpec validation, then verify both routes in Play Mode: exit completion opens and preloads, neither player teleports, Level 4.5 recovery retains Level 4, and L05_Checkpoint unloads only Level 4 while establishing L5 recovery.
