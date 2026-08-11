## Context

The repository contains multiple similarly named SuperBreadMan scenes and extensive historical MoMing design documentation. The target whitebox and art scenes are under `Assets/Scenes/Test/`, not `Assets/MoMing/Scenes/Test/`. The two target scenes are intended as paired versions of one level, while their existing content and later navigation work must remain intact.

## Goals / Non-Goals

**Goals:**
- Establish a single source of truth for the paired scenes' required route and constraints.
- Let configuration-only work make existing main-route objects function consistently.
- Use the whitebox scene as the first verification reference, then check the matching art scene.

**Non-Goals:**
- Redesign the level or reconcile it with older room-reference documents.
- Add, remove, duplicate, move, or rotate scene objects.
- Change code, controls, collision, player navigation, monster navigation, UI, art, lighting, or audio assets.
- Integrate the scenes into the legacy build sequence.

## Scene Scope

```text
Whitebox reference
Assets/Scenes/Test/superbreadman.unity
             |
             | same route and existing interactions
             v
Art scene
Assets/Scenes/Test/superbreadman 1.unity

Required route:
Level1 -> Level2 -> Level3 -> Level4 -> Level4.5 -> Level5 -> Exit
```

The whitebox scene is inspected and verified first. An accepted whitebox configuration is then compared against the art scene's corresponding existing objects. The art scene differs only in presentation, not required route logic.

## Allowed Configuration Surface

| Allowed | Not allowed |
| --- | --- |
| Existing component references | Runtime script edits |
| Existing component field values | New, copied, or deleted objects |
| Tags and layers | Transform position or rotation changes |
| Active states and component enabled states | Collision and navigation changes |
| Existing gate, switch, plate, checkpoint, monster, and exit configuration | UI, audio assets, art, material, lighting, and documentation changes |

If an issue needs any prohibited change, it is recorded as a blocker. It is not worked around by altering unrelated configuration.

## Main-Route Test Model

```text
Start
  -> linked box interaction
  -> character switch and separation
  -> human-only interaction
  -> dog ability
  -> monster threat
  -> existing checkpoint
  -> sequence gate
  -> final exit
```

This is gameplay coverage, not an assertion that each item belongs to a specific room before scene inspection confirms it. Existing checkpoint location is authoritative; it must not be moved to optimize difficulty.

## Failure and Future Navigation

Current failures use the existing checkpoint behavior. Existing player and monster controls remain unchanged even if their direct-movement behavior has limitations. A later navigation-focused change will address player movement integration, collision, and navigation; this change must not preempt that design.

## Documentation References

- `Assets/MoMing/Docs/02-核心玩法.md`: historical core mechanic reference.
- `Assets/MoMing/Docs/03-关卡结构.md`: historical room-structure reference.
- `Assets/MoMing/Docs/04-场景清单.md`: historical MoMing scene inventory, not the target-scene authority.
- `Assets/MoMing/Docs/05-脚本说明与道具制作指南.md`: historical script and prefab reference.
- `Assets/MoMing/Docs/06-音效需求清单.md`: future audio integration reference.

These documents are retained and consulted only when relevant. They do not override the target scenes' existing route.
