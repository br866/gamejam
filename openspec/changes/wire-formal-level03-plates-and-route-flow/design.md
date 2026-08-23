## Context

Formal Level 3 (`Assets/MoMing/FormalLevels/FormalLevel03.unity`) currently contains five triggers under `L03GameplayRoot`: one both-player coop plate and four dog-only plates chained by prerequisites, where only the last opens an exit door and preloads Level 4. Four interior doors (`door5 (2)`..`door5 (5)` with `FormalDoor`) exist in-scene but reference nothing. The shared Level 3 / Level 4 door (`ToLevel04_door4 (3)` in `FormalSharedArt_L03_L04`) is opened exclusively by `FormalGameFlowController.FindTransitionDoor` (name match on "ToLevel"), invoked from the Level 1 key pickup and checkpoint handoff paths.

Established conventions observed in Levels 1 and 2:

- Plates reference doors only within the same scene asset (direct serialized references; cross-scene references do not survive save/load).
- Shared boundary doors are opened only through the route flow controller by name lookup.
- Forward progression (`LoadSuccessor`) keeps every prior level loaded (`keepPriorLevels=true`); only unused shared-art scenes are pruned, which is what strands a lingering actor when its floor unloads.

`FormalActuatorTrigger.CompleteTrigger` also contains a duplicated successor-scene loading block (two identical `if (!string.IsNullOrEmpty(successorScene))` sections).

## Goals / Non-Goals

**Goals:**

- Let a completing plate open interior doors directly and the shared boundary door indirectly, reusing the single established shared-door mechanism.
- Add prerequisite require-any mode without changing any existing trigger's behavior.
- Retire stale levels during forward progression: close the two most recent levels' doors, unload everything older than the direct predecessor.
- Fix the duplicated successor loading block.

**Non-Goals:**

- No change to checkpoint auto-open behavior, `FormalPrerequisiteActuator`, the Level 2 dog plate's null actuator entry, or any other level's wiring.
- No new objects added to scenes by this change; all scene edits are manual Inspector configuration performed by the level owner after compile.
- No redesign of reset/restart semantics.

## Decisions

### D1: Remove the prerequisite machinery entirely

`FormalActuatorTrigger` keeps only `requirement`, `actuators`, `permanent`, `opensTransitionDoor`, and `successorScene`. The prerequisite list, prerequisite mode enum, and completion-state reporting were removed per owner decision; plates are ungated and complete immediately on eligible occupancy. Orphan `FormalMechanismState` components co-located with triggers were stripped from the level scenes. Existing chains in Levels 3–5 lose their gating by design and are reconfigured through the remaining fields.

*Alternative considered:* keeping require-all/require-any modes - rejected after the owner simplified the plate contract to "step to open".

### D2: Shared-door control via an `opensTransitionDoor` checkbox

Add `[SerializeField] bool opensTransitionDoor`. On completion, the trigger asks the flow controller to open the transition door from the trigger's own scene toward the route successor. A small public helper on `FormalGameFlowController` (e.g. `OpenTransitionDoorToSuccessor(string fromScene)`) resolves the successor from the existing route catalog and calls the existing `FindTransitionDoor` + `OpenPermanently` path - the exact path already used by the key and checkpoints.

*Alternatives considered:*
- Free-text door-name field on the trigger with its own scene scan - rejected: duplicates `FindTransitionDoor` matching rules and forces hand-typed strings into the Inspector.
- Relay component implementing `IFormalLevelActuator` placed into the actuators array - rejected: requires creating a new scene object and indirection for something a checkbox expresses.

Preloading the successor stays available through the existing `successorScene` field (the Level 2 cooperative plate pattern).

### D3: Arrival cleanup inside `LoadLevelRoutine`

For forward transitions (target differs from predecessor), after the target scene activates:

1. Determine the new level's route index `i`.
2. For each loaded level scene among `route[i-1]` and `route[i-2]`, call `Close()` on every `FormalDoor` found in that scene.
3. Unload every loaded route level scene except the new current level and `route[i-1]`.

The existing `UnloadUnusedSharedArt` pass continues to prune shared art against `{current, pendingUnload}`, satisfying shared-art retention without new logic. Restart/reset paths (`RestartCurrentLevelRoutine`, `ResetCurrentLevel`) are untouched. The `keepPriorLevels` parameter stays in the signature for compatibility but forward flow no longer relies on it to preserve anything beyond the direct predecessor.

### D4: Remove the duplicated successor block

Delete the second identical `successorScene` section in `CompleteTrigger`; one load call remains.

## Risks / Trade-offs

- [Closing the predecessor's doors may confine an actor still wandering inside it] -> Explicitly requested behavior (anti-backtracking); the predecessor itself stays loaded so nothing falls. If a specific door must stay open later, it can be excluded by convention without code changes.
- [Transition-door open fires while the shared scene is not yet loaded] -> `FindTransitionDoor` returns null and the flow controller logs its existing warning; checkpoint auto-open remains as a functional fallback.
- [Multiple "ToLevel" doors in one shared pair could match ambiguously] -> Each shared pair currently contains exactly one such door; the first match wins, matching today's behavior for key/checkpoint paths.
- [EditMode traversal validation may encode current wiring assumptions] -> Run the suite after compile; adjust expectations only where they assert the old accumulate-forever lifecycle.

## Migration Plan

All new serialized fields carry safe defaults, so existing scenes load unchanged before any wiring. Rollback is a plain revert: defaults mean no scene reserialization is required by the code change alone.

## Open Questions

None.
