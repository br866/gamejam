## Context

The formal route uses flattened level-content prefabs and five additive shared-art scenes. The extraction target is now every object carrying a renderable model that does not already originate from an independent prefab, regardless of whether it is a small prop or a large architectural model group. See `proposal.md` for motivation.

## Goals / Non-Goals

**Goals:**

- Make every non-prefab modeled object editable from an independent prefab source.
- Preserve existing world placement, visual appearance, and scene-level gameplay ownership.
- Keep extraction auditable and incremental while covering all modeled objects.

**Non-Goals:**

- Extract objects without a renderable model component.
- Refactor room architecture, collision roots, shared-art boundaries, or gameplay assets.
- Replace source art meshes, materials, lighting, or scene-specific door behavior.

## Decisions

### Identify modeled objects and existing prefab ownership

The audit records every object with a renderable model component, its mesh/material identity, hierarchy, and existing prefab source. Object names are labels only. Existing independent prefabs are retained as their current source; all other modeled objects receive an extracted prefab.

Name-based grouping was rejected because the current content prefabs include repeated names with different world roles and sometimes different source assets.

### Extract every non-prefab modeled object

Every object carrying a model component and lacking an independent prefab source is extracted, including architecture, walls, floors, large model groups, static props, and non-gameplay decorative doors. Their prefabs preserve renderers and model-local static colliders. Scene-specific interactions and collision proxies remain in the owning scene.

Objects without a model component are excluded because a transform-only root, collider-only proxy, trigger, checkpoint, monster, or gameplay controller does not benefit from a model prefab.

### Use incremental replacement batches

Create an independent prefab for each audited non-prefab modeled object, replace only its audited instance or verified identical instances, validate transforms and references, then move to the next batch. The audit report records excluded non-model and existing-prefab objects.

A single bulk rewrite was rejected because it would create large YAML churn and make an incorrect identity match difficult to review or undo.

### Keep gameplay configuration outside art prefabs

If an art model participates in a formal mechanism, its visual hierarchy may be extracted but `FormalDoor`, triggers, checkpoints, state components, actor/monster configuration, and other scene references remain on scene-owned wrappers.

Embedding level-specific component references in a shared art prefab was rejected because those references cannot safely target objects across scenes.

## Risks / Trade-offs

- [Flattened content hierarchy hides source identity] -> Record mesh/material/hierarchy fingerprints and existing prefab source for every extracted object.
- [Prefab replacement changes transform interpretation] -> Capture and validate each replacement instance's world transform before and after replacement.
- [Static colliders become duplicated or missing] -> Preserve only model-local static colliders and retain scene collision roots as authoritative.
- [Shared scene changes unload behavior] -> Do not move objects between level and shared-art scene ownership in this change.
- [Large asset churn] -> Work in small model batches and inspect each diff before the next batch.

## Migration Plan

1. Produce a read-only model audit classifying every formal object as existing prefab, extractable model, or non-model.
2. Create prefabs for the first audited model batch.
3. Replace only audited instances while preserving transform and static-collider behavior.
4. Run editor validation, inspect scene diffs, and open representative formal levels before proceeding to later batches.
5. Continue until every non-prefab modeled object is extracted; retain excluded non-model objects unchanged.
