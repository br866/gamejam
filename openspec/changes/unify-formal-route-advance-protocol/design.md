# Design: unify-formal-route-advance-protocol

## Context

`FormalGameFlowController` (hosted only in `FormalPersistent.unity`, DontDestroyOnLoad) additively loads the six route levels plus shared-art scenes. Today "advance to the next level" exists as 7 ad-hoc call chains (see proposal). Guard flag `operationInProgress` silently discards requests at four different shapes of call sites; the shared transition door is found by first door whose name contains `"ToLevel"` inside the shared-art intersection; the L04→L045 edge's special treatment (`== "FormalLevel045"` string checks in four methods) and the chase sequence are entangled with a debug-only assembly path (Alpha2). The route catalog is serialized in `FormalPersistent.unity` (~13 serialized lines), with an in-code `EnsureRouteCatalog()` fallback that builds defaults when empty.

Constraints carried over from exploration:
- `FormalDoor.cs` must not change (parallel change `complete-formal-route-playable-flow` task 3.3 owns it).
- Inspector wiring done by `wire-formal-level03-plates-and-route-flow` task 4.x must not need rework.
- Keypad5 reset semantics stay byte-for-byte.

## Goals / Non-Goals

**Goals:**
- One advance entry point; one source of truth for "the next level" (route catalog).
- No silent request loss across load windows.
- Deterministic transition-door resolution independent of naming accidents.
- L045 arrival sequence reachable through normal play (checkpoint activation).
- GM keys behave consistently enough that 2/8/6 alone can exercise every route edge.

**Non-Goals:**
- Data-driven declarative policy tables (方案乙) — predicates suffice for one special edge.
- Door open/close animation/collider timing fixes (explicitly rejected by owner: doors only open in practice).
- Touching legacy MoMing Puzzle system, `FormalDoor.cs`, or checkpoint respawn anchoring behavior.
- Unload-on-physical-crossing detection for Level05 arrival (accepted nuance: cleanup runs when the crate door opens and L05 preloads).

## Decisions

### One parameterless entry point

`RequestRouteAdvance()` computes the successor as `routeCatalog[index(currentLevelScene) + 1]`. Callers pass nothing because the controller already knows the active scene. Rejected alternatives: keeping per-source serialized successors (creates the two-sources-of-truth bug class this change kills); passing `fromScene` explicitly (redundant; the drain-time staleness check needs origin anyway, tracked internally).

Key, actuator trigger, and crate trigger each collapse to: local bookkeeping (collect/hide crate door) + one call. `FormalHumanKey`'s hardcoded `"FormalLevel01"` and `successorScene` field become inert; `FormalActuatorTrigger`'s `opensTransitionDoor`/`successorScene` fields remain serialized (protecting existing Inspector wiring) but semantically merge into "any ticked → request advance".

### Single-slot pending with origin guard

A pending advance is stored as `{fromScene}` (one slot, not a queue): while `operationInProgress`, requests overwrite the slot; every routine tail drains it once — executing only if `fromScene == currentLevelScene` still holds at drain time (the intended edge may have been satisfied by the very load that was running). Rejected alternatives: retry-at-source (three scripts each growing polling loops); a general queue (no gameplay path can produce two distinct concurrent edges; YAGNI).

### Registered door-name token per edge

`FormalRouteEntry` gains `arrivalTransitionDoorName` ("the door that leads into this level"). Lookup intersects both levels' shared-art scenes (unchanged) and matches doors whose name contains the registered token (e.g., `ToLevel02`). Defaults ship in `EnsureRouteCatalog()` so an empty catalog still works; the five entries in `FormalPersistent.unity` get backfilled. `CloseLevel045To05Doors`' private substring scan dies with its caller. Rejected alternative: direct cross-scene object references (breaks down with additive load order and duplicates scene-ownership problems).

### Edge policies as centralized predicates

Two predicates replace four scattered string comparisons: retention (`ShouldRetainPredecessor`: true only for entering L045) and arrival sequence (`HasArrivalSequence`: true only for L045). All existing behaviors keyed off these checks — ArrivalCleanup's skip, ResetCurrentLevel's retained-monster branch, pursuit-sequence gating — read them instead. Behavior is byte-for-byte preserved except where specs intentionally change it (chase trigger moves from crate-door band-aid to checkpoint activation).

### Chase sequence triggered by checkpoint activation

`FormalCheckpoint.ActivateCheckpoint()` additionally notifies the flow controller (every checkpoint notifies; the controller filters by `HasArrivalSequence(activeScene)`). This requires zero new Inspector wiring and makes the sequence reachable through normal play — fixing the confirmed oversight. The crate-door callback `NotifyLevel045DoorOpened` is deleted; reset keeps restarting the sequence after restoring hostile patrol.

### GM parity and Alpha2 demolition

- Keypad2 → `RequestRouteAdvance()` (now also opens the transition door, matching play).
- Keypad6 → force-complete current-level conditions (unchanged) → `RequestRouteAdvance()`.
- Keypad8 → previous-level fast travel, now also placing players at the target spawn (spawn placement condition changes from "only initial load" to "every destructive jump").
- Keypad5 → untouched.
- Deleted outright: Alpha2 binding, `StartGmLevel045`, `LoadGmLevel045Routine`, `gmLevel045Pending`, `CloseLevel045To05Doors`, `FindCheckpoint`, `OpenAllDoorsInScene(string)`. Their only irreplaceable duty (assembling L045 with chase) transfers to normal advance + checkpoint policy.

### Execution ordering inside the advance routine

Shared art for {current, successor} is ensured loaded **before** door resolution (removes the "door lookup fails because shared scene absent" failure mode), then door opens, then successor loads (additive, predecessor retained), then arrival cleanup per policies, then pending-slot drain. The drain runs last so chained advances observe settled state.

## Risks / Trade-offs

- [Monsters vanish at crate-door-open moment, not physical L05 crossing] Accepted by owner; documented in spec scenario wording ("via a normal advance").
- [Losing Alpha2 lengthens dev path to L045] Mitigated: Keypad6 chain reaches any level quickly; Keypad2 now behaves like real play, making it a better test vehicle than Alpha2 was.
- [Catalog serialization drift when adding levels] Mitigated: `EnsureRouteCatalog()` carries default tokens; validation test asserts every non-final edge resolves a door.
- [Pending slot hides bugs by deferring instead of failing loud] Mitigation: drain executes exactly once with a log line when a deferral actually happened; EditMode test pins the contract.
- [Refactor regressing subtle unload timing] Mitigation: capture baseline EditMode results first; new focused tests lock keep/unload sets per edge before rewriting internals.

## Migration Plan

Single commit-series inside this change, no data migration beyond the `FormalPersistent.unity` catalog backfill. Rollback = revert the commits; no persisted state depends on removed APIs. Order: baseline tests → controller internals behind new entry → call-site slimming → deletions → scene backfill → full regression.

## Open Questions

None — all previously open points were resolved with the owner (方案甲; GM parity including Alpha2 removal; Keypad8 spawn placement; chase trigger = L045 checkpoint; monster-unload timing accepted).
