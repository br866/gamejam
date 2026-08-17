# Formal Level 5 Art Manifest

Source scene: `Assets/Scenes/Test/superbreadman 1.unity`

## User-Selected Assembly

- Active Unity selection contained `113` objects with `114` Renderer components.
- The selection is authoritative and is intentionally preserved broadly for later manual visual review.
- `112` selected visual Renderers were copied into `Prefabs/L05_Content.prefab`.
- Two PlayerSystem player renderers were excluded so FormalPersistent remains the only owner of formal player actors.
- Prototype scripts, colliders, rigidbodies, audio, navigation, player objects, triggers, and mechanic components were stripped from copied content.

## Global Transform Verification

- Each Renderer was flattened and explicitly assigned its selected source world Position, Rotation, and Lossy Scale.
- Immediate source-to-Prefab verification matched `112/112` Renderers by world Bounds center and size.
- Maximum source-to-Prefab delta: Bounds center `0.000001`, Bounds size `0.000010` Unity units.
- FormalLevel05 instantiates L05_Content under Level05ContentRoot without player actors.
- Prefab-to-scene verification matched `112/112` Renderers with zero world Bounds center and size delta.

## Scene Registration

- FormalLevel05 is registered in Build Settings.
- The art-only scene contains no local Camera, so it cannot introduce a competing runtime view.

## Deferred Follow-Up

- Add Level 5 traversal anchors, collision, player entry, mechanics, and final exit behavior in dedicated changes.
- Review the broad selected visual layout and manually remove unwanted adjacent content before detailed collision work.

## Traversal And Collider Coverage

- `L05_CollisionRoot` owns one foundational floor and four outer boundary BoxColliders for the Level 5 hospital region.
- HumanSpawn: `(-231.00, 10.86, 1.00)`.
- DogSpawn: `(-229.50, 10.86, 1.00)`.
- Added `49` scene-owned non-trigger BoxCollider proxies: `25` architecture proxies and `24` substantial fixed-prop proxies.
- FormalLevel05 now contains `54` Collider components.
- Kept `37` visual-only objects non-blocking, including floors, lights, signs, pictures, bottles, buttons, plates, pads, footprints, pickups, switches, player/monster visuals, carpets, pushable boxes, and small details. Excluded `7` overhead visuals and `6` near-zero-size visuals.
- Both anchors have floor support and no blocking capsule overlap. Human has clear two-unit movement east and south; dog has clear two-unit movement in all four horizontal directions.
- FormalPersistent is configured with `FormalLevel05` as the active direct-test startup scene. Runtime verification was subsequently confirmed: the persistent formal human/dog pair loads correctly into Level 5.
