# Formal Level 3 Art Manifest

Source scene: `Assets/Scenes/Test/superbreadman 1.unity`

## Source Selection Summary

- Active selection at assembly time: 211 objects.
- Objects with explicit `Level3` hierarchy attribution: 98.
- Initial explicit `Static Scene/Level3` visual objects: 96.
- Explicit Level 3 non-visual objects: 2.
- Formal Level 1/2 world-position duplicates among the explicit Level 3 visuals: 0.
- Initial copied static visual objects: 96.
- Final source-group rebuild: 163 Level 3 visual objects.

## Accepted Assembly Boundary

- Objects under `Static Scene/Level3`, `floor/Level3`, and `Item/Level3` with a Renderer are assembled into `Prefabs/L03_Content.prefab`.
- `floor/Level3` supplies the previously missing ProBuilder structural geometry required for the complete Level 3 visual layout.
- `Item/Level3` supplies Gate, SequenceGate plate, PressurePlate, and Checkpoint Pad visuals only. Their prototype behavior components are stripped from the formal art Prefab.
- The content Prefab is instantiated below `Level03ContentRoot` in `FormalLevel03.unity`.
- Effective source world position, rotation, and lossy scale are preserved for accepted art.
- The flattened Prefab does not retain source parent hierarchy or source `GlobalObjectId`; source identity must be maintained in this manifest for future review.

## Explicit Exclusions

- Two Level 3-attributed non-visual selected objects: excluded because they have no Renderer.
- Any selected Level 4, player/runtime, interactive, shared, or otherwise unconfirmed object: excluded from automatic Level 3 migration.
- No explicit Level 3 visual objects duplicated existing Formal Level 1 or Level 2 content at the 0.001-unit world-position tolerance.

## Current Art Layout

- The assembled Level 3 hospital/playroom visual region spans source world positions approximately from `(-83.30, 9.00, -32.15)` to `(-37.15, 16.94, 18.06)`.
- Accepted environment includes Level 3 floors, ProBuilder structure, walls, windows, vents, beds, lockers, cabinets, furniture, hospital dressing, paintings, toys, playroom dressing, gates, plates, and checkpoint-pad visuals.
- The art Prefab contains no source runtime collision, navigation, character, checkpoint, exit, trigger, or mechanic components. FormalLevel03 separately owns its traversal anchors and foundational collision.

## Missing-Content Correction

- The first Level 3 assembly included only the explicit `Static Scene/Level3` group and therefore omitted `floor/Level3` structural geometry and `Item/Level3` mechanism visuals.
- `L03_Content` was rebuilt from all three Level 3 source groups: `95` static visual objects, `57` floor/structure objects, and `11` Item/Level3 visuals, for `163` visual objects total.
- All copied source Collider, AudioSource, ProBuilder, GateController, SequenceGateController, PressurePlate, Checkpoint, and other runtime components were removed from the formal art Prefab. Scene-owned traversal collision and anchors remain the only active Level 3 runtime foundation.
- The rebuilt layout preserves the source objects' world positions, rotations, and scales, and the formal player pair now remains visible and grounded when Level 3 loads through FormalPersistent.

## Broad Nearby-Art Completion

- User-confirmed selection bounds: `x=-85.19..-17.39`, `y=-2.27..18.86`, `z=-37.01..25.27`.
- Nearby scan bounds, expanded by eight units: `x=-93.19..-9.39`, `y=-10.27..26.86`, `z=-45.01..33.27`.
- Broad inclusion found 332 source visual candidates inside the scan volume.
- Included in the rebuilt Level 3 art Prefab: 331 visual objects.
- Skipped due to same world position already represented during rebuild: 1 object.
- Excluded because source path explicitly attributed the object to Level 2: 41 objects.
- Excluded because source path explicitly attributed the object to Level 4: 40 objects.
- Names, mesh reuse, shared hierarchy, prototype behavior, and prior-level appearance did not exclude nearby visual objects when their world positions differed.
- Copied visuals have runtime behavior stripped. Source Colliders, scripts, audio, rigidbodies, navigation, player, trigger, and mechanic components are not copied.
- FormalLevel03 now renders 536 objects including the persistent scene camera/light and scene-owned visualization; the rebuilt `L03_Content` contains 331 broad-scan visual children.
- The user will perform later manual visual pruning; this manifest preserves the broad inclusion rationale and exclusion counts.

