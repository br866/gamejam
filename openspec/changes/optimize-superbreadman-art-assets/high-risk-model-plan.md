# High-Risk Model Plan

The following `Assets/SuperBreadMan/human` models remain excluded from automatic optimization because their Unity import metadata enables animation and blend-shape import:

| Model | Required individual validation before replacement |
|---|---|
| `boy/boy.fbx` | Skeleton hierarchy, humanoid Avatar, clips, blend shapes, player prefab references, and playback |
| `boy 1/boy.fbx` | Skeleton hierarchy, humanoid Avatar, clips, blend shapes, player prefab references, and playback |
| `boy 1/push.fbx` | Push animation clip names, timing, avatar linkage, blend shapes, and playback |
| `boy3/Meshy_AI_Meshy_AI_Hooded_Still_biped_Animation_Push_and_Walk_Forward_withSkin.fbx` | Skeleton hierarchy, embedded animation clips, blend shapes, avatar mapping, and playback |
| `dog/dog.fbx` | Skeleton hierarchy, clips, blend shapes, character prefab references, and playback |
| `monster 1/monster 1.fbx` | Skeleton hierarchy, clips, blend shapes, enemy prefab references, and playback |
| `monster 2/monster 2.fbx` | Skeleton hierarchy, clips, blend shapes, enemy prefab references, and playback |
| `monster3/monster3.fbx` | Skeleton hierarchy, clips, blend shapes, enemy prefab references, and playback |

These models must receive an individual Blender/Unity trial with a backup, preserved names and bones, Avatar and animation checks, scene playback review, and a rollback rehearsal. They are not included in the current static-scene-model performance pass.

# Follow-Up Boundaries

- Shader Graph or material edits, including texture-channel packing, are excluded because they would change rendering contracts.
- UI and TextMesh Pro textures are excluded because their readability and atlas behavior require separate validation.
- Scene transforms, colliders, navigation, lighting, and gameplay scripts remain unchanged.
- Git history still contains previous large assets. Reducing historical repository size requires a separately approved history rewrite or Git LFS migration.
