## Context

See `proposal.md` for motivation. The project is Unity 2022.3 targeting PC/Standalone. SuperBreadMan contains roughly 171 FBX files and 562 PNG files, frequently arranged as one model with color, normal, roughness, metallic, and emission textures. Existing texture import settings commonly cap Standalone textures at 2048, but the larger original image files remain under `Assets`, increasing Git checkout size and project disk use. `Art/BackUp` exists outside the Unity project and is empty.

Current level and whitebox changes explicitly exclude art assets. This change must not modify their target scenes or their working files.

## Goals / Non-Goals

**Goals:**

- Keep a local high-quality source hierarchy outside Unity while reducing the size of runtime files tracked under `UnityProject/Assets`.
- Reduce PC texture memory through asset-specific source dimensions and matching Standalone importer settings.
- Preserve Unity references by retaining paths and `.meta` files, and make replacements traceable and reversible.
- Establish a trial-first process that separately handles low-risk static props and high-risk animated or skinned models.

**Non-Goals:**

- Rework shaders, pack texture channels, replace materials, or alter scene rendering.
- Modify active scenes, gameplay logic, controls, collision, navigation, or lighting.
- Automatically bulk-optimize characters, rigged models, animation clips, blend shapes, UI, fonts, or TextMesh Pro assets.
- Remove existing large-file Git history; history cleanup is a separate repository operation requiring explicit coordination.

## Decisions

### Two-tier asset storage

`Art/BackUp` is the local high-quality source store and mirrors each runtime asset's relative path below `UnityProject/Assets`. `UnityProject/Assets` continues to contain only the optimized runtime version. Backups do not receive Unity `.meta` files and are excluded from normal Git tracking.

This keeps high-quality source material recoverable without Unity importing duplicate assets or Git storing both versions. Plain Git tracking was rejected because it defeats the repository-size goal. Git LFS remains the future option if source assets must be shared and versioned.

### Preserve runtime paths and metadata

The optimized output replaces the runtime file in place. Existing `.meta` files are never moved, deleted, regenerated, or copied from the backup hierarchy. Required importer-setting changes are made in the existing `.meta` file, preserving its GUID and existing serialized references.

For FBX files, an unchanged outer GUID is insufficient by itself: re-export can change internal mesh, animation, material, and bone identifiers. Static FBX models are eligible for controlled trials; rigged, animated, or blend-shape models need individual pre/post reference and playback checks. Creating optimized assets at new paths was rejected because it would require scene, prefab, and material reference rewiring.

### Budget by visual role rather than a global scale factor

Texture dimensions and triangle targets are assigned per candidate before processing:

| Class | Color | Normal | Roughness/Metallic | Emission | Model handling |
|---|---:|---:|---:|---:|---|
| Hero / primary enemy | 2048 | 2048 | 1024 | 1024 | Individual only |
| Important near prop | 2048 | 1024 | 1024 | 512 | Conservative static reduction |
| Normal prop | 1024 | 1024 | 512 | 512 | Medium static reduction |
| Small or distant prop | 512 | 512 | 256-512 | 256-512 | Aggressive static reduction |
| Large tiled surface | 2048 | 1024-2048 | 1024 | 512 | Evaluate projected screen area |

The first implementation batch is limited to static `Assets/SuperBreadMan/Scene Model` props and their associated world-space texture sets. Character and creature files under `Assets/SuperBreadMan/human` remain out of bulk scope.

### PC import settings are an independent control

Source resizing reduces Git and disk usage; Unity's Standalone importer controls loaded texture memory and build data. Both are set to the chosen budget. World-space textures retain mipmaps. Color maps are sRGB; normal maps use Unity's Normal Map importer; data maps use linear interpretation. Compression and format changes are only accepted after a Unity visual validation because current shaders may have asset-specific expectations.

### Manifest-driven small batches

Every batch has a tracked manifest, stored with the change or future project asset-optimization records, with:

- Asset ID and runtime path.
- Backup path and a pre-replacement content hash.
- Classification and texture/triangle budget.
- Pre- and post-optimization dimensions, triangle count where applicable, and file size.
- Unity importer settings applied.
- Material, prefab, scene, and animation validation result.
- Rollback status and reviewer acceptance.

Start with one static prop plus its texture set. Expand only to other props of the same classification after measured savings and Unity validation. Blender MCP can produce analyzed or optimized static FBX outputs, while Unity MCP/Editor remains the reference check for imports and serialized links.

## Risks / Trade-offs

- [Re-exported FBX changes internal subasset IDs] -> Restrict initial work to static props; preserve names/material slots/UVs; validate referenced prefabs and scenes before accepting each batch; individually test animation, skeleton, and blend-shape models.
- [Texture resizing makes text or close-up details unreadable] -> Assign budgets by maximum expected screen coverage and compare an in-scene visual baseline before accepting the batch.
- [Incorrect texture type or color space changes material output] -> Record original importer settings, apply type-specific PC settings, and validate every map in its existing material.
- [Backups re-inflate Git or are lost on a new machine] -> Keep `Art/BackUp` ignored locally; use an explicitly configured Git LFS workflow only if sharing becomes necessary.
- [Existing Git history remains large] -> Treat history rewriting or host-side LFS migration as a separate, explicitly approved repository maintenance change.
- [Concurrent level changes create merge or validation noise] -> Do not touch target scene files and keep optimization commits/artifact changes separate from active level work.

## Migration Plan

1. Inventory candidate files and baseline asset disk use and Unity texture memory.
2. Copy originals to the mirrored `Art/BackUp` path, verify hashes, and record a batch manifest.
3. Process one approved static-prop texture/model trial, retaining runtime names, paths, and `.meta` files.
4. Refresh Unity and validate materials, referenced prefabs/scenes, mesh appearance, and relevant import settings.
5. Record size and memory deltas. Accept the batch only if it meets its visual and reference checks.
6. Process additional candidates only by approved class and budget.

Rollback restores the backed-up content to its original `Assets` path while retaining the current `.meta` file, then refreshes and revalidates Unity. If an FBX's internal references fail, immediately roll back that model and mark it for individual treatment.
