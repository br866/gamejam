## Why

The SuperBreadMan art assets include many large FBX and PNG files, increasing Git checkout size and Unity texture memory beyond what the PC-targeted scenes need. The project needs a repeatable way to retain high-quality source assets outside Unity while replacing only approved runtime assets with optimized versions.

## What Changes

- Establish an asset-optimization workflow for SuperBreadMan models and textures that keeps editable high-quality source assets under `Art/BackUp` and optimized runtime assets at their existing Unity paths.
- Define classification, optimization budgets, and validation requirements for texture sets, static models, and higher-risk skinned or animated models.
- Preserve existing Unity asset GUIDs by retaining `.meta` files when optimized runtime files replace their source-path counterparts.
- Require traceable manifests, batch-level measurements, Unity reference checks, and a rollback path before each optimization batch is accepted.
- Exclude active level-layout changes, runtime scripts, materials, shaders, and scene object changes from the asset-optimization pass.

## Capabilities

### New Capabilities
- `superbreadman-art-asset-optimization`: Defines source-asset retention, safe runtime-asset replacement, resource classification, and validation for reduced project size and Unity memory use.

### Modified Capabilities
- None.

## Impact

- Source backups: `Art/BackUp/`, outside Unity asset import scope and excluded from ordinary Git tracking unless later moved to Git LFS.
- Runtime art assets: `UnityProject/Assets/SuperBreadMan/` and other approved asset paths, including their existing `.meta` files.
- Unity import configuration for approved model and texture assets, with Standalone/PC settings as the target.
- Blender and Blender MCP may be used for model analysis and optimized static-model exports; Unity MCP and the Unity Editor verify imports and references.
- The in-progress SuperBreadMan whitebox/navigation and level-alignment changes remain independent and are not modified by this change.
