## 1. Route Lifecycle And Startup

- [x] 1.1 Audit the current formal route catalog, scene identities, build settings, startup menu target, exits, checkpoints, and persistent-scene references; record every mismatch before modifying runtime behavior.
- [x] 1.2 Make `FormalPersistent` the first enabled build scene so it loads Formal Level 1 directly, while preserving the legacy prototype route as disabled reference material.
- [x] 1.3 Restore checkpoint-committed predecessor retention for gameplay exits, while keeping explicitly destructive development fast travel separate from normal handoff behavior.
- [x] 1.4 Add a formal final-completion presentation with a deterministic restart or return path after Level 5.
- [ ] 1.5 Add focused lifecycle tests for startup, successor loading, reset before successor checkpoint, checkpoint commit, predecessor unload, shared-art reconciliation, and final completion.

## 2. Formal Scene Contract And Validation

- [ ] 2.1 Normalize every formal scene's unique route identity, paired entrance anchors, content root, collision root, checkpoint ownership, and configured exit/final state without changing approved art placement.
- [x] 2.2 Validate route-catalog uniqueness, scene/build registration, shared-art membership, missing required contract objects, duplicate formal actors, unsafe spawn support, and blocked spawn capsule space for all six levels.
- [ ] 2.3 Validate exit prerequisites and checkpoint-to-successor relationships so no route transition depends on a blank or unregistered scene.
- [x] 2.4 Run the full formal contract EditMode suite and record its results before level gameplay wiring begins.

## 3. Shared Cooperative Mechanics

- [x] 3.1 Complete formal role eligibility and unique occupancy support for human-only, dog-only, either-player, both-player, and approved physics occupants.
- [x] 3.2 Add reusable prerequisite-gated, ordered, permanent, and resettable route progression behavior for mechanisms, doors, checkpoints, and exits.
- [ ] 3.3 Extend formal doors and exits to require configured progression state, preserve permanent open state, and reset only resettable route states deterministically.
- [x] 3.4 Add bounded monster hostile-region, safe-zone, pursuit, navigation, and capture behavior that uses one consistent scene configuration.
- [ ] 3.5 Add focused EditMode tests for eligibility, duplicate collider occupancy, ordered progression, reset stages, prerequisite gates, door/exit state, monster safe zones, and pursuit through ordinary cover.

## 4. Level 1 And Level 2 Playable Proof

- [x] 4.1 Migrate and wire Level 1's human crate step, human-only key, permanent mechanism door, permanent exit door, checkpoint, reset behavior, exit, and Level 2 handoff to the shared contracts.
- [ ] 4.2 Verify the Level 1 dog cannot collect the key or bypass the intended human-only crate/key route.
- [ ] 4.3 Configure Level 2's dog-only footprints and first plate, cooperative second plate, route gate, monster region, safe space, checkpoint, exit, and Level 3 handoff.
- [ ] 4.4 Verify both levels in Play Mode from their real predecessor/entry state, including death before and after each checkpoint, permanent door state, and retained-predecessor transitions.

## 5. Level 3 Through Level 4.5 Progression

- [ ] 5.1 Record the approved Level 3 object-to-behavior mapping, then configure its completion hint, central cooperative plate, ordered dog plates, permanent final door, checkpoint, exit, and Level 4 handoff.
- [ ] 5.2 Record the approved Level 4 object-to-behavior mapping, then configure both monsters, dog-only first plate, human-only second plate, reunion plate, permanent exit door, checkpoint, exit, and Level 4.5 handoff.
- [ ] 5.3 Configure Level 4.5 as a complete route bridge with its checkpoint, reset behavior, exit prerequisite, and Level 5 handoff.
- [ ] 5.4 Playtest Level 3, Level 4, and Level 4.5 in sequence, validating role restrictions, ordered/cooperative completion, monster safe behavior, checkpoints, resets, and checkpoint-committed scene unloads.

## 6. Level 5 Escape And Final Completion

- [ ] 6.1 Resolve the concrete Level 5 checkpoint placement, fixed camera composition, cabinet-area safety decision, and final completion presentation against the approved formal layout; update this change's planning artifacts if the resolution changes scope.
- [ ] 6.2 Configure the controlled corridor mode: switching and voluntary separation restrictions, fixed camera, running ability, two-monster pursuit, cabinet pushing, corridor exit, checkpoint, and deterministic escape reset.
- [ ] 6.3 Configure the final-room right and left cooperative plates, their permanent doors, final door, and final-completion trigger.
- [ ] 6.4 Run Level 5 Play Mode verification for normal escape, caught-player reset, cabinet traversal, controlled-mode release, final-room ordering, and final completion/restart behavior.

## 7. Route Acceptance

- [ ] 7.1 Run all formal EditMode validation and focused mechanic tests; resolve Console errors and route-related warnings.
- [ ] 7.2 Run a clean Play Mode route from the build's first scene through final completion without keypad commands, direct jumps, or scene-open shortcuts.
- [ ] 7.3 Build the configured target platform and smoke-test startup, a representative transition, final completion, restart/return, and application shutdown behavior.
- [ ] 7.4 Record route acceptance evidence, known deferred presentation work, and regression coverage in the change before requesting archive verification.
