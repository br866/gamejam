## Context

See `proposal.md` and `specs/formal-level-respawn-recovery/spec.md`. Existing formal work uses `HumanSpawn`/`DogSpawn` for entry and checkpoint-specific respawn anchors for recovery, but it does not establish one canonical anchor type, ground-resolved placement, or a common recovery contract for all formal route stages.

## Goals / Non-Goals

**Goals:**

- Route initial entry, anxiety failure, and monster capture through one active-level placement decision.
- Use only `HumanRespawnAnchor` and `DogRespawnAnchor` pairs for formal player placement, resolving their Y from valid ground below their XZ.
- Let each level declare which local objects and modes reset without leaking that responsibility into unrelated levels.
- Preserve additive predecessor retention until successor-checkpoint commitment during recovery.
- Make reset classification and route-wide verification explicit.

**Non-Goals:**

- Redesign player controls, monster AI, puzzle mechanics, scene geometry, collision, or navigation.
- Change permanent-progress rules beyond their established current-level lifetime or use anchor Y as authored gameplay data.
- Add new gameplay content or redefine the Level 5 checkpoint position.

## Decisions

### Select player placement from the active level controller

The formal route flow resolves entry and failure through the active level controller. It uses the level's initial `HumanRespawnAnchor`/`DogRespawnAnchor` pair until a checkpoint supplies the current respawn pair, then relocates both shared actors as a coordinated operation and restores active-level resettable state. `HumanSpawn` and `DogSpawn` fields and scene objects are removed from the formal placement contract.

This makes the selected destination owned by the currently playable level rather than by the player actor or a global scene search. It also keeps the same behavior available for either failure source.

Alternative considered: let anxiety and each monster independently reposition the character it caught. Rejected because it can split the pair, bypass level-local reset work, and make the next failure source behave differently.

### Resolve vertical placement from valid ground

Each selected respawn anchor supplies only an XZ target. The placement path casts downward from a safe height at that XZ, excludes triggers, and uses the first valid ground surface to calculate the actor's feet position. An anchor with no valid ground below is invalid configuration: placement stops with a diagnostic rather than using its authored Y.

This prevents misplaced anchors, floating level art, and trigger volumes from deciding the actor's vertical position while preserving independent human and dog XZ locations.

Alternative considered: use the transform's complete position, including Y. Rejected because anchor Y is inconsistent across existing scenes and can leave actors floating, buried, or standing on a trigger.

### Make reset state explicit and level-local

Each level controller registers or configures its resettable local objects and modes. Recovery asks the active level to restore those objects while leaving its marked permanent progress untouched. Level 5 uses the same boundary, with an escape-stage reset group for the cabinet, monsters, exit, and controlled mode.

Alternative considered: reload every loaded scene on failure. Rejected because it would discard permanent progress and violate the retained-predecessor handoff lifecycle.

### Preserve the active additive handoff during reset

Entering a successor makes it the active recovery owner immediately, even before its checkpoint is committed. Recovery therefore uses the successor initial respawn pair and retains its predecessor until the existing checkpoint handoff commits.

Alternative considered: recover to the predecessor checkpoint until the successor checkpoint activates. Rejected because it makes a pre-checkpoint failure silently move players across a completed level boundary and does not match level-local failure expectations.

### Restore state before returning control to players

Recovery suppresses overlapping reset requests, restores resettable state and actor movement/interaction readiness, repositions both actors, then resumes normal input and threat evaluation. This prevents a stale monster attack, moving Rigidbody, or trigger callback from immediately re-failing an actor at the destination.

Alternative considered: teleport first and let local objects reset asynchronously. Rejected because the destination may overlap stale object or monster state for at least one frame.

## Risks / Trade-offs

- [A respawn anchor has no ground beneath its XZ] → Add scene-level validation that reports the invalid anchor and refuse placement rather than using anchor Y.
- [A resettable object is not registered by a level] → Add scene-level validation that reports missing recovery configuration and validate every stage in Play Mode.
- [A permanent object is mistakenly reset] → Use explicit permanent/resettable classification and include mixed-progress acceptance checks.
- [Two failure sources request recovery in the same frame] → Guard recovery as an atomic in-progress operation and ignore duplicate requests until completion.
- [Level 5 escape reset also clears final-room progress] → Scope escape reset registration only to corridor state and test a failure after final-room progress exists.
- [A retained predecessor holds stale references during recovery] → Keep recovery ownership with the successor and retain existing handoff cleanup only for checkpoint commitment.

## Migration Plan

1. Inventory and replace formal `HumanSpawn`/`DogSpawn` placement references with initial `HumanRespawnAnchor`/`DogRespawnAnchor` pairs.
2. Introduce ground-resolved XZ-only player placement and the shared active-level recovery entry point without changing route ordering.
3. Configure and validate Level 1 through Level 4.5, then configure the Level 5 escape reset group.
4. Run focused Play Mode recovery checks per level and the cross-level pre-commit handoff check.
5. Roll back by reverting the shared recovery wiring and affected level configuration together; no saved-data migration is required.
