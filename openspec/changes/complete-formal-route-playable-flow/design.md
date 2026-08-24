## Context

See `proposal.md` and its route-release and gameplay-coverage specifications. The repository currently ships a legacy `Start -> Level1 -> Level2 -> End` path while the intended formal route exists as additive scenes driven from `FormalPersistent`. Formal route scenes have uneven identity, checkpoint, exit, and gameplay wiring; the runtime currently unloads prior levels immediately despite the approved checkpoint-commit lifecycle.

## Goals / Non-Goals

**Goals:**

- Establish one player-facing execution path with explicit scene ownership from start through final completion.
- Complete shared gameplay infrastructure only where at least two levels need the behavior, then wire each level as configuration rather than extending a generic graph framework.
- Make route acceptance observable through focused editor tests and a build-first-scene Play Mode checklist.

**Non-Goals:**

- Merge the legacy prototype and formal runtime stacks.
- Redesign approved art placement, replace art assets, or bulk-rewrite existing scene hierarchies.
- Block first-route completion on anxiety, final audio, full HUD/pause polish, or optional hint cinematics.

## Decisions

### Use the formal persistent scene as the sole formal runtime owner

The player-facing startup scene will enter `FormalPersistent`; that scene owns the actor pair, camera, global flow, and final completion coordination. Formal levels remain additive and own only level-local state and references.

Keeping legacy `Start` as the formal runtime owner was rejected because it carries prototype scene-loading assumptions and would create two global flows. Replacing it entirely was rejected because the legacy path remains reference material; the startup bridge can preserve it without migrating its internals.

### Restore checkpoint-committed transitions

On an exit, the successor and its declared shared scenes load additively while the predecessor remains retained. The successor checkpoint commits the transition, then prior level objects and unreferenced shared art unload. Direct jump and development navigation may use an explicit destructive fast-travel mode, but must not redefine gameplay transition semantics.

Immediate predecessor unload was rejected because deaths before the successor checkpoint cannot be reconciled with the approved route lifecycle and produces a different route than the documented game.

### Treat the route catalog and scene contract as validated data

One catalog remains authoritative for route ordering, playable scene names, and shared-art membership. Each formal scene receives a matching unique level identity, paired entrance anchors, content root, collision root, checkpoint, configured exit where applicable, and no persistent-player duplicate. Validation will inspect all six entries, including build settings and Level 5 final completion rather than relying on object names alone.

Scene scanning to infer adjacency was rejected because additive shared-art ownership is explicit and scene names are not durable gameplay identifiers.

### Build shared mechanics as small stateful components

Role eligibility, unique occupancy, prerequisite state, ordered completion, reset policy, doors/exits, and hostile-region safety are composed at scene level. Level-specific objects own their references and completion ordering. The controlled escape is a scoped control-mode override that activates at the Level 5 corridor boundary and releases at the final room boundary.

A visual general-purpose puzzle graph was rejected because no approved level requires arbitrary graph editing and it would obscure the level-specific routes being validated.

### Finish levels in dependency order

Finish lifecycle/validation and Level 1 compatibility first, then reusable cooperative and enemy foundations with Level 2 as the first consumer. Implement Levels 3 and 4 from recorded business routes, add the minimal Level 4.5 checkpoint/exit bridge, then finish Level 5's escape, final room, and ending. Every level is playtested through its real predecessor handoff before the next integration boundary is accepted.

Parallel scene-only completion was rejected because exits, checkpoints, shared art, actors, and monsters interact across boundaries and failures are otherwise discovered too late.

### Make final completion a route state, not an invalid successor

Level 5's final door resolves into a dedicated completion presentation owned by persistent flow. It exposes a deterministic restart or return-to-start action and never attempts to find a nonexistent Level 6.

Using the legacy `End` directly was rejected because it couples formal completion to legacy flow ownership; a small formal completion surface can coexist with it while preserving a single formal reset path.

## Risks / Trade-offs

- [Existing scene object references are incomplete or stale] -> Validate all serialized references before Play Mode wiring and repair one level at a time.
- [Retaining predecessor scenes duplicates overlapping visual/collision content] -> Retain only declared predecessor and successor scenes, track shared-art references explicitly, and validate transition overlap in Play Mode.
- [A reset closes a route behind a checkpoint] -> Separate permanent and resettable mechanism registration and assert state after each reset stage.
- [Monster boundaries disagree with navigation] -> Validate hostile collision, navigation destinations, and capture checks as one configuration.
- [Level 5 control override leaves controls disabled] -> Use explicit enter/exit/reset states and test each exit path, including player capture and final-room transition.
- [Route work expands into polish] -> Hold presentation work outside the route acceptance checklist until the full traversal succeeds.

## Migration Plan

1. Add the formal startup bridge and correct transition lifecycle behind validated route-catalog configuration.
2. Normalize all scene contracts and add validation before attaching additional gameplay components.
3. Complete shared trigger, progression, reset, door/exit, and enemy-boundary behavior with focused tests.
4. Wire and verify Level 1 and Level 2, then record and implement Level 3, Level 4, Level 4.5, and Level 5 in route order.
5. Add final completion, run all editor checks, and run a clean build-first-scene playthrough without debug navigation.

Rollback is scoped by feature: the startup bridge can return to the legacy route, and each level's scene-local wiring can be reverted independently. Persistent runtime and shared-art ownership changes require their focused lifecycle tests to remain passing before adoption.
