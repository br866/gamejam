## Why

Scale normalization (`FormalScaleNormalizer`) bakes non-uniform/huge scales into mesh copies and resets transform scale to 1. When a node also carries a MeshCollider, the collider must be repointed at the same baked mesh. At least two production nodes (`broken_wall (1)` in L01_Content, `table_and_chairs (1)` in L03_Content) silently kept the raw FBX mesh, leaving colliders hundreds of times smaller than the visible geometry. There was no automated way to detect this class of failure before players fall through walls.

## What Changes

- Add an editor tool `MeshColliderSyncAuditor` under `Tools/Final/` that audits a user-selected hierarchy (including nested prefab instances) for nodes holding both a MeshFilter and a MeshCollider.
- Classify each such node as in-sync, intentional collision proxy (different mesh but matching world bounds), broken (divergent bounds), or missing collider mesh.
- Provide a report-only menu action and a confirm-guarded repair action; repair reassigns only the offending MeshCollider mesh references via recorded prefab-instance overrides. Transforms, layers, materials, and unrelated objects are never touched.
- Add EditMode tests covering the classification logic, the repair behavior, and a regression check that `L01_Content.prefab` contains no unresolved violations.

## Capabilities

### New Capabilities
- None.

### Modified Capabilities
- `formal-level-collider-normalization`: Adds requirements that normalized art keep renderer/collider meshes consistent, and that a selection-scoped editor audit can detect and repair violations.

## Impact

- New editor-only script `Assets/MoMing/Scripts/Editor/MeshColliderSyncAuditor.cs`; no runtime scripts or scenes change.
- Existing formal-level prefabs are read by tests but not modified by this change.
- Future normalization work gains a repeatable verification step instead of manual inspection.
