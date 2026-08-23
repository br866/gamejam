## Context

Formal-level Prefabs currently contain many MeshColliders, including purely decorative models. The existing A* GridGraph uses only `NavStatic` and `NavDynamic` colliders as obstacles. The audit found one direct rendered object, `SuccessorCheckpoint`, in `FormalLevel02`; all other formal-level rendered scene objects are already Prefab instances.

## Goals / Non-Goals

**Goals:**
- Make collision, navigation, and visual-only responsibilities visible through the existing Layer scheme and component presence.
- Replace simple blocking MeshColliders with tight BoxColliders calculated from source mesh local bounds.
- Remove collision entirely from small visual-only props.
- Validate the policy on L01 before applying it to all formal-level assets.

**Non-Goals:**
- Redesign player or enemy collision code, A* graph parameters, camera collision code, or the existing Layer names.
- Approximate irregular blocking shapes with one large BoxCollider.
- Modify reference scenes outside `Assets/MoMing/FormalLevels/`.

## Decisions

### Use existing navigation Layers rather than a new NoCollision Layer

`NavStatic` and `NavDynamic` already drive A* obstacle detection. Visual-only objects will be assigned `Default` and have no Collider, which guarantees no physics or navigation participation. A new `NoCollision` Layer would not prevent physics queries while a Collider remains attached and would consume a limited Layer slot.

### Classify before converting

The conversion audit will classify each MeshCollider as `remove`, `single-box`, or `manual-review`. Simple fixtures such as walls, cabinets, lockers, desks, beds, and rectangular doors qualify for a single BoxCollider. Small decoration qualifies for removal. Concave or thin irregular objects require compound boxes or a deliberate decision to remain non-blocking.

`broken_wall` is an approved exception: it is a fixed `NavStatic` obstacle that retains its existing MeshCollider because its irregular opening must preserve the intended traversal shape.

Door frames and jambs have no Collider. Door leaves use `NavDynamic` BoxColliders so opening and closing the leaf controls both player blocking and A* obstacle participation.

### Preserve collider settings when conversion is appropriate

For a converted collider, the BoxCollider inherits enabled state, trigger state, physics material, and contact offset. Its center and size are calculated from the source shared mesh local bounds, preserving the model's local transform instead of using world bounds.

### Pilot L01 before project-wide conversion

L01 contains the largest concentration of formal-level MeshColliders. Its audit, conversion, and runtime checks establish the classification rules before applying them to L02-L05 and shared art.

### Prefabize the direct rendered checkpoint

`SuccessorCheckpoint` is the only identified direct rendered formal-scene object. It will be extracted to a Prefab and replaced in `FormalLevel02`, preserving its serialized behavior and references.

### Separate mechanism triggers from physical blockers

Mechanism devices such as pedals, pressure plates, buttons, checkpoints, and exit volumes SHALL be Prefab-owned and SHALL keep their actor-detection Collider separate from any optional physical blocker. The actor-detection Collider is a Trigger and owns the trigger behavior; a mechanism visual model SHALL not receive an implicit solid Collider merely because it is rendered. Objects whose gameplay role is to block traversal, such as crates, door leaves, walls, and movable platforms, retain a separate non-trigger Collider and are not converted to trigger-only devices.

The formal trigger path SHALL prefer `FormalTriggerEligibility` together with `FormalActuatorTrigger` or `FormalOccupancyTrigger` for new mechanism instances. Existing specialized components remain valid where their serialized behavior is already established, but their trigger Collider must still be explicit and must not be supplied by the visual model.

### Deduplicate strictly equivalent Prefab assets through instance replacement

Prefab deduplication SHALL be based on a structural fingerprint, not on names, numeric suffixes, shared mesh names, or asset filename prefixes. The fingerprint includes the complete root and child hierarchy, relative transforms, active state, Layer, Tag, component types, mesh references, Collider settings, and serialized script identity/configuration. Material references are deliberately excluded from identity: different art materials do not prevent Prefab deduplication.

For an eligible duplicate group, one canonical Prefab SHALL be selected and every formal scene or content-Prefab instance referencing another member SHALL be replaced with a new instance of the canonical Prefab. Replacement SHALL preserve the old instance's parent, local Transform, name, active state, Layer, Tag, and supported serialized overrides. Renderer material slots from the old instance SHALL be captured before replacement and reapplied as instance-level material overrides on the canonical Prefab instance. Objects with runtime references, non-identical scripts, or non-identical Collider configuration SHALL remain separate and be recorded for manual review.

## Risks / Trade-offs

- [A box extends beyond an irregular visual mesh] -> Classify irregular blocking objects as manual review and verify traversable gaps in Scene view and play mode.
- [Removing a decorative collider removes an unintended interaction] -> Audit trigger and runtime references before removal; currently formal-level MeshColliders are non-trigger and non-convex.
- [Layer changes alter enemy paths] -> Re-scan and test formal navigation after each level group; navigation already filters to `NavStatic` and `NavDynamic`.
- [Prefab extraction loses a serialized reference] -> Create the Prefab from the scene object through Unity's Prefab API and verify all scene references after replacement.

## Migration Plan

1. Produce a per-Prefab collider audit and approve classifications for L01.
2. Convert approved L01 assets, retaining a list of manual-review assets.
3. Verify L01 player movement, camera collision, pushable-object behavior, and A* pathing.
4. Apply the approved rules to remaining formal-level Prefabs and shared art in small batches.
5. Extract `SuccessorCheckpoint` into a Prefab and audit all formal scenes for direct rendered objects.
6. Verify no automatic conversion left an inappropriate MeshCollider or a missing required blocker.

Rollback consists of reverting the affected Prefab or scene asset batch; no runtime data migration is involved.
