## Context

`FormalScaleNormalizer` bakes leaf-mesh scales into mesh copies (`Assets/MoMing/BakedMeshes/`) and resets `localScale` to 1. Its current version syncs MeshColliders, but the pass that produced today's live overrides did not, leaving colliders ~967x smaller than visuals (found on `broken_wall (1)` in `L01_Content.prefab`, fixed 2026-08-23). The same defect existed on `table_and_chairs (1)` in `L03_Content.prefab`. We need a reusable detection/repair tool scoped to user selection, without touching legacy normalizer code.

## Goals / Non-Goals

- Goals:
  - Selection-scoped audit (report) + confirm-guarded fix under a `Tools/Final/` menu group.
  - Reliable classification that never mistakes an approved collision proxy for a bug.
  - Correct override recording inside nested prefab instances.
  - EditMode tests incl. regression check on `L01_Content.prefab`.
- Non-Goals:
  - Batch scanning of all scenes or prefab assets (user chose selection-driven workflow).
  - Changes to `FormalScaleNormalizer.cs` (RollbackScene MeshCollider gap stays documented, not fixed here).
  - Runtime behavior changes.

## Decisions

### D1. Classification via world-space bounds comparison
A node with MeshFilter(mesh) + MeshCollider is classified by comparing world bounds:
- collider mesh null → **Missing** (fixable)
- same mesh reference → **Ok**
- different meshes, per-axis size ratio within ±10% → **Proxy** (report only; e.g., simplified collision geometry is legitimate)
- otherwise → **Broken** (fixable)

World bounds come from `MeshRenderer.bounds` when present, else from mesh bounds transformed by the node's `localToWorldMatrix`. ±10% absorbs floating-point noise while catching 967x-scale divergence trivially.

*Alternative rejected:* comparing vertex data/hashes — heavier and still ambiguous for legitimately decimated proxies.

### D2. Repair writes through SerializedObject
Fix assigns `m_Mesh` objectReferenceValue via `SerializedObject.ApplyModifiedProperties()` on the MeshCollider, pointing at the MeshFilter's current sharedMesh. This records instance-level overrides on nested prefab instances (same mechanism proven in the 2026-08-23 manual repair), instead of raw field assignment which can silently fail to persist as an override.

### D3. Selection-driven API surface
Static class with `[MenuItem]` entries plus public static `Audit(IEnumerable<GameObject>)` / `Fix(List<Issue>)` so EditMode tests drive the same code path users trigger from menus — mirroring `FormalScaleNormalizer` conventions but without journal files (a console report suffices at this scope).

## Risks / Trade-offs

- Proxy tolerance (±10%) could misclassify a slightly-off broken pair as proxy → reported anyway, just not auto-fixed; acceptable since report lists it.
- Fix relies on renderer mesh being correct; if both filter and collider were stale the tool cannot know — out of scope, report shows references for human review.

## Migration Plan

No data migration. Ship tool + tests; run audit on formal levels once as verification.

## Open Questions

- None.
