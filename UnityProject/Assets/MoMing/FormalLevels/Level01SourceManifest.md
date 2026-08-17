# Level 1 Source Manifest

Source scene: `Assets/Scenes/Test/superbreadman 1.unity`

Migration selection captured on 2026-08-13: 115 GameObjects. After review, `wall3 (3)` was identified as a selected source object omitted from that selection snapshot and was added separately to the formal architecture prefab. A later review also explicitly added `door2` from `Level3/door2` as a Level 1 art exception.

The formal assembly is now instantiated into `FormalLevel01` as `Prefabs/L01_Content.prefab`. It contains the selected art grouped as `Architecture`, `SetDressing`, and `GameplayVisuals`; the Level 1 scene retains only the prefab instance and level configuration roots.

## Confirmed Gameplay Sources

- `door5 (1)`: mechanism door visual.
- `door4 (1)`: Level 2 exit-door visual.
- `wooden_crate (1)`: human-pushable physical-step visual.
- `Pedal1 (1)`: mechanism visual.
- `key`: human-only key visual.
- `door2`: additional Level 1 door visual explicitly selected after the initial migration, despite its legacy `Level3/door2` source path.

## Confirmed Set Dressing

- `low wooden stool (1)` and `stool (1)`: blocking decoration only.
- `cabinet*`, `medical cabinet*`, and `big metal locker*`: blocking decoration only.
- `Pedal2 (1)`: non-interactive decoration.

## Late Additions

- `wall3 (3)`: selected structural art added to `L01_HospitalArchitecture.prefab` after the initial 115-object snapshot was reviewed.

## Excluded From Formal Level 1 Migration

- `HoldSwitch`, `PuzzleSwitch`, `PressurePlate`, and both `PickupItem` objects: prototype gameplay only.
- `Checkpoint` and `Checkpoint (1)`: prototype checkpoint objects; the formal scene owns its own checkpoint.
- `PlayerSystem`, `boy`, and `dog`: persistent-scene content.

## Collision Policy

- Formal collision uses dedicated primitive/compound proxy objects, not render-mesh colliders.
- Dynamic `wooden_crate (1)` uses a primitive BoxCollider and Rigidbody.
- Small props remain non-blocking unless playtesting identifies a navigation or traversal requirement.

## Traversal Anchor Verification

- Human entrance: `(30.77, 10.90, -4.76)` on `Collision_Floor_West`.
- Dog entrance: `(32.27, 10.90, -4.76)` on `Collision_Floor_West`.
- Human checkpoint respawn: `(44.50, 10.85, -5.20)` on `Collision_Floor_East`.
- Dog checkpoint respawn: `(46.00, 10.85, -5.20)` on `Collision_Floor_East`.
- Entrance and checkpoint anchors have ground support and no blocking capsule overlap for either formal player actor.
- Direct entrance-to-checkpoint capsule checks pass for both actors; runtime movement remains grounded on the Level 1 floor collision.
- The checkpoint-to-exit segment is intentionally blocked by `L01_ExitDoor_ToLevel02` while the exit is closed. Door-opening behavior remains Level 1 mechanism scope.
- Formal Play Mode confirms both actors spawn grounded with zero velocity at their entrance anchors. Checkpoint reset returns both actors to their separate checkpoint anchors.
