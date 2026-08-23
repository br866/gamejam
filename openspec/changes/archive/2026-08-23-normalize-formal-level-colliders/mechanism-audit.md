# Formal Mechanism Collider Audit

## Scope

Audited formal-level Prefabs and content Prefabs for mechanism-like names and runtime trigger components across L01-L05. The audit distinguishes detection volumes from objects that intentionally block traversal.

## Resolved

| Object | Finding | Decision |
| --- | --- | --- |
| `L01_Mechanism_Pedal` | Formal content used an old direct model instance with a solid `NavStatic` BoxCollider. The reusable mechanism Prefab already had a trigger Collider and `FormalMechanismPedal`. | Replaced the L01 content object with the mechanism Prefab. The detection Collider is `BoxCollider`, `isTrigger=true`, on `Default`; the permanent door behavior remains linked. |

## Manual Review

| Object | Finding | Decision |
| --- | --- | --- |
| L02 `Pedal1` / `Pedal2` and their `Pedal_Car` models | These objects have non-trigger BoxColliders on `Default`, but no formal mechanism trigger component was found. Their names describe visual or vehicle assets rather than a confirmed pressure mechanism. | Do not convert by name. Confirm gameplay role and ownership before changing collision or creating a mechanism Prefab. |
| L01 `EmergencyButton` models | These objects have non-trigger `NavStatic` BoxColliders and no formal trigger component in the scanned Prefabs. | Treat as art/physical-prop candidates, not trigger volumes, until their gameplay role is confirmed. |

## Existing Trigger-Backed Mechanisms

- `L01_Checkpoint` uses a trigger Collider and `FormalCheckpoint`.
- `L01_Mechanism_Pedal` uses a trigger Collider and `FormalMechanismPedal` after this change.
- Formal `FormalActuatorTrigger` and `FormalOccupancyTrigger` are the preferred generic components for new mechanism instances.

## Policy

- Pedals, pressure plates, buttons, checkpoints, and exit volumes use explicit trigger Colliders for actor detection.
- Crates, door leaves, walls, and movable platforms retain separate non-trigger blocking Colliders when they physically obstruct traversal.
- A visual model name alone is not sufficient evidence to convert its Collider to a Trigger.

## Strict Prefab Deduplication

The first strict structural fingerprint audit found 19 exact duplicate groups (38 SharedModels Prefabs total). Twenty formal content-Prefab instances referenced duplicate members. Those instances were replaced with canonical Prefab instances while preserving parent, local Transform, name, active state, Layer, Tag, and sibling order.

The updated deduplication rule ignores material identity. When a structural duplicate has a different visual material, the replacement must preserve the old Renderer material slots as instance-level overrides rather than keeping a separate Prefab solely for art material variation.

The 19 duplicate source assets now have zero references from formal content Prefabs. They remain on disk for rollback and are not deleted in this pass. Deletion requires a separate cleanup pass after repository-wide asset-reference review.

After the external-reference review, 717 obsolete SharedModels Prefab assets and their 717 `.meta` files were deleted. Fifteen older SharedModels Prefabs were retained because they are still referenced by the standalone `FormalSharedArt_*.unity` scenes. The retained assets were not replaced or deleted in this cleanup.

The automatic replacement intentionally excludes near-matches and behavior-bearing objects whose scripts, serialized fields, hierarchy, or Collider data differ.