## Traversal Foundation

- Human entrance: `(-80.00, 10.85, 18.00)` on `L03_CollisionRoot/Floor_WestNorth`.
- Dog entrance: `(-78.50, 10.85, 18.00)` on `L03_CollisionRoot/Floor_WestNorth`.
- Human checkpoint respawn: `(-60.75, 10.86, 18.00)` on `L03_CollisionRoot/Floor_CenterNorth`.
- Dog checkpoint respawn: `(-59.25, 10.86, 18.00)` on `L03_CollisionRoot/Floor_EastNorth`.
- Provisional exit anchor: `(-40.00, 10.85, 18.00)` on `L03_CollisionRoot/Floor_FarEastNorth`. It is a route-validation endpoint only, not a completion trigger.
- `L03_CollisionRoot` contains eight floor volumes, four outer boundaries, and twelve wall proxies derived from accepted Level 3 walls.
- All entrance, checkpoint, and provisional exit anchors have downward floor support and no initial blocking player-capsule overlap.
- Human and dog CapsuleCast checks pass from entrance to checkpoint and from checkpoint to the provisional exit anchor after wall proxies are present.
- `L03_Checkpoint` is configured with separate human and dog respawn anchors through the existing formal checkpoint system.

## Direct Level 3 Test

- Add `FormalLevel03` to Build Settings before using formal additive loading.
- Open `FormalPersistent.unity`, select `FormalGameFlow`, set `Initial Level Scene` to the exact name `FormalLevel03`, then enter Play Mode.
- Direct-entry verification loads `FormalPersistent` and `FormalLevel03`, creates exactly one human/dog actor pair, and leaves both actors grounded on `Floor_WestNorth`.
- Restore `Initial Level Scene` to `FormalLevel01` after direct Level 3 testing unless a temporary Level 3 test session is intentionally active.

## Identity And Transform Verification

- Primary source identity: source-scene `GlobalObjectId`.
- Secondary identity: mesh asset GUID and local file ID when present.
- Placement verification: effective source world position, rotation, and lossy scale.
- Object names are descriptive only and are not unique identifiers.
- Verification tolerance: position `0.001` Unity units, rotation `0.01` degrees, scale `0.01` Unity units.

## Deferred Follow-Up

- Define HumanSpawn, DogSpawn, checkpoint anchors, and a provisional exit anchor on supported Level 3 geometry.
- Add floor, boundary, architecture, and fixed-prop collision before player traversal tests.
- Validate the Level 3 path with both formal player capsules.
- Configure Level 3 gates, plates, pickups, enemies, navigation, checkpoint progression, and exit behavior only in a dedicated Level 3 mechanics change.

## Broad Collider Coverage

- The user-approved Level 3 visual curation remains unchanged; collision is scene-owned under `L03_CollisionRoot/L03_BroadColliderCoverage` and does not modify `L03_Content` renderer objects.
- Added `161` enabled, non-trigger BoxCollider proxies from retained visual world bounds: `67` architecture, `16` gates/doors/grilles, and `78` substantial fixed props.
- `FormalLevel03` now contains `186` Collider components: the original `25` traversal-foundation colliders plus the new broad coverage proxies.
- Visual-only or non-blocking exclusions: human and dog display meshes, pressure plates, pads, footprint/particle hints, floor renderers, small toys, pictures, clocks, zero-bounds renderers, and objects intersecting the approved north-side baseline route corridor.
- Existing dedicated floor volumes, outer boundaries, wall proxies, spawn anchors, checkpoint anchors, and provisional exit anchor remain unchanged.
- Post-coverage physics checks confirm floor support with no blocking overlap at HumanSpawn, DogSpawn, both checkpoint respawn anchors, and the provisional exit anchor. Human and dog CapsuleCast checks pass from entrance to checkpoint and checkpoint to provisional exit.
- Direct Play Mode test temporarily set `FormalPersistent` to `FormalLevel03`, loaded `FormalPersistent` plus `FormalLevel03`, and created exactly one grounded human/dog actor pair with Rigidbody components. The dog advanced along the approved corridor through the existing movement API without scene-collider blockage.
- The human actor's direct `Move` call was immediately reset by its pre-existing runtime input/controller state, while its spawn capsule had no collision overlap. This is not a Level 3 collider failure and is deferred outside this collision-only change.
- `FormalPersistent` was restored to `FormalLevel01` after the direct test. Unity Console reported no errors or warnings.

