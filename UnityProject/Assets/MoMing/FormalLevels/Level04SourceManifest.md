# Formal Level 4 Art Manifest

Source scene: `Assets/Scenes/Test/superbreadman 1.unity`

## User-Selected Assembly

- The final active Unity selection contained `263` objects with `268` Renderer components.
- The Level 4 selection is the primary source boundary for this assembly.
- `266` selected visual objects were copied into `Prefabs/L04_Content.prefab` with their effective world transforms preserved.
- The two `PlayerSystem` player renderers were excluded because FormalPersistent remains the sole owner of the formal human/dog pair.
- The wide selection is accepted intentionally and places the final visual-pruning decision in the Unity Editor.
- Names, mesh reuse, shared hierarchy, prior-level appearance, and prototype behavior did not exclude selected visuals.
- Runtime scripts, colliders, rigidbodies, audio, navigation, player objects, triggers, and mechanic components were stripped from copied content.
- The selected content includes the Level 4 hospital area, gates, plate/switch/pickup visuals, checkpoint visuals, monster visuals, architecture, furniture, and environmental dressing.

## Global Transform Verification

- The first whole-object cloning method allowed source parent transforms to distort many copied visual world positions. It was replaced with a flattened per-Renderer copy that explicitly writes world Position, Rotation, and Lossy Scale.
- Immediate verification while the source selection was still active matched all `266/266` copied Renderers by world Bounds center and size.
- Maximum verification deltas: Bounds center `0.000001`, Bounds size `0.000005` Unity units. These are floating-point precision differences within the Level 3 transform verification tolerance.
- Unity clears Selection when the editor switches from the source scene to FormalLevel04; verification is intentionally performed before that switch so it never relies on names or hierarchy afterward.

## Explicit Separation

- `floor/Level4.5` is a separate downstream corridor and was not copied unless explicitly selected by the user.
- The source scene remained unchanged after prefab construction.

## Traversal Foundation

- `FormalLevel04.unity` instantiates `L04_Content` under `Level04ContentRoot` and contains no player actors.
- `L04_CollisionRoot` owns a basic hospital-area floor and outer boundaries.
- HumanSpawn: `(-92.00, 10.86, -5.50)`.
- DogSpawn: `(-93.50, 10.86, -5.50)`.
- Both anchors are west of the selected checkpoint Pad area, are supported by the formal floor, have no blocking capsule overlap, and have clear westward movement.

## Direct Test

- `FormalLevel04` is registered in Build Settings.
- FormalPersistent `Initial Level Scene` is intentionally `FormalLevel04` for the active fourth-level test session.
- Direct `FormalPersistent -> FormalLevel04` Play Mode verification loads exactly one grounded human/dog actor pair at the Level 4 entry anchors.
- FormalLevel04 contains `266` Renderer components, `7` scene-owned Collider components, and no scene-local Camera. The persistent FormalMainCamera remains the only runtime main camera.

## Broad Collider Coverage

- Added `114` scene-owned non-trigger BoxCollider proxies below `L04_CollisionRoot/L04_BroadColliderCoverage`: `64` architecture/gate/door/window/pipe proxies and `50` substantial fixed-prop proxies.
- FormalLevel04 now contains `119` Collider components: the original floor/boundary foundation plus broad static coverage.
- Kept `31` visual objects outside the Level 4 hospital bounds non-blocking.
- Kept `117` visual-only objects non-blocking: selected cross-level Plate/Pad/Footprint visuals, player/monster display meshes, hints, pickup/switch visuals, lamps, pictures, signs, labels, bottles, trays, buttons, pedals, carpets, curtains, restroom decoration, and other small details.
- Entry anchors remain supported with no blocking overlap. Both formal actors have clear two-unit movement directions west, east, and south from their Level 4 entry positions.
- Direct verification was run through `FormalPersistent -> FormalLevel04`, which spawned exactly one grounded human/dog pair. Directly pressing Play while FormalLevel04 alone is open intentionally has no actors because players are persistent-scene owned.

## Deferred Follow-Up

- Review the broad user-selected visual layout in the Unity Editor and manually prune any unwanted objects.
- Add detailed Level 4 wall, furniture, gate, and prop collision after the visual selection is accepted.
- Implement Level 4 gates, plates, pickup, monsters, checkpoint, exit, and any Level 4.5 transition in dedicated changes.
