## 1. Audit And Classification

- [x] 1.1 Generate a formal-level Prefab and scene audit listing every MeshCollider, its Layer, model bounds, runtime dependencies, and recommended `remove`, `single-box`, or `manual-review` classification.
- [x] 1.2 Review L01 classifications against traversal, interaction, camera obstruction, and A* obstacle responsibilities.
- [x] 1.3 Record irregular or concave blocking models in a manual-review list rather than applying automatic single-box conversion; retain `broken_wall` as an approved `NavStatic` MeshCollider exception.

## 2. L01 Pilot

- [x] 2.1 Remove Collider components and navigation-obstacle Layer assignments from approved L01 visual-only small props.
- [x] 2.2 Replace approved L01 simple blocking MeshColliders with bounds-aligned BoxColliders that preserve supported collider settings.
- [x] 2.3 Assign retained fixed obstacles to `NavStatic` and retained route-changing obstacles to `NavDynamic` without changing existing navigation code.
- [ ] 2.4 Verify L01 player traversal, camera obstruction, crate pushing, and A* obstacle behavior after the pilot conversion.

## 3. Formal Asset Rollout

- [x] 3.1 Apply approved visual-only removal and simple-box conversion rules to L02-L05 and shared-art Prefabs in reviewable batches.
- [x] 3.2 Verify each converted batch has no inappropriate MeshCollider, no missing required blocker, and no visual-only Collider.
- [x] 3.3 Verify formal navigation continues to use only `NavStatic` and `NavDynamic` obstacles.

## 4. Prefab Ownership And Final Verification

- [x] 4.1 Create a Prefab for `SuccessorCheckpoint` and replace the direct rendered object in `FormalLevel02` while preserving behavior and references.
- [x] 4.2 Audit every formal-level scene for rendered non-Prefab GameObjects and resolve or document any exception.
- [ ] 4.3 Run Unity validation and targeted play checks for collision, navigation, and scene integrity.
