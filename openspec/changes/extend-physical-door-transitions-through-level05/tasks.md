## 1. Generic physical-exit routing

- [ ] 1.1 Extend the scene-owned physical exit policy so matching actuator and crate-door exits request successor preload and shared-door opening instead of direct route advancement.
- [ ] 1.2 Keep unmatched exits on their existing direct behavior and preserve GM direct transitions as a separate immediate path that cancels pending physical traversal.
- [ ] 1.3 Make retained-predecessor arrival confirmation complete the entry seal without closing or unloading the retained predecessor.

## 2. Level configuration

- [ ] 2.1 Add a scene-owned Level 4 physical-exit policy targeting `FormalLevel045`; do not modify Prefab assets.
- [ ] 2.2 Add a scene-owned two-player Level 4.5 entry seal that confirms arrival from Level 4 while retaining Level 4 for pursuit.
- [ ] 2.3 Add a scene-owned Level 4.5 physical-exit policy targeting `FormalLevel05`, covering its actuator and crate-door exit paths without modifying Prefab assets.

## 3. Verification

- [ ] 3.1 Add focused edit-mode coverage for Level 4 and Level 4.5 policy resolution, including the crate-door route-producing exit.
- [ ] 3.2 Add coverage for retained Level 4 arrival confirmation, Level 5 physical arrival cleanup, duplicate exit requests, and GM interruption.
- [ ] 3.3 Run the focused tests and relevant OpenSpec validation, then verify both routes in Play Mode: exit completion opens and preloads, neither player teleports, and both-player crossing commits the successor.
