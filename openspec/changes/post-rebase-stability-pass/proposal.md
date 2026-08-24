# Post-Rebase Stability Pass

## Why

The origin/main merge brought a comprehensive animation pass that converted every character animation FBX to Humanoid muscle curves, while our formal-route character models still use Generic avatars (player) and no avatar at all (L02 monster). The result: animator state machines play, but skeletons are frozen — player walk/idle and all six monster3 states are visually dead. In the same window the physics crate rewrite exposed two gameplay gaps (crate can be shoved by walking into it while disengaged; L05's doors stand on open floor with no physical rooms), and the art assets still carry 100x-800x transform scales that make scene math fragile.

## What Changes

- **Character rig alignment (Option A)**: Convert the player visual model (`Push_and_Walk_Forward` FBX) and the L02 monster model (doctor3 Walking FBX instance) to Humanoid rig type so the teammate-authored Humanoid clips drive them via muscle retargeting. Dog stays Generic (its only clip is bone-path Generic and works).
- **Crate engagement gating**: `FormalPushableCrate` reverts to kinematic while disengaged; only an F-engaged human (plus dog when `requiredPushers=2`) unlocks physics movement.
- **Cooperative cabinet variant**: Add a fixed push-axis configuration (enum/vector) and optional travel limit to `FormalPushableCrate`, so an L05-style cabinet can be authored as the same component with `requiredPushers=2`.
- **L05 physical separation**: Add whitebox interior walls splitting the corridor from the final hall and the right/left room halves; existing `L05_LeftRoomDoor` and `L05_FinalDoor` become real passage gates. Monster navigation areas re-verified after the walls land.
- **Scale normalization batch (careful mode)**: Run the existing leaf-mesh normalizer across formal-level scenes with a pre-flight inventory, per-node journal, and one-command rollback per scene. Prefab-level normalization for shared art props so all instances inherit.

## Capabilities

### New Capabilities
- `character-animation-rig`: Character model rigs must match their animation clip rig type (Humanoid clips require Humanoid avatars) so state-driven animations actually deform skeletons.
- `pushable-crate-engagement`: Pushable crates are immovable while disengaged and only move under physics while an eligible pusher set is engaged, with fixed-axis cooperative variants.
- `scale-normalization-batch`: Batch scale normalization of static leaf meshes is journal-logged and reversible per scene, without breaking colliders, hierarchies, or prefab instances.

### Modified Capabilities
- `formal-level05-traversal-foundation`: The final stage gains physically separated rooms; the left-room and final doors gate real passages instead of decorating open floor.

## Impact

- `UnityProject/Assets/SuperBreadMan/human/boy3/*.fbx.meta` (player model import settings), monster model instance in `FormalLevel02.unity`, `FormalPlayerActor`/`MonsterAnimatorDriver` (no code change expected — state names unchanged).
- `FormalLevel01/FormalPushableCrate.cs` (engagement gating + axis/limit fields), `FormalPlayerControl` (no change expected).
- `FormalLevel05.unity` (interior walls, door repositioning), `LevelMonsterNavigation` areas for L05 monsters.
- New baked-mesh assets under `Assets/MoMing/BakedMeshes/` plus a rollback journal per scene; shared art prefabs under `FormalLevels/Prefabs/SharedModels/`.
- Risk concentration: Humanoid avatar auto-mapping may fail or shift visual pose on Meshy rigs; fallback is reverting clip FBXes to Generic (Option B), kept cheap by journalling.
