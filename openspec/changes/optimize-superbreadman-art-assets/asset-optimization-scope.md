# Initial Optimization Scope

## Approved Runtime Root

The initial optimization pass is limited to static world assets below:

`UnityProject/Assets/SuperBreadMan/Scene Model/`

The root currently contains 160 FBX files and 529 image files, occupying approximately 2.87 GiB on disk. Initial prioritization is by repeated high-size static props, not by a global reduction ratio.

## Initial Texture Budgets

| Asset classification | Color | Normal | Metallic/Roughness | Emission | Static model target |
|---|---:|---:|---:|---:|---|
| Important near prop | 2048 | 1024 | 1024 | 512 | Preserve silhouette; set an inspected triangle target before export |
| Normal prop | 1024 | 1024 | 512 | 512 | Medium reduction after inspected triangle count |
| Small or distant prop | 512 | 512 | 256-512 | 256-512 | Aggressive reduction after inspected triangle count |
| Large tiled surface | 2048 | 1024-2048 | 1024 | 512 | Evaluate separately from small props |

The currently selected trial candidate is the normal-prop texture set at:

`UnityProject/Assets/SuperBreadMan/Scene Model/Level1/wooden_crate/`

The trial includes the color, normal, metallic, roughness, and emission textures. Its FBX is not approved for replacement until Blender inspection confirms it has no rig, animation, blend shape, or required embedded subasset dependency.

The separately approved high-triangle static-FBX trial is:

`UnityProject/Assets/SuperBreadMan/Scene Model/Level2/electro-medical_cabinet/electro-medical_cabinet.fbx`

Blender inspection measured 39,254 triangles, one material slot, one UV layer, no armature, no actions, and no blend shapes. The trial target is 30-40% of the original triangles, subject to Unity reference and visual validation. It is not an acceptance of the normal-prop class and does not permit a bulk FBX operation.

## Exclusions

- `UnityProject/Assets/SuperBreadMan/human/`: Contains eight FBX files whose current import metadata enables animation and blend-shape import. These are individual-review assets, not bulk candidates.
- `UnityProject/Assets/Scenes/`: Scenes and their object layouts are excluded.
- `UnityProject/Assets/Prefabs/`: Prefabs are validation targets only.
- `UnityProject/Assets/TextMesh Pro/`, UI assets, fonts, and sprites: excluded.
- `.mat`, `.shadergraph`, and other material or shader assets: excluded.
- Runtime scripts, colliders, navigation, lighting, and the active SuperBreadMan route changes: excluded.

## Required Tool Evidence Before Replacement

- Unity Editor: texture dimensions, texture type, platform import settings, material references, missing-reference check, and PC texture-memory measurement.
- Blender: static FBX triangle count, object hierarchy, material slots, UV presence, and whether the FBX contains animation, armatures, or blend shapes.
- Visual validation: the intended art scene view for the trial candidate before and after replacement.
