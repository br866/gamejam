# Formal Level 2 Art And Traversal Manifest

Source scene: `Assets/Scenes/Test/superbreadman 1.unity`

The formal Level 2 art assembly uses individual art objects selected from `Assets/Scenes/Test/superbreadman 1.unity`. Hierarchy grouping is not used as an ownership rule. An object is excluded as a Level 1 duplicate only when its world position matches a formal Level 1 object within 0.001 Unity units. Same-name objects at different world positions are retained as separate Level 2 art candidates. The source hierarchy remains unchanged. The retained objects are copied into `Prefabs/L02_Content.prefab`, which is instantiated below `Level02ContentRoot` in `FormalLevel02`. Because the Prefab is flattened, every retained object's position, rotation, and scale are written from its effective source world Transform rather than copied from its source local Transform.

This document is the current factual record for the Level 2 visual assembly, collision foundation, entry anchors, and direct testing workflow. It does not define the unfinished Level 2 puzzle, monster, gate, or exit gameplay.

## Accepted Source Boundary

- Current Editor selection: 180 objects.
- Visual selection: 179 objects with a Renderer or MeshFilter.
- Formal Level 1 world-position matches excluded: 2 objects.
- Initial migrated Level 2 visual objects: 177 objects.
- Corrected migrated Level 2 visual objects before review deletions: 175 objects, preserving their source world transforms.
- Review deletion result: 165 visual objects.
- Approved wall restoration result: 168 visual objects in the current `L02_Content` Prefab.
- Same-name objects at different positions are not deduplicated.

## Explicit Exclusions

- Two selected visual objects at world positions matching formal Level 1 content were excluded as duplicates.
- Selected objects without a Renderer or MeshFilter were excluded from the art-only Prefab.
- The selected protagonist visual meshes `boy (1)` and `dog (1)` were removed from the Level 2 art Prefab.
- The source scene remains unchanged; hierarchy names and parent grouping were not used to exclude otherwise distinct positions.

## Deferred Work

- Monster navigation and monster-inaccessible safe-space boundaries.
- Dog-only footprint visibility and first-plate interaction.
- Two-character second plate, checkpoint behavior, and level handoff.
- Gameplay triggers, UI, audio, and runtime logic.

Collision, spawn anchors, checkpoint anchors, direct scene entry, and baseline route validation are complete. The retained mechanism visuals are not playable mechanics yet.

## Retained Mechanism Foundation

The Level 2 visual review approved retaining the following existing objects as the foundation for the next Level 2 mechanism change. They remain outside this art-assembly change's original pure-art scope and must be validated, configured, or replaced in that later change rather than treated as completed gameplay.

- `monster3` at `(-11.36, 13.54, -5.36)`.
- `Monster` at `(-6.97, 15.82, 5.61)`, including its existing `CapsuleCollider`, `MonsterPatrol`, and `AudioSource` components. `MonsterPatrol` is currently disabled at runtime when its waypoint array contains an unassigned entry, so patrol, pursuit, and its navigation requests are intentionally inactive until the Level 2 monster-mechanics change configures valid waypoints.
- `Gate` at `(20.35, 14.44, -5.98)`, including its existing `BoxCollider`, `GateController`, and `AudioSource` components.
- Six Level 2 `FootprintMarker` objects, including their existing `FootprintMarker` components. Their MeshColliders are disabled because the markers are visual route hints and must not block formal player traversal.
- `wall5 (8)` at `(19.61, 13.82, -0.08)` and `wall5 (7)` at `(19.94, 13.93, -14.23)`, which overlap formal Level 1 content and are retained by visual approval pending route-boundary validation.

## World Transform Verification

- Prefab visual objects after transform correction: 175.
- Current Prefab visual objects after reviewed deletions and approved wall restoration: 168.
- Source-to-Prefab position mismatches above 0.001 units: 0.
- Source-to-Prefab rotation mismatches above 0.01 degrees: 0.
- Source-to-Prefab scale mismatches above 0.01 units: 0.
- `FormalLevel02` content instance world position: `(0, 0, 0)`.
- Source scene dirty after audit: `false`.
- Formal Level 2 scene dirty after audit: `false`.

## Identity Tracking Rule

Use the source scene object's `GlobalObjectId` as the primary migration and review identity. Record the object's Mesh asset GUID and local file ID as a secondary identity, and use its world Transform only for placement verification. Object names are descriptive only and must not be used as unique identifiers: several `wall5` objects share the same mesh asset while representing distinct scene objects. A flattened Prefab instance does not retain the source object's `GlobalObjectId`, so every future migration must record a source `GlobalObjectId` to destination object mapping at copy time.

