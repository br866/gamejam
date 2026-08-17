## Context

See `proposal.md` and `specs/formal-level02-art-assembly/spec.md`. `superbreadman 1.unity` is a shared prototype/reference scene whose bulk selection contains content from multiple route stages. `FormalLevel01` already owns its migrated art through `L01_Content`, while `FormalLevel02` is the target for the next formal art pass.

## Goals / Non-Goals

**Goals:**
- Preserve the source scene and Level 1 formal assembly.
- Create a traceable Level 2 candidate set before arranging content.
- Establish a reusable Level 2 art assembly beneath the existing formal Level 2 scene.

**Non-Goals:**
- Implement the Level 2 monster, plates, footprints, safe space, checkpoint, collision, navigation, or runtime interactions.
- Delete or move objects in the source scene.
- Migrate content attributed to Level 1, Level 3 through Level 5, prototype systems, players, or unresolved objects.

## Decisions

### Classify before copying

The current 186-object Editor selection is evidence of a working set, not a migration instruction. Each object is classified against the Level 1 manifest and its source hierarchy before it is copied to Level 2.

Alternative considered: duplicate the entire selection and remove known Level 1 objects later. Rejected because it risks introducing unrecognized duplicates and later-level content into the Level 2 scene.

### Keep formal Level 2 content scene-owned

Accepted art is grouped beneath a Level 2 content prefab or root instantiated by `FormalLevel02`. The source scene remains unchanged and no formal level references its live scene objects.

Alternative considered: use source-scene objects directly through cross-scene references. Rejected because it prevents independent additive level ownership and unloading.

### Defer gameplay treatment

Art assembly uses existing visual objects only. Collider, navigation, mechanism, monster, player, trigger, and checkpoint decisions remain subsequent Level 2 work after the large-scale layout is reviewed.

Alternative considered: configure gameplay while objects are selected. Rejected because the current priority is a complete large-scale art pass and interaction roles have not yet been validated.

## Risks / Trade-offs

- [A Level 1 object is misclassified as Level 2] -> Compare candidate names and source paths against `Level01SourceManifest.md` and the `L01_Content` hierarchy before copying.
- [A later-level object is pulled into Level 2] -> Exclude objects whose Level 2 ownership cannot be established and record them as unresolved.
- [The assembled layout lacks gameplay clearance] -> Review the large-scale Level 2 environment before authoring collision or interactions.

## Migration Plan

1. Read the current Editor selection and source hierarchy.
2. Exclude all confirmed Level 1 duplicates and non-Level-2 runtime/prototype content from the candidate set.
3. Copy only confirmed Level 2 visual art into a Level 2-owned formal content assembly.
4. Place the assembly in `FormalLevel02` without editing the source scene.
5. Save and inspect the formal scene, then record deferred gameplay work.
