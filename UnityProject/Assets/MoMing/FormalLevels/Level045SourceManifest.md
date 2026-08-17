# Formal Level 4.5 Art Manifest

Source scene: `Assets/Scenes/Test/superbreadman 1.unity`

## User-Selected Assembly

- Active Unity selection contained `148` objects with `149` Renderer components.
- The selection is authoritative and is intentionally treated broadly; selected adjacent Level 4 edge visuals remain included for later manual review.
- `147` selected visual Renderers were copied into `Prefabs/L045_Content.prefab`.
- Two PlayerSystem player renderers were excluded so FormalPersistent remains the only owner of formal player actors.
- Prototype scripts, colliders, rigidbodies, audio, navigation, player objects, triggers, and mechanic components were stripped from copied content.

## Global Transform Verification

- Each Renderer was flattened and explicitly assigned its selected source world Position, Rotation, and Lossy Scale.
- Immediate source-to-Prefab verification matched `147/147` Renderers by world Bounds center and size.
- Maximum source-to-Prefab delta: Bounds center `0.000000`, Bounds size `0.000006` Unity units.
- FormalLevel045 instantiates `L045_Content` below `Level045ContentRoot` without player actors.
- Prefab-to-scene verification matched `147/147` Renderers with zero world Bounds center and size delta.

## Scene Registration

- `FormalLevel045.unity` is registered in Build Settings.
- The art-only scene contains no local Camera, so it cannot introduce a competing runtime view.

## Corridor Wall Completion

- A world-Bounds audit of the Level 4.5 corridor identified five source wall tiles missing from the original user-selected content.
- Added wall tiles at world centers `(-132.71, 17.02, 5.83)`, `(-131.94, 17.00, -6.04)`, `(-199.05, 17.02, 5.83)`, `(-199.25, 17.00, -6.07)`, and `(-205.72, 24.58, 0.25)`.
- FormalLevel045 now contains `152` Renderer components.
- Re-audit finds all `17/17` source corridor wall tiles represented in FormalLevel045 with maximum Bounds center delta `0.000000` and size delta `0.000006`.

## West Door And Traversal Foundation

- Added the selected west doorway visuals `door4 (4)` and `door4 jamb (4)` at their source world Bounds. FormalLevel045 contains one matching Renderer for each selected object.
- FormalLevel045 now contains `156` Renderer components.
- `L045_CollisionRoot` owns one corridor floor and four outer boundary BoxColliders; the scene has `5` foundational Collider components.
- HumanRespawnAnchor: `(-128.50, 10.86, -2.00)`.
- DogRespawnAnchor: `(-130.00, 10.86, -2.00)`.
- Both anchors have floor support, no blocking player-capsule overlap, and clear two-unit movement in all four horizontal directions.
- FormalLevel045 remains an art and traversal-foundation scene. It is not yet connected to FormalPersistent flow because Level 4.5 checkpoint/exit progression remains deferred.

## Broad Collider Coverage

- Added `71` scene-owned non-trigger BoxCollider proxies below `L045_CollisionRoot/L045_BroadColliderCoverage`: `46` architecture proxies and `25` substantial fixed-prop proxies.
- This includes `20` wall tile proxies, correcting the initial classification that treated all tile names as floor decoration.
- FormalLevel045 now contains `76` Collider components: `5` foundational floor/boundary colliders plus broad static coverage.
- Kept `78` visual-only objects non-blocking, including floor tiles, lights, signs, pictures, bottles, buttons, pedals, plates, pads, footprints, pickups, switches, player/monster visuals, trays, restroom decoration, carpets, pushable boxes, and small clutter.
- Excluded `4` overhead visuals and `3` near-zero-size visuals from blocking collision.
- Human respawn remains supported without overlap and has clear two-unit movement west, north, and south. Dog respawn remains supported without overlap and has clear two-unit movement in all four horizontal directions.

## Deferred Follow-Up

- Add a Level 4.5 traversal foundation, spawn anchors, collision, and direct formal-flow entry in a dedicated change.
- Review the broad visual selection and manually remove unwanted adjacent Level 4 content before detailed collision or mechanics work.