## Selected Object Deletion Audit

The following objects were selected in the `FormalLevel02` Prefab instance and then removed from `L02_Content`. The recorded IDs are destination-scene instance identities, not source-scene identities. Each deletion matched the Prefab object by its name, Mesh GUID/local file ID, and world position, so same-name objects were not deleted.

- `medical_drawer_cart (1)`: `GlobalObjectId_V1-2-fb4d4e3b578848042ade42adaa0f232e-6436147464945484034-918470391`; Mesh `b7a64439a2cfe8e4b9286ec493ae62ad/7158796146257112550`.
- `medical privacy screen (3)`: `GlobalObjectId_V1-2-fb4d4e3b578848042ade42adaa0f232e-7861782476262441827-918470391`; Mesh `3a8608c8d1406404c9d68c3cf1a5ef9a/7158796146257112550`.
- `wall5 (10)`: `GlobalObjectId_V1-2-fb4d4e3b578848042ade42adaa0f232e-8421983901915169882-918470391`; Mesh `37f78a18ce42f2b499bff224c23c1f12/4858151225365440216`.
- `Meshy_AI_Weathered_Wood_Chair_0803213228_texture (1)`: `GlobalObjectId_V1-2-fb4d4e3b578848042ade42adaa0f232e-1914811448907392119-918470391`; Mesh `585c99d9323a28745ad747b64d508a0e/7158796146257112550`.
- `PressurePlate`: `GlobalObjectId_V1-2-fb4d4e3b578848042ade42adaa0f232e-6297469042160437254-918470391`; Mesh `0000000000000000e000000000000000/10202`.
- `clock (2)`: `GlobalObjectId_V1-2-fb4d4e3b578848042ade42adaa0f232e-9189757976339555368-918470391`; Mesh `deaef870551588a4689fbb85b85e868f/-6373483690846238654`.
- `medicine-box (1)`: `GlobalObjectId_V1-2-fb4d4e3b578848042ade42adaa0f232e-262980194264821066-918470391`; Mesh `7ce5a4dcfb69ee34f9353f56e48d58b4/7158796146257112550`.
- `wall5 (37)`: `GlobalObjectId_V1-2-fb4d4e3b578848042ade42adaa0f232e-21918379777865873-918470391`; Mesh `37f78a18ce42f2b499bff224c23c1f12/4858151225365440216`.
- `wall5 (35)`: `GlobalObjectId_V1-2-fb4d4e3b578848042ade42adaa0f232e-8125124326450237946-918470391`; Mesh `37f78a18ce42f2b499bff224c23c1f12/4858151225365440216`.
- `wall5 (34)`: `GlobalObjectId_V1-2-fb4d4e3b578848042ade42adaa0f232e-6638647746795921324-918470391`; Mesh `37f78a18ce42f2b499bff224c23c1f12/4858151225365440216`.

## World-Space Candidate Review

After the deletion, `L02_Content` contains 165 visual objects. A world-space proximity review found 84 un-migrated source visual objects near the remaining Level 2 layout. This is a review queue, not an automatic migration result. Whitebox and interaction candidates such as `Cube`, `PressurePlate`, `Gate`, `Pad`, and `PuzzleSwitch` are intentionally excluded from the high-confidence art list below.

High-confidence structural or decorative candidates, listed with their true source-scene `GlobalObjectId` and world position:

