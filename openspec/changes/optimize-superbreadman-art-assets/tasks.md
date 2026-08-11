## 1. Inventory and Source-Asset Controls

- [x] 1.1 Inventory candidate FBX and texture files, including runtime path, file size, texture dimensions, triangle count where applicable, imported texture type, and whether each model is static, skinned, animated, or uses blend shapes.
- [x] 1.2 Measure and record the baseline `UnityProject/Assets` art-asset disk usage, a representative Unity PC texture-memory snapshot, and the initial Git working-tree size for the approved asset roots.
- [x] 1.3 Add and verify the local `Art/BackUp` ignore policy so high-quality source assets are not ordinarily tracked by Git, while documenting Git LFS as the required future option for shared source assets.
- [x] 1.4 Define the mirrored `Art/BackUp` path convention and create a batch manifest template containing source/runtime paths, hashes, classifications, budgets, sizes, importer settings, and validation outcomes.

## 2. Classification and Optimization Budgets

- [x] 2.1 Classify `Assets/SuperBreadMan/Scene Model` candidates by visual role and maximum expected screen use; assign the approved texture dimension and static-model triangle budgets.
- [x] 2.2 Identify and exclude UI, TextMesh Pro, shader, material, scene, and unapproved asset roots from bulk optimization.
- [x] 2.3 Identify all skinned, animated, and blend-shape assets under `Assets/SuperBreadMan/human`; record them for individual review and exclude them from the initial batch.
- [x] 2.4 Select one low-risk static prop and its complete world-space texture set as the first trial batch; record its before-state in the manifest.

## 3. Trial Batch Optimization

- [x] 3.1 Copy and hash the trial batch's unmodified runtime files into the corresponding `Art/BackUp` paths before any replacement.
- [x] 3.2 Produce resized trial textures according to their individual color, normal, data, and emission budgets without changing runtime filenames or paths.
- [x] 3.3 Configure the trial textures' Unity Standalone import settings for the recorded budget, mipmap use, and correct color/normal/data interpretation.
- [x] 3.4 Use Blender or Blender MCP to inspect and, if warranted, produce an optimized version of the trial static FBX while preserving required object names, material slots, UVs, and geometry orientation.
- [x] 3.5 Replace only approved trial runtime files in place, retaining each existing `.meta` file and GUID while applying approved importer-setting changes; refresh Unity and record post-optimization dimensions, triangle counts, hashes, and file sizes.

## 4. Validation and Batch Expansion

- [x] 4.1 Verify the trial prop's materials, texture assignments, prefabs, and scenes resolve with no missing references after Unity reimport.
- [x] 4.2 Inspect the trial asset in its intended scene context for texture, UV, normal, transparency, lighting, and mesh silhouette regressions.
- [x] 4.3 Compare the trial batch's source file size, Git working-tree size, Unity texture-memory measurements, and PC build contribution where available against the recorded baseline.
- [x] 4.4 Restore the trial asset from `Art/BackUp` in a rollback rehearsal while retaining `.meta` files; verify Unity references resolve after the restore.
- [x] 4.5 Accept the static-prop class under the project owner's performance-priority decision, with complete backup, reimport, reference, and rollback evidence and with visual degradation explicitly permitted.
- [x] 4.6 Process additional static-prop batches only after each uses the approved classification, manifest, backup, in-place replacement, and validation workflow.

## 5. Approved High-Triangle FBX Trial

- [x] 5.1 Create and hash a mirrored `Art/BackUp` copy of the approved electro-medical cabinet FBX before replacement.
- [x] 5.2 Produce a 30-40% triangle static-FBX candidate in Blender while retaining the source object name, material slot, UV layer, and orientation.
- [x] 5.3 Replace the approved cabinet FBX in place while retaining its `.meta` file and GUID; refresh Unity and record the post-optimization file size and triangle count.
- [x] 5.4 Validate cabinet instances, material binding, and the target art scene after reimport; perform and record a rollback rehearsal.
- [x] 5.5 Record the cabinet trial's accepted performance-priority trade-off: rear-detail degradation is permitted in exchange for the measured triangle and source-size reduction.

## 6. High-Risk Assets and Completion

- [x] 6.1 For each requested skinned or animated model, create an individual optimization plan covering bones, mesh names, material slots, animation clips, blend shapes, and playback/reference validation before editing it.
- [x] 6.2 Validate all accepted batches in a PC/Standalone build and record aggregate source-disk, Git, runtime-memory, and build-size results.
- [x] 6.3 Record assets that require material, shader, scene, UI, collision, navigation, or repository-history changes as separately scoped follow-up work.
