## Context

See `proposal.md` and `specs/formal-level02-mechanics/spec.md`. `FormalLevel02` already provides a scene-owned `Level02GameplayRoot`, player spawn points, and a successor checkpoint. The accepted `L02_Content` Prefab retains prototype monster, gate, footprint marker, and visual objects, but their current scene behavior and boundary assignments have not been validated for the formal route.

## Goals / Non-Goals

**Goals:**
- Turn the approved Level 2 visual route into a playable, resettable progression route.
- Reuse existing character, monster, pressure plate, gate, footprint, checkpoint, and navigation systems where they satisfy the required behavior.
- Keep level-specific runtime configuration owned by `FormalLevel02` and preserve the reference scene unchanged.
- Verify both character paths and monster safety boundaries in Unity play mode.

**Non-Goals:**
- Rebuild the accepted Level 2 art layout or migrate additional art candidates.
- Redesign character controls, global player management, generic gate behavior, or the navigation framework.
- Implement later-level mechanics or rely on the prototype source scene at runtime.

## Decisions

### Configure mechanics in the formal scene

Level-specific references, trigger volumes, navigation bounds, checkpoint, and successor handoff belong in `FormalLevel02` below `Level02GameplayRoot`. Existing retained prototype visuals may remain in `L02_Content`, but runtime components are moved, configured, or replaced in the formal scene when necessary.

Alternative considered: treat the copied art Prefab as the runtime owner. Rejected because the Prefab is flattened from a mixed source scene and should remain reusable visual content rather than own Level 2 progression references.

### Reuse existing character-aware components with narrow extensions

`FootprintMarker` already reacts to the active dog state, `PressurePlate` counts trigger occupants, `GateController` supports resettable opening, `MonsterPatrol` supports bounded pursuit, and `Checkpoint`/`SuccessorCheckpoint` provide reset and scene handoff integration. Extend these components only where a Level 2 behavior cannot be configured, such as restricting a plate to a specific player role or enforcing a safe-space boundary independently of visual geometry.

Alternative considered: create a separate Level 2 monolithic controller. Rejected because it would duplicate established reset, player, gate, and monster behavior and make level-specific configuration opaque.

### Verify physical and navigation boundaries together

The monster region, player route, gate collision, first-plate trigger, cooperative plate trigger, checkpoint, and exit trigger must be validated as one scene configuration. The implementation must make the safe space both physically inaccessible to the monster and excluded from its chase region.

Alternative considered: rely only on the monster's room bounds. Rejected because chase clamping alone does not prove that the physical route or navigation graph prevents entry into the safe space.

### Treat prototype preservation as an explicit migration decision

For each retained prototype component, implementation must either configure it for the formal route, relocate equivalent runtime behavior to `Level02GameplayRoot`, or remove it after replacement. The resulting formal scene must not rely on source-scene references.

Alternative considered: leave existing component state untouched. Rejected because existing prototype references and coordinates may not describe the approved formal route.

## Risks / Trade-offs

- [Flattened art Prefab contains runtime components] -> Audit every retained non-visual component and relocate or replace it with formal-scene-owned behavior before acceptance.
- [Monster bounds and navigation disagree] -> Test patrol, chase, and safe-space entry in play mode; adjust both the configured bounds and blocker geometry together.
- [Pressure plates count unintended colliders] -> Limit accepted occupants to the required character roles and test enter/exit/reset transitions.
- [Checkpoint or exit advances too early] -> Gate checkpoint and successor trigger activation behind cooperative route completion and test reset from each progression state.
- [Level 1 overlap walls obscure route ownership] -> Preserve user-approved visuals but validate that Level 2 collision and navigation only use the intended Level 2 route boundary.

## Migration Plan

1. Inspect retained prototype objects and current references in `FormalLevel02`.
2. Configure scene-owned Level 2 gameplay objects, triggers, collision, and navigation boundaries.
3. Connect dog-only clue and first plate, then cooperative second plate and route gate.
4. Configure checkpoint and successor exit after route completion.
5. Run Unity play-mode checks for both characters, monster behavior, reset, checkpoint, and handoff.
6. Keep the source scene unloaded and unchanged; roll back by removing only formal Level 2 runtime configuration if a mechanism cannot be validated.