- `medicine-box (1)`: `GlobalObjectId_V1-2-0269c95022770ad4cb2c1b40d0f693c5-919132149155446097-236500401`; `(-3.26, 12.81, -13.33)`.
- `Meshy_AI_Weathered_Wood_Chair_0803213228_texture (1)`: `GlobalObjectId_V1-2-0269c95022770ad4cb2c1b40d0f693c5-919132149155446097-1185614133`; `(-5.49, 11.53, -15.31)`.
- `clock (2)`: `GlobalObjectId_V1-2-0269c95022770ad4cb2c1b40d0f693c5-919132149155446097-1150011373`; `(-4.88, 16.21, -19.04)`.
- `wall5 (8)`: `GlobalObjectId_V1-2-0269c95022770ad4cb2c1b40d0f693c5-919132149155446097-1636560461`; `(-9.52, 13.83, -19.63)`.
- `medicine-box (2)`: `GlobalObjectId_V1-2-0269c95022770ad4cb2c1b40d0f693c5-919132149155446097-1975184234`; `(3.18, 14.16, -12.47)`.
- `wall5 (34)`: `GlobalObjectId_V1-2-0269c95022770ad4cb2c1b40d0f693c5-919132149155446097-1975456886`; `(2.12, 14.50, 19.09)`.
- `wall5 (15)`: `GlobalObjectId_V1-2-0269c95022770ad4cb2c1b40d0f693c5-919132149155446097-646792227`; `(16.17, 13.83, -3.27)`.
- `wall5 (37)`: `GlobalObjectId_V1-2-0269c95022770ad4cb2c1b40d0f693c5-919132149155446097-1490441906`; `(-4.46, 14.50, 18.34)`.
- `wheelchair (2)`: `GlobalObjectId_V1-2-0269c95022770ad4cb2c1b40d0f693c5-919132149155446097-440069472`; `(-19.55, 11.58, 7.06)`.
- `medical privacy screen (3)`: `GlobalObjectId_V1-2-0269c95022770ad4cb2c1b40d0f693c5-919132149155446097-1383749738`; `(-20.08, 13.44, -10.59)`.
- `wall5 (6)`: `GlobalObjectId_V1-2-0269c95022770ad4cb2c1b40d0f693c5-919132149155446097-986363209`; `(16.78, 13.83, -9.10)`.
- `wall5 (7)`: `GlobalObjectId_V1-2-0269c95022770ad4cb2c1b40d0f693c5-919132149155446097-422926347`; `(8.37, 13.83, -19.63)`.
- `wall5 (35)`: `GlobalObjectId_V1-2-0269c95022770ad4cb2c1b40d0f693c5-919132149155446097-1480883843`; `(8.51, 14.50, 18.32)`.
- `wall5 (10)`: `GlobalObjectId_V1-2-0269c95022770ad4cb2c1b40d0f693c5-919132149155446097-1743546071`; `(-18.76, 13.83, -19.63)`.
- `Carpet2 (2)`: `GlobalObjectId_V1-2-0269c95022770ad4cb2c1b40d0f693c5-919132149155446097-87581370`; `(-26.25, 9.74, -6.23)`.
- `medical_drawer_cart (1)`: `GlobalObjectId_V1-2-0269c95022770ad4cb2c1b40d0f693c5-919132149155446097-1322688997`; `(-21.85, 13.51, -16.12)`.

## FootprintMarker Verification

The source scene contains eleven `FootprintMarker` objects in two separate world-space clusters. Six markers in the Level 2 cluster are present in `L02_Content` with zero position, rotation, and scale deltas:

- `FootprintMarker`: `(7.73, 11.77, -0.45)`.
- `FootprintMarker (1)`: `(10.23, 11.77, -4.65)`.
- `FootprintMarker (2)`: `(1.98, 11.71, 2.80)`.
- `FootprintMarker (3)`: `(-3.52, 11.61, 2.80)`.
- `FootprintMarker (4)`: `(2.09, 11.83, 7.19)`.
- `FootprintMarker (5)`: `(7.09, 11.95, 7.19)`.

Five markers at `x=-92.92` through `x=-107.97` are not in `L02_Content`. They form a remote source-scene cluster and remain excluded pending route ownership review.

## Selected Wall Verification

The following current source-scene wall selection was matched by Mesh identity and world position:

- `wall5 (5)`: present in Level 2 at `(-0.59, 13.83, -19.63)`.
- `wall5 (10)`: copied and verified in Level 2; source `GlobalObjectId_V1-2-0269c95022770ad4cb2c1b40d0f693c5-919132149155446097-1743546071`; `(-18.76, 13.83, -19.63)`.
- `wall5 (8)`: copied and verified in Level 2; source `GlobalObjectId_V1-2-0269c95022770ad4cb2c1b40d0f693c5-919132149155446097-1636560461`; `(-9.52, 13.83, -19.63)`.
- `wall5 (7)`: copied and verified in Level 2; source `GlobalObjectId_V1-2-0269c95022770ad4cb2c1b40d0f693c5-919132149155446097-422926347`; `(8.37, 13.83, -19.63)`.

The three copied walls have zero world-position and rotation deltas. Their world-scale deltas are below `0.0004` Unity units, which is floating-point serialization precision.

## Traversal Anchor Verification

