## 1. Baseline and Eligibility Inventory

- [ ] 1.1 Select and document representative art-scene viewpoints that cover an enclosed room, a corridor or long sightline, dense static decoration, and active route content.
- [ ] 1.2 Capture pre-change Profiler and Frame Debugger records for every selected viewpoint, including Batches, SetPass Calls, triangles, CPU frame time, and GPU frame time when available.
- [ ] 1.3 Classify every candidate art renderer as eligible static environment/decor, excluded dynamic or interactive content, or unresolved; record the evidence for each exclusion and unresolved item.
- [ ] 1.4 Inventory `Assets/SuperBreadMan/Scene Model/` FBXs with animation import enabled and classify each asset's animation eligibility before changing importer settings.
- [ ] 1.5 Use Frame Debugger evidence to inventory material-state splits and candidate redundant material slots in the representative viewpoints.

## 2. Equivalent Material-Reference Consolidation

- [ ] 2.1 Compare every candidate material's complete serialized output-affecting state and classify it as fully equivalent, non-equivalent, or unresolved.
- [ ] 2.2 Redirect only fully equivalent art-scene renderer references to one shared material asset, retaining separate material assets for non-equivalent and unresolved candidates.
- [ ] 2.3 Remove only verified redundant renderer material slots, and verify every affected submesh retains its prior visual result.
- [ ] 2.4 Inspect each bounded material-reference diff in Unity to confirm that it does not modify material appearance or parameters, shaders, mesh topology, UVs, or prefab hierarchy.

## 3. Static-Batching Preparation

- [ ] 3.1 Enable static batching only for the verified immobile environment and decorative renderer groups in `Assets/Scenes/Test/superbreadman 1.unity`.
- [ ] 3.2 Confirm characters, monsters, doors, route gates, switches, plates, boxes, checkpoints, exits, triggers, animated content, and unresolved objects remain non-static.
- [ ] 3.3 Review each bounded scene diff to confirm no hierarchy, transform, component reference, collider, navigation, lighting setup, material appearance, Shader Graph, or prefab-structure changes were introduced.

## 4. Static Model Importer Hygiene

- [ ] 4.1 Disable animation import for each affirmatively verified static scene-model FBX and reimport it through Unity.
- [ ] 4.2 Preserve animation import on all animation-required and unresolved FBXs; record deferred eligibility decisions.
- [ ] 4.3 Verify affected models resolve with no missing mesh, material, or shader bindings after reimport.

## 5. Validation and Follow-Up

- [ ] 5.1 Exercise the existing art-scene main route and inspect changed areas for visual, lighting, shadow, collision, route, and Console-error regressions.
- [ ] 5.2 Capture post-change Profiler and Frame Debugger records from the same viewpoints and compare them to the baseline.
- [ ] 5.3 Build StandaloneWindows64 and confirm the build completes without errors.
- [ ] 5.4 Record measured gains, no-gain or regressing groups, and deferred GPU instancing, mesh combination, Shader Graph, atlas, UV, cross-appearance material-unification, and prefab-restructuring opportunities.
