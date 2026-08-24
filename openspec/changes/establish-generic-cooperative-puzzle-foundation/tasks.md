## 1. Role-Aware Trigger Foundation

- [ ] 1.1 Add a reusable player-role resolver that maps root and child colliders to human or dog identity without depending on a specific scene.
- [ ] 1.2 Extend or replace generic pressure-trigger occupancy so it supports human-only, dog-only, either-role, and both-role requirements without duplicate collider counts.
- [ ] 1.3 Add focused EditMode tests for eligibility, child colliders, duplicate colliders, enter/exit, and destroyed occupants.

## 2. Resettable Route Progression

- [ ] 2.1 Add reusable prerequisite and completion state that can gate a route object without hard-coding level object references.
- [ ] 2.2 Integrate reset registration with existing gate and trigger behavior so occupants, completion, dependent availability, and gate state reset deterministically.
- [ ] 2.3 Add focused tests for incomplete prerequisites, persistent non-continuous completion, continuous completion, and reset from each progression state.

## 3. Bounded Enemy Safety

- [ ] 3.1 Define reusable hostile-region and safe-zone configuration shared by patrol, chase, navigation destination, and capture checks.
- [ ] 3.2 Update generic monster navigation so generated movement cannot enter configured safe zones or leave the hostile region.
- [ ] 3.3 Add focused tests or deterministic simulation checks for chase abandonment, capture suppression, and path requests at safe-zone boundaries.

## 4. Migration And Verification

- [ ] 4.1 Preserve existing serialized scene behavior by providing safe defaults or a controlled migration path for existing puzzle components.
- [ ] 4.2 Run Unity compilation and the focused EditMode/PlayMode test suite; inspect the Console for errors and warnings.
- [ ] 4.3 Document the generic configuration contract and identify `implement-formal-level02-mechanics` as the first formal-scene consumer without wiring Level 2 in this change.
