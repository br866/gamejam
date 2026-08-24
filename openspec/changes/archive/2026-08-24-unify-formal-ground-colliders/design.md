## Design

### Volume component

`FormalGroundVolume` (ExecuteAlways, `Assets/MoMing/Scripts/Environment/`):
- Serialized `topHeight` (world Y of walkable surface) and `thickness` (default 5).
- Owns its BoxCollider; `OnValidate`/editor update syncs `size.y = thickness` and `center.y = topHeight − transform.position.y − thickness/2` (assumes unrotated, unscaled volume object at scene root — validated with a warning otherwise).
- Green translucent gizmo; layer set to NavGround.
- Global instance lives in `FormalPersistent` (always loaded across the additive route), so one collider serves every level combination.

### Tools (`Tools/Formal/Ground/…`, Scripts/Editor, JSONL journal like FormalScaleNormalizer)

1. **Volume From Selection** — union world XZ bounds of selected renderers → create or refit a scene-root volume; default topHeight = min renderer bottom (designer overrides to L01 floor value on first run).
2. **Disable NavGround Colliders** — designed but intentionally unused in the final approach: the single large volume makes legacy disable unnecessary. Tool remains available for rollback scenarios.
3. **Audit Coverage** — heuristic: renderer bounds whose top faces are near walkable height and XZ-overlap route extents but fall outside volume bounds → "hole"; bounds top within ±1 m of surface height but off by > 5 cm → "misaligned". Logs a plain-text list.
4. **Copy Top Height** — first selection's `topHeight` applied to other selected volumes via Undo.

### Interaction with navigation

Monster/dog A* graph obstacle mask is NavStatic+NavDynamic (excludes NavGround). Since legacy floor colliders are untouched, existing graphs continue sampling them as before; the volume adds redundant support rather than replacing collision. A re-bake is only needed if gameplay testing reveals height-sampling artifacts from the added overlap.

### Ordering

Ships after `reform-formal-actor-pivot-and-scale` so playtests validate both physics conventions together.

### Risks

- Audit heuristics may over-/under-report on decorative meshes; output is advisory lists, never auto-edits.
- Disable pass depends on stakeholder's manual retag completeness; audit doubles as the completeness check.
