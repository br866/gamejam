## 1. Identity Audit

- [x] 1.1 Audit every formal content-prefab and shared-art-scene object for renderable model components, mesh/material identity, hierarchy, and existing prefab source; classify each object as extractable model, existing prefab, or non-model.
- [x] 1.2 Produce an editor-readable extraction report listing every modeled object, its extraction destination or existing prefab source, and excluded non-model objects with reasons.
- [x] 1.3 Confirm that collision roots, gameplay triggers, checkpoints, monsters, scene-specific door actuators, and cross-level shared-art ownership remain scene-owned when their visual model hierarchy is extracted.

## 2. Shared Model Prefabs

- [x] 2.1 Create `Assets/MoMing/FormalLevels/Prefabs/SharedModels/` and extract the first audited non-prefab modeled-object batch with mesh, materials, renderer configuration, hierarchy, and model-local static colliders preserved.
- [x] 2.2 Replace only the audited instances of that batch in formal content prefabs and shared-art scenes, preserving world transform, active state, materials, and scene-owned gameplay components.
- [x] 2.3 Repeat extraction in small verified batches until every non-prefab modeled object, including large architectural model groups, has an independent prefab; do not duplicate existing independent prefabs such as formal doors.

## 3. Validation

- [x] 3.1 Add editor validation for shared model prefab identity provenance, replacement transform preservation, and broken prefab/component references.
- [x] 3.2 Open representative formal levels and every shared-art scene to verify visual placement, static collision, and additive scene ownership after each extraction batch.
- [x] 3.3 Run formal EditMode validation, inspect the Unity Console, and record excluded non-model or existing-prefab objects without modifying them.
