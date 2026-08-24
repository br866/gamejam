## Context

See proposal.md for motivation. The established physical-transition state in the flow controller separates successor preload from arrival confirmation. At present, the Level 4 exit actuator and two distinct Level 4.5 exit paths still invoke direct route advancement. The prior Level 3 repair showed that a Prefab override cannot be relied upon for nested exit content; scene-owned configuration is required. Level 4.5 deliberately retains Level 4 after arrival for pursuit.

## Goals / Non-Goals

**Goals:**

- Apply one route-level physical-exit policy to all configured progression sources in a scene.
- Reuse the generic flow-controller preload and arrival-confirmation state rather than introduce route-specific loading paths.
- Allow retained-predecessor arrival to finish as a successful arrival without closing or unloading the retained scene.
- Keep GM direct transitions outside the physical-exit policy.

**Non-Goals:**

- Change Level 4, Level 4.5, or Level 5 puzzle prerequisites, pursuit behavior, door art, layouts, or Prefab assets.
- Replace the existing shared-art door lookup or player-placement behavior for direct GM transitions.

## Decisions

### Register a scene-owned physical-exit policy by source scene and successor

The existing `FormalPhysicalDoorExitBinding` pattern will be expanded from actuator-only runtime mutation into a generic source-scene policy. Route-producing exit components in that scene will query the policy before requesting direct advancement; a matching policy instead asks the flow controller to preload the configured successor and open the transition door.

This covers both `FormalActuatorTrigger` and `FormalCrateDoorTrigger`, while preserving a single physical progression rule per source scene. The Level 4 and Level 4.5 scenes own their policy components, so no Prefab asset or fragile nested-Prefab override is modified.

Alternative: add separate preload flags to every exit component. Rejected because it duplicates scene policy and leaves future route-producing exits easy to miss.

### Add a Level 4.5 scene-owned two-player arrival seal

Level 4.5 needs its own arrival trigger, configured equivalently to the existing Level 4 and Level 5 entry seals. It commits only the matching pending physical transition after both actors arrive.

Alternative: immediately commit Level 4.5 when it is preloaded. Rejected because it violates physical traversal and changes the active level before the players cross the door.

### Treat retained predecessor as completed arrival cleanup

Arrival confirmation and predecessor cleanup will report separate outcomes. When Level 4.5 is current, the cleanup phase intentionally preserves Level 4 and the door state needed by pursuit, but the entry seal must still finish after successful arrival confirmation.

Alternative: leave the seal armed when predecessor retention blocks cleanup. Rejected because it retries forever and does not represent the completed arrival state.

### Let L05_Checkpoint commit recovery and release retained Level 4 only

L05_Checkpoint is reached after players have physically crossed the L4.5-to-L5 door, but before the two-player Level 5 arrival seal. It commits Level 5 as the recovery level without placement, then releases `retainedPhysicalPredecessorScene` (Level 4) directly, leaving `pendingUnloadScene` (Level 4.5) untouched. This makes a subsequent death recover to the L05 checkpoint, removes the L4 monsters when the pursuit segment is over, and preserves L4.5 for the rest of the route.

Alternative: use the Level 5 arrival seal to unload both prior levels. Rejected because L4.5 must remain loaded and checkpoint timing is the intended end of the retained-L4 pursuit segment.

### Re-establish Level 4 before Level 4.5 recovery when needed

While Level 4.5 is the active pursuit level, Level 4 is a runtime dependency rather than an optional predecessor. Recovery will verify the retained scene is loaded; if not, it reloads the retained Level 4 scene and then resumes the normal dog-following and delayed-monster sequence.

### Preserve direct GM entry points

GM commands continue to call their direct load path rather than the source-scene physical-exit policy. Direct loading cancels any pending physical transition as it does today.

Alternative: route GM commands through preload. Rejected because testers need deterministic immediate placement.

## Risks / Trade-offs

- [A new route-producing exit is not registered by the policy] → cover known actuator and crate-door paths in tests, and report an explicit diagnostic when a policy has no matching exits.
- [Both Level 4.5 exit mechanisms fire in the same frame] → flow-controller pending-transition guards make the first preload authoritative and ignore duplicates.
- [Retained L4 is accidentally closed or unloaded] → test the L4-to-L4.5 arrival specifically asserts L4 remains loaded.

## Migration Plan

1. Extend the scene-owned physical-exit policy and route exit integration points.
2. Configure Level 4 and Level 4.5 policy components, and add the Level 4.5 scene-owned arrival seal.
3. Add edit-mode coverage for both exit paths, retained arrival, L05 checkpoint recovery commit and retained-L4 cleanup, L4.5 recovery preservation, and GM interruption.
4. Verify the two routes in Play Mode, including physical crossing by both actors, L4-only cleanup and L5 recovery at L05_Checkpoint, and direct GM jumps.

Rollback consists of removing the two scene policy components and the Level 4.5 entry seal, then reverting the generic policy integration together; no Prefab migration is involved.