## Pad-Adjacent Spawn

- Retained Pad world bounds: center `(-35.97, 10.74, -5.96)`, size `(1.50, 0.20, 1.50)`.
- HumanSpawn moved to the Pad's west side at `(-37.92, 10.86, -6.56)`.
- DogSpawn moved to the Pad's west side at `(-37.92, 10.86, -5.06)`.
- Both anchors have floor support, no blocking player-capsule overlap, a clear three-unit westward movement direction, and `1.50` units of separation from each other.
- Direct `FormalPersistent -> FormalLevel03` verification spawned exactly one grounded human/dog pair at the Pad-adjacent anchors. FormalPersistent was restored to `FormalLevel01` afterward.

## Formal Camera Ownership

- Formal Level 3 previously loaded two enabled depth-zero cameras tagged `MainCamera`: persistent `FormalMainCamera` and the scene-local prototype `Main Camera`.
- `FormalLevel03/Main Camera` is retained for reference but its Camera component is disabled and its tag is `Untagged`.
- Direct `FormalPersistent -> FormalLevel03` verification now reports exactly one enabled `MainCamera`: `FormalMainCamera` in FormalPersistent, with CameraFollow targeting `FormalHumanActor`.
- FormalPersistent was restored to `FormalLevel01` after verification.

## Collider Integrity Audit

- Full audit inspected all `186` original Collider components: `161` broad renderer-derived BoxCollider proxies, `24` dedicated non-trigger traversal colliders, and one checkpoint trigger.
- No enabled Collider had zero or near-zero dimensions.
- Removed `94` defective broad proxies without modifying any curated renderer or transform:
  - `69` visual-only or suspended details: wall lamps, pictures, signs, room labels, clocks, carpets, pedals, pipes, buttons, windows, curtains, vents, and character decoration.
  - `14` proxies outside the formal playable boundary or below the playable floor.
  - `11` proxies exactly duplicated the existing dedicated wall colliders.
- Retained `67` broad proxies for major static architecture, gates, doors, beds, cabinets, tables, chairs, carts, lockers, and similar fixed obstacles.
- FormalLevel03 now has `92` enabled Collider components: `67` broad fixed-obstacle proxies, `24` dedicated traversal colliders, and the checkpoint trigger.
- Post-audit checks confirm all Pad-adjacent spawn, checkpoint, and provisional-exit anchors have floor support and no blocking capsule overlap. Human has clear two-unit movement directions west, east, and north; dog has clear west, east, and south directions.
- Direct `FormalPersistent -> FormalLevel03` Play Mode verification still creates exactly one grounded human/dog pair at the Pad-adjacent anchors.
- `FormalPersistent.initialLevelScene` remains `FormalLevel03` for the active Level 3 test session.

## Center Floor Connector

- Measured gap between `Floor_CenterNorth` and `Floor_CenterWestSouth`: shared world x span `-75.870..-59.810`, with an uncovered z span from `-13.110` to `-7.015` (`6.095` units).
- Added `L03_CollisionRoot/Floor_CenterConnector` at world center `(-67.840, 9.800, -10.063)`, size `(16.060, 0.200, 6.295)`.
- The connector overlaps each adjacent floor edge by `0.100` units to prevent a physics seam.
- Floor support now succeeds at the north seam, connector center, and south seam. The only southbound player-capsule route hit is existing `Wall_2`, not the connector, and remains as intended architecture collision.
- A temporary 0.25-unit floor-coverage function found that the initial connector only covered the center third of the full x-span. It reported remaining gaps west at `x=-91.35..-75.85` and east at `x=-59.85..-29.10`, both across the same `z=-13.11..-7.015` band.
- Added `Floor_WestConnector` and `Floor_EastConnector` at the shared floor height, overlapping their neighbors by `0.1` units.
- Re-running the temporary coverage function reports `centerBandMissing=0` across the complete center band `x=-91.48..-28.99`, `z=-13.11..-7.015`.
