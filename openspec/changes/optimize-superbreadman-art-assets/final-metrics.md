# Final Optimization Metrics

## Static Scene Models

| Metric | Before | After | Result |
|---|---:|---:|---|
| Eligible FBX count | 73 | 73 | Optimized |
| Source FBX size | 690.92 MB | 8.70 MB | 650.61 MiB saved |
| Triangle count | 339,136 | 162,105 | 52.2% reduction |
| Low-triangle FBX count | 87 | 87 | Backed up; not decimated |
| Unity model assets resolving after reimport | 160 | 160 | No missing assets |

## Static Scene Textures

| Metric | Before | After | Result |
|---|---:|---:|---|
| Texture count | 529 | 529 | Processed and backed up |
| Resized textures | N/A | 524 | Color/normal capped at 1024; data/emission capped at 512 |
| Source texture size | 1,585.11 MB | 328.00 MB | 1,257.11 MiB saved |
| Normal Map importers | N/A | 103 | All detected normal textures use Normal Map import |
| Linear data textures | N/A | 319 | Metallic, roughness, and mask data use linear import |

## PC Build

| Metric | Result |
|---|---|
| Target | StandaloneWindows64 |
| Build status | Succeeded |
| Build duration | 110.77 seconds |
| Total output size | 77.63 MB |
| Errors | 0 |
| Warnings | 0 |

## Validation

- All 160 `Scene Model` FBX assets reimported and resolve in Unity.
- `superbreadman 1` has 1,363 Renderer components with no missing material or Shader binding.
- Unity Console reported no errors after model and texture reimport.
- The project owner manually reviewed the optimized static scene assets and accepted their visual quality under the performance-priority policy.
- Original source assets are mirrored below `Art/BackUp` and excluded from ordinary Git tracking.
- The eight skinned/animated human models remain unmodified; see `high-risk-model-plan.md`.
