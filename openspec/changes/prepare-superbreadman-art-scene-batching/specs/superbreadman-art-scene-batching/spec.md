## Purpose

Defines safe, measured static-batching preparation for the SuperBreadMan art scene while preserving its route behavior and future rendering-optimization options.

## ADDED Requirements

### Requirement: Measured art-scene rendering baseline

The project SHALL record comparable pre-change and post-change rendering metrics for representative viewpoints in `Assets/Scenes/Test/superbreadman 1.unity`. The recorded metrics SHALL include Batches, SetPass Calls, triangle count, CPU frame time, and GPU frame time where the target hardware exposes it.

#### Scenario: Compare representative viewpoints
- **WHEN** a developer captures rendering data before and after batching preparation from the defined viewpoints
- **THEN** the records identify the viewpoint, build or editor context, and each required metric so the change can be evaluated against the same workload

### Requirement: Safe static-batching eligibility

The project SHALL enable static batching only for existing art-scene renderers verified to remain transform-invariant at runtime and to have no gameplay, route, interaction, actor, trigger, or animation responsibility.

The project SHALL exclude characters, monsters, doors, route gates, switches, pressure plates, boxes, checkpoints, exits, triggers, and any renderer with runtime transform or animation behavior from static batching.

#### Scenario: Classify an immobile environment renderer
- **WHEN** an existing wall, floor, ceiling, fixed pipe, or fixed decorative renderer is verified not to change at runtime
- **THEN** it is eligible for static-batching preparation without changing its object structure or transform

#### Scenario: Encounter dynamic or interactive scene content
- **WHEN** a renderer belongs to an actor, movable object, route mechanism, interaction, trigger, or animated object
- **THEN** it remains excluded from static-batching preparation

### Requirement: Preserved art-scene behavior and presentation

Static-batching preparation SHALL preserve the art scene's existing main route, component references, collision behavior, baked-lighting appearance, shadow behavior, and visual presentation.

#### Scenario: Validate after batching preparation
- **WHEN** a developer exercises the existing art-scene route after batching preparation
- **THEN** the route interactions remain functional and no visual, lighting, shadow, collision, or Console-error regression is observed

### Requirement: Equivalent material-reference consolidation

The project SHALL consolidate art-scene material references only when the candidate materials are demonstrably equivalent in shader, texture assignments, scalar and color parameters, enabled keywords, render queue, surface and blend state, shadow behavior, and all other serialized rendering properties that affect output.

The project SHALL preserve separate material assets whenever equivalence cannot be demonstrated. The project SHALL reduce an existing renderer's redundant material slots only when every affected submesh retains the same visual material result.

#### Scenario: Consolidate equivalent material references
- **WHEN** repeated art-scene renderers reference material assets with fully equivalent rendering state
- **THEN** they reference one shared material asset without a visual presentation change

#### Scenario: Encounter similar but non-equivalent materials
- **WHEN** two material assets differ in any output-affecting serialized rendering property
- **THEN** their references remain separate and the difference is recorded as a later optimization candidate

#### Scenario: Remove a redundant material slot
- **WHEN** a renderer has multiple slots that resolve to the same fully equivalent material
- **THEN** the slot reduction preserves every affected submesh's visual result

### Requirement: Static model importer hygiene

The project SHALL disable animation import only for `Assets/SuperBreadMan/Scene Model/` FBX assets verified to be static scene-model content with no current or planned animation requirement.

The project SHALL preserve animation import for character, door, or any other FBX asset with an animation requirement or unresolved animation eligibility.

#### Scenario: Reimport a verified static scene-model FBX
- **WHEN** a verified static scene-model FBX is reimported with animation import disabled
- **THEN** its art-scene references resolve and the scene has no missing model, mesh, material, or shader binding

#### Scenario: Encounter an FBX with unclear animation use
- **WHEN** the animation requirement for an FBX cannot be verified
- **THEN** its animation-import setting remains unchanged and the asset is recorded for later review

### Requirement: Deferred structural rendering optimization

This change SHALL preserve existing mesh topology, material appearance and parameters, Shader Graph assets, prefab hierarchy, and scene object hierarchy. It SHALL NOT introduce GPU instancing, mesh combination, material atlasing, UV changes, cross-appearance material unification, or prefab restructuring.

#### Scenario: Review the rendering-optimization scope
- **WHEN** the completed change is reviewed
- **THEN** it contains only measured static-batching preparation and verified importer-hygiene changes, with structural rendering optimization deferred to a later change
