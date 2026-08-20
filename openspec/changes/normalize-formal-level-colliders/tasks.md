## 1. Audit And Classification

- [x] 1.1 Generate a formal-level Prefab and scene audit listing every MeshCollider, its Layer, model bounds, runtime dependencies, and recommended `remove`, `single-box`, or `manual-review` classification.
- [x] 1.2 Review L01 classifications against traversal, interaction, camera obstruction, and A* obstacle responsibilities.
- [x] 1.3 Record irregular or concave blocking models in a manual-review list rather than applying automatic single-box conversion; retain `broken_wall` as an approved `NavStatic` MeshCollider exception.

## 2. L01 Pilot

- [x] 2.1 Remove Collider components and navigation-obstacle Layer assignments from approved L01 visual-only small props.
- [x] 2.2 Replace approved L01 simple blocking MeshColliders with bounds-aligned BoxColliders that preserve supported collider settings.
- [x] 2.3 Assign retained fixed obstacles to `NavStatic` and retained route-changing obstacles to `NavDynamic` without changing existing navigation code.
- [x] 2.4 Verify L01 player traversal, camera obstruction, crate pushing, and A* obstacle behavior after the pilot conversion.

## 3. Formal Asset Rollout

- [x] 3.1 Apply approved visual-only removal and simple-box conversion rules to L02-L05 and shared-art Prefabs in reviewable batches.
- [x] 3.2 Verify each converted batch has no inappropriate MeshCollider, no missing required blocker, and no visual-only Collider.
- [x] 3.3 Verify formal navigation continues to use only `NavStatic` and `NavDynamic` obstacles.

## 4. Prefab Ownership And Final Verification

- [x] 4.1 Create a Prefab for `SuccessorCheckpoint` and replace the direct rendered object in `FormalLevel02` while preserving behavior and references.
- [x] 4.2 Audit every formal-level scene for rendered non-Prefab GameObjects and resolve or document any exception.
- [x] 4.3 Run Unity validation and targeted play checks for collision, navigation, and scene integrity.

## 5. Mechanism Trigger Standardization

- [x] 5.1 Audit formal mechanism devices across L01-L05 for Prefab ownership, Collider presence, trigger state, visual-model colliders, and trigger behavior.
- [x] 5.2 Make the L01 pedal a Prefab-owned mechanism with an explicit trigger-only detection Collider and preserved permanent door behavior.
- [x] 5.3 Record other mechanism devices that require migration or manual review instead of changing unrelated solid obstacles.
- [x] 5.4 Add and run validation coverage for trigger-only mechanisms versus retained physical blockers.

## 6. Strict Prefab Deduplication

- [x] 6.1 Generate a structural fingerprint for SharedModels Prefabs that ignores material identity while retaining hierarchy, mesh, Collider, Layer, Tag, and script identity.
- [x] 6.2 Select canonical Prefabs and audit all formal scene/content-Prefab instances referencing duplicate members.
- [x] 6.3 Replace eligible instances with canonical Prefab instances while preserving parent, Transform, name, active state, Layer, Tag, and supported overrides.
- [x] 6.4 Verify no duplicate-member instances remain in formal scenes/content Prefabs and record manual-review exclusions.
- [ ] 6.5 Preserve per-instance Renderer material differences when replacing duplicate members with canonical Prefab instances.
- [x] 6.6 Delete obsolete duplicate Prefab assets only after excluding external scene/resource references; retain referenced shared-art assets.
