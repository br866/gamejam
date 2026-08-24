## Why

Formal route floors are pure visual meshes; walkability comes from dozens of hand-placed invisible box colliders scattered across scenes and per-instance prefabs. This system leaks (forgotten/misaligned proxies), has no single source of truth for floor height, and every fix is a manual scene edit.

## What Changes

- New `FormalGroundVolume` component: one BoxCollider driven by a single `topHeight` field (world-space walkable surface height, thickness 5 m downward), with gizmo preview.
- One global volume instance in `FormalPersistent` covering the entire route at a single height (initial value measured from L01's existing floor).
- Four small editor tools under `Tools/Formal/Ground/` with JSONL journals (rollback-able):
  1. Create/fit a volume from selected renderers
  2. Disable all colliders on the `NavGround` layer except carriers of `FormalGroundVolume`
  3. Audit coverage: report walkable surfaces beyond volume bounds and art surfaces whose top deviates from `topHeight`
  4. Copy first-selected volume's `topHeight` onto other selected volumes
- Layer semantics: `NavGround` = "legacy/auxiliary floor colliders". Manual stakeholder audit retags Default-layer floor proxies to NavGround before running the disable tool.

## Capabilities

### New Capabilities
- `formal-ground-volume`: Defines unified ground collision for the Formal route: a single authoritative ground surface, NavGround disable semantics, and coverage auditing.

### Modified Capabilities
<!-- None. -->

## Impact

- New: `Assets/MoMing/Scripts/Environment/FormalGroundVolume.cs` (runtime, ExecuteAlways) and editor tools under `Assets/MoMing/Scripts/Editor/`.
- `FormalPersistent.unity`: one new GameObject with the volume.
- Requires stakeholder-run retag of legacy floor proxies to NavGround; A* graph re-bake after swap (height-sampling mask must include the new ground layer).
