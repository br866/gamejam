## Context

Post-rebase diagnosis (see proposal.md - Why) established three facts that constrain the design:

1. Teammate's animation pass set every character clip FBX to `animationType: 3` (Humanoid). The player model FBX (`Push_and_Walk_Forward`, which supplies the avatar used by `FormalHumanVisual.prefab`) is still `animationType: 2` (Generic), and the L02 monster model instance (`Monster3Model`, instantiated from the doctor3 Walking FBX) has no avatar at all. Humanoid muscle clips on non-humanoid avatars leave skeletons frozen while animator state time advances — confirmed by bone-position sampling in play mode.
2. The dog pipeline works and must not be touched: its single clip is a Generic bone-path clip (`Armature/Hips/...`) on a Generic rig.
3. The physics crate rewrite made crates dynamic rigidbodies; without engagement gating, any body can shove them. L05's doors were placed as free-standing prefab instances on one open floor slab.

The scale normalizer tool already exists (`FormalScaleNormalizer`, leaf-mesh-only, collider-compensating) and was proven on the test crate; what is missing is batch orchestration with journaling and rollback.

## Goals / Non-Goals

**Goals:**
- Make Humanoid clips drive the player and monster skeletons (Option A from exploration).
- Keep the change reversible at every step: import-setting flips are single-file reverts; normalization is journal-driven.
- Gate crate physics behind engagement; add fixed-axis + travel-limit fields for the cabinet variant.
- Give L05 real interior walls so its two doors gate actual passages.
- Verify monster navigation connectivity after every scene-geometry change.

**Non-Goals:**
- Converting skinned characters or hierarchical art to normalized scales (tool deliberately skips them).
- Implementing the L5 spec's controlled-escape rules (switch lock, fixed camera, run unlock) — separate future change.
- Reworking the dog animation setup (only one dog clip exists by design).
- Any UI/audio work (owned by teammate's branches).

## Decisions

### D1: Adopt Humanoid on the models, not Generic on the clips
Converting the two model FBXs to Humanoid adopts the teammate's comprehensive clip pass wholesale. Reverting clips to Generic (Option B) would break teammate scenes built on Humanoid assumptions and fight the direction of main.
- Player model: flip `Push_and_Walk_Forward` FBX `animationType` to 3; regenerate avatar; verify in play mode that Walk/Idle deform the skeleton.
- Monster model: assign a Humanoid avatar to the `Monster3Model` instance (the Walking FBX it is instantiated from gets flipped to Humanoid too).
- Fallback if Meshy auto-mapping fails (T-pose, shifted feet, unmapped bones): revert these metas and instead flip the five boy clip FBXes + six monster3 clip FBXes back to Generic (Option B). Both paths are meta-file-only edits.
- Jump/Pull `.anim` assets use plain transform curves; on a Humanoid avatar these still animate matching transform paths — verified during play-mode check; if Pull regresses, re-bake Pull curves against humanoid bone names as a local fix.

### D2: Engagement gating via kinematic toggle, not mass/forces
`Awake` keeps the crate kinematic; `TryEngage` switches it dynamic; `Cancel`/reset restores kinematic. Alternatives rejected: huge mass (still slides on slopes, breaks drag tuning), constraints freeze per-axis (walls would still receive push forces). Kinematic-when-idle also makes crates monster-proof for free.

### D3: Fixed axis as serialized enum + magnitude, not derived-from-point
Cabinet variant adds `axisMode {Auto, PlusX, MinusX, PlusZ, MinusZ}` and `travelLimit` (0 = unlimited). `Auto` preserves today's derive-axis-from-engagement-point behavior, so existing L01 rail usage and test scene stay untouched.

### D4: L05 walls as plain whitebox BoxColliders parented under a new `L05_InteriorWalls` root
Matches the established BroadColliderCoverage style, avoids touching art content, and makes nav-graph rescans trivially explainable. Door gaps are cut around the existing door prefab positions; doors themselves do not move. After walls land, run the flood-fill connectivity probe (same tooling that diagnosed L03) against both monsters' graphs and adjust patrol areas if any pocket appears.

### D5: Normalization batch = inventory → transform → journal → optional rollback
Batch flow per scene: (1) pre-flight inventory dump of candidate leaf nodes with their scales/meshes; (2) normalize node-by-node, appending a JSON journal line per node BEFORE the scene save; (3) rollback command reads the latest journal for a scene, restores scale + mesh reference, deletes created baked assets, then appends a "rolled back" marker. Prefab-level pass runs the same tooling inside prefab asset mode for shared props, recorded in a separate journal section. Skipped nodes (children/skinned/particles) are journaled as skipped for auditability.

## Risks / Trade-offs

- [Meshy rigs fail humanoid auto-mapping] → Meta-only fallback to Option B costs minutes; decision gate is the first play-mode bone-motion check after flipping the player model.
- [Humanoid conversion shifts visual pose/foot placement] → Compare world hip height before/after; adjust visual offset on the instance, not the controller.
- [Kinematic gating changes L01 crate feel] → L01 uses the old RailMover, not this component; only test-scene and future cabinet behavior changes.
- [Normalization batch touches live level geometry] → Per-scene journals + rollback; run one scene first (start with CrateMechanicsTest, then L045 as the emptiest formal scene), verify, then continue.
- [Prefab-level normalization diverges instances with scale overrides] → Pre-flight inventory lists instances carrying scale overrides before touching a prefab; those instances get explicit review rather than silent inheritance.
