## Context

The art scene contains 1,363 renderers and many repeated prefab instances, but no serialized scene object currently has static flags. URP's SRP Batcher is already enabled in every project renderer configuration, which improves CPU submission overhead but does not itself combine draw calls. The scene also includes interactive route content alongside static environment art, so broad static marking is unsafe.

## Goals / Non-Goals

**Goals:**
- Reduce rendering submissions where Unity's static batching can safely apply them.
- Preserve existing GameObjects, prefab instances, and their ability to be edited later.
- Use reproducible measurements rather than object-count assumptions to assess benefit.
- Remove confirmed-unused animation import work from static scene-model FBXs.

**Non-Goals:**
- Guarantee an absolute batch-count target before representative hardware measurements exist.
- Optimize the whitebox scene or unrelated MoMing reference scene.
- Rebuild lightmaps, navigation data, collision meshes, or occlusion data except where Unity requires validation of existing data.

## Decisions

### Apply static batching by verified renderer eligibility

Existing renderers will be classified before any static flag change. The eligible class is limited to transform-invariant, non-interactive environment and decorative art. Dynamic and gameplay classes remain unmodified.

This retains the source GameObjects and prefab instances, unlike manual mesh combining. A whole-scene static mark was rejected because it would silently include route mechanisms and actors.

### Measure first and compare equivalent viewpoints

Profiler and Frame Debugger records from fixed, representative scene viewpoints are the acceptance evidence. Measurements include Batches, SetPass Calls, triangles, and CPU/GPU frame time when available.

Draw calls may remain separate for distinct materials, lightmap data, shader variants, and rendering state. Renderer count and repeated prefab counts are therefore not accepted as performance evidence. An absolute draw-call budget was rejected until target hardware and a baseline are established.

### Consolidate only equivalent material references

Material reference consolidation is allowed only when the candidate materials have the same shader, texture assignments, scalar and color parameters, enabled keywords, render queue, surface and blend state, shadow-related settings, and other serialized rendering properties. Equivalent renderer material slots can be redirected to one shared material asset, and duplicate slots within a model can be removed only when they resolve to that same equivalent material without changing submesh appearance.

This preserves source mesh topology, UVs, material appearance, and prefab hierarchy. Creating a visually approximate shared material was rejected because it would hide an art change inside a performance task. Frame Debugger evidence determines whether a material split is worth addressing.

### Keep batching and material-reference work separate from future structural work

GPU instancing, Shader Graph changes, mesh combination, atlas generation, UV changes, cross-appearance material unification, and prefab restructuring are deferred. They can reduce more draw calls, but require different compatibility checks and constrain authoring flexibility more than static flags and equivalent-reference consolidation.

### Disable FBX animation import only with affirmative eligibility

`importAnimation` is disabled only after confirming the FBX belongs to static art and does not require animation. Uncertain assets retain their importer setting. This prevents an importer-wide change from damaging doors or other assets that may need animation later.

## Risks / Trade-offs

- [A static renderer moves through a script, parent, animation, or interaction] -> Inspect components and runtime behavior before marking it static; exclude uncertain objects.
- [Static batching increases build/runtime mesh memory] -> Compare memory and frame metrics at representative viewpoints; revert classifications that do not provide a worthwhile result.
- [Lighting, shadows, colliders, or route behavior regress] -> Exercise the route and inspect affected art areas after each bounded batch of changes; restore prior settings on regression.
- [Scene edits create large, noisy serialized diffs] -> Perform edits through Unity and review scene diffs after each logically grouped pass.
- [Frame Debugger identifies material-state fragmentation as the dominant cause] -> Record the evidence and defer material/instancing work rather than expanding this change.
- [Materials that look similar differ in hidden serialized properties] -> Compare complete serialized rendering state and preserve separate assets unless full equivalence is proven.
- [Removing a redundant material slot changes a submesh assignment] -> Verify the mesh renderer and visual output before accepting the slot reduction; restore the original assignment on any difference.

## Migration Plan

1. Capture a pre-change measurement record and classify candidate renderers and FBXs.
2. Identify Frame Debugger-confirmed material splits and consolidate only fully equivalent material references or redundant slots, verifying each bounded group in Play Mode.
3. Apply static flags in bounded environment/decorative groups, verifying each group in Play Mode.
4. Disable animation import on affirmatively eligible static FBXs and verify references after reimport.
5. Capture post-change measurements, validate route and build health, and document deferred opportunities.
6. Roll back a group by restoring its prior material references, static flags, or importer setting if it regresses behavior, presentation, or measured performance.