- Human entrance: `(-8.00, 10.85, -5.16)` on `L02_CollisionRoot/Floor_West`.
- Dog entrance: `(-6.50, 10.85, -5.16)` on `L02_CollisionRoot/Floor_West`.
- Human checkpoint respawn: `(6.25, 10.85, -5.16)` on `L02_CollisionRoot/Floor_East`.
- Dog checkpoint respawn: `(7.75, 10.85, -5.16)` on `L02_CollisionRoot/Floor_East`.
- Provisional exit anchor: `(12.00, 10.85, -5.16)` on `L02_CollisionRoot/Floor_East`. It is an anchor only; exit behavior remains later Level 2 scope.
- `L02_CollisionRoot` supplies four floor volumes, four outer boundaries, and wall blockers derived from the approved visible architecture.
- Entrance and checkpoint anchors have ground support and no blocking capsule overlap for either formal player actor.
- Direct capsule checks pass from both entrance anchors to their checkpoint anchors and from the human checkpoint anchor to the provisional exit anchor.
- Six `FootprintMarker` MeshColliders are disabled because the markers are visual hints and must not block character traversal.
- Real formal Play Mode placement leaves both actors grounded with zero velocity at the Level 2 entrance. A direct physics walk from the entrance corridor remains grounded on `Floor_East`.
- The source prototype scene remained unchanged during traversal validation.
- Unity's current Test Runner returned an empty suite for the project test assembly, so anchor checks were executed directly through Editor physics queries and the formal Play Mode loading path. The validation test source remains under `Assets/Editor` for future test-assembly setup.

## Expanded Collider Coverage

- The Level 2 content Prefab contains 168 rendered objects. Broad BoxCollider coverage was added to 154 previously uncovered architecture, doors, monsters, furniture, and substantial fixed props.
- The formal scene exposes 188 Collider components, with 178 enabled after route-safety exclusions.
- Existing floor, boundary, wall, spawn, checkpoint, and provisional-exit collision remain authoritative for the validated traversal route.
- Disabled colliders: six footprint markers, two pedal-car visuals at the human/dog entrance, and two waiting-room chairs that overlapped the entrance capsule or crossed the approved entrance corridor.
- Direct Level 2 test procedure: open `FormalPersistent.unity`, select `FormalGameFlow`, set `FormalGameFlowController`'s `Initial Level Scene` to the exact scene name `FormalLevel02`, then enter Play Mode. The persistent spawner creates exactly one human/dog pair and the formal flow loads Level 2 additively.
- Direct-entry validation produced exactly two formal actors; both spawned grounded on `Floor_West` at the configured Level 2 entrance anchors.

## Current Formal Scene Ownership

- `FormalPersistent.unity` owns `FormalGameFlow`, `FormalPlayerSpawner`, the persistent human/dog actor Prefab, and the direct-test startup selection.
- `FormalLevel02.unity` is an additive content scene. It owns Level 2 art, collision, anchors, checkpoint configuration, and later gameplay configuration; it does not contain a duplicate player actor pair.
- The correct loadable scene name is `FormalLevel02`. `FormalLevel2` is not a build-settings scene name and causes additive loading to fail, leaving the persistent actors without Level 2 floor support.
- The normal route default may be set to `FormalLevel01`; set it to `FormalLevel02` only when directly testing Level 2 from `FormalPersistent`.

## Current Runtime Safeguards

- `MonsterPatrol` validates its entire waypoint array during `Awake`. If the array is empty or contains any unassigned Transform, the component disables itself before it can call patrol, pursuit, player detection, or navigation code.
- Current Level 2 validation found one retained `MonsterPatrol` with seven waypoint slots and at least one unassigned entry. Its component is therefore disabled at runtime until its explicit monster-mechanics setup is implemented.
- This safeguard intentionally prevents the previous `UnassignedReferenceException` from `MonsterPatrol.Patrol()` and does not configure any monster behavior.

## Verified Runtime State

- Direct `FormalPersistent -> FormalLevel02` startup loads exactly two `FormalPlayerActor` instances.
- Human starts at `(-8.00, 10.85, -5.16)` and dog starts at `(-6.50, 10.85, -5.16)`.
- Both actors are grounded on `Floor_West` after the scene is active.
- The Level 2 scene is active after additive load; `FormalPersistent` remains loaded as the owner of the player pair and game flow.
- Unity Console validation after the monster safeguard has no project errors. Any MCP transport warning is external tooling output and not Level 2 runtime behavior.

## Later Level 2 Mechanics Boundary

The following are deliberately not complete and belong to `implement-formal-level02-mechanics`:

- Assign valid monster waypoints, room bounds, chase bounds, safe-space exclusion, and navigation data.
- Make footprint markers dog-only visible route guidance.
- Configure the dog-only first plate, the cooperative second plate, and the retained gate opening/reset state.
- Enable checkpoint progression only after cooperative route completion.
- Configure the final exit trigger and successor scene handoff.
