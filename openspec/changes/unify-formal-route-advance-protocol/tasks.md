# Tasks: unify-formal-route-advance-protocol

## 1. Baseline Capture

- [x] 1.1 Run the existing EditMode suites and record current results (including pre-existing Level02 scene failures) as the regression baseline

> Baseline record (2026-08-24): 24 tests, 8 pre-existing failures, all in `FormalTraversalValidationTests`: EntranceAndCheckpointAnchorsAreSupported(L02 chairs overlap), EntranceToCheckpointSegmentIsClear(L01 no human checkpoint anchor), EveryFormalRouteSceneHasTheRequiredBaseContract(L02 no collision root), FormalMechanismPedalPrefabHasNoSolidChildCollider(missing prefab), L01MechanismPedalUsesTriggerPrefabAndDoesNotBlock(missing prefab), Level02DogPlateAndSafeZoneGateTheLevel03Exit(null), Level02MonsterPatrolsSceneWaypoints(expected 2 waypoints, found 4), MonsterSafeZoneSuppressesCapture(AmbiguousMatchException).

- [x] 1.2 Add focused EditMode tests that lock CURRENT behavior before refactor: per-edge keep/unload scene sets after advance, ArrivalCleanup door-closing scope, restart routine closing the transition door

> Implemented as `Assets/Editor/FormalRouteAdvanceProtocolTests.cs` (catalog shape, shared-art-intersection door resolution, degrade-to-warning, restart closes registered transition door). Async unload/keep-set coverage is deferred to Play Mode acceptance (task 6.2) because EditMode cannot drive SceneManager async flows deterministically.

## 2. Flow Controller Core (`FormalGameFlowController.cs`)

- [x] 2.1 Add `arrivalTransitionDoorName` to `FormalRouteEntry` with default tokens in `EnsureRouteCatalog()` (L02→`ToLevel02` … L05→`ToLevel05`, empty for L01)
- [x] 2.2 Rewrite `FindTransitionDoor` to match the registered token within the shared-art intersection; warn (no exception) when unresolvable
- [x] 2.3 Add edge-policy predicates `ShouldRetainPredecessor(edge)` and `HasArrivalSequence(level)`; route `ArrivalCleanup`, `ResetCurrentLevel` L045 branch, and pursuit gating through them, preserving current outcomes
- [x] 2.4 Implement `RequestRouteAdvance()`: catalog-derived successor, shared-art ensure → registered door open → additive successor load → arrival cleanup → drain; single-slot pending `{fromScene}` recorded when busy, drained once at every routine tail, discarded when origin no longer active (with a log line on actual deferral)
- [x] 2.5 Re-point `NotifySuccessorCheckpointActivated` internals onto the same advance path (busy case records pending instead of early-return drop); keep public signature
- [x] 2.6 Add `NotifyCheckpointActivated(scene)` hook filtered by `HasArrivalSequence`; make it start the L045 pursuit sequence
- [x] 2.7 Re-wire GM keys: Keypad2 → `RequestRouteAdvance()`; Keypad6 completes conditions then calls it; Keypad8 additionally places players at target-level spawn; remove Alpha2 binding plus `StartGmLevel045`, `LoadGmLevel045Routine`, `gmLevel045Pending`, `CloseLevel045To05Doors`, `FindCheckpoint`, `OpenAllDoorsInScene(string)`, `NotifyLevel045DoorOpened`
- [x] 2.8 Compile via Unity refresh and confirm zero console errors

## 3. Gameplay Call Sites

- [x] 3.1 `FormalHumanKey`: replace hardcoded edge + `LoadSuccessor` pair with single `RequestRouteAdvance()`; stale serialized field removed and legacy prefab test updated to the new contract
- [x] 3.2 `FormalActuatorTrigger`: in `CompleteTrigger`, treat any of (`opensTransitionDoor`, non-empty `successorScene`) as one `RequestRouteAdvance()` call; keep fields serialized for existing wiring
- [x] 3.3 `FormalCrateDoorTrigger`: keep local crate-door opening; replace hardcoded L045→L05 flow calls and notify with single `RequestRouteAdvance()`; trim redundant debug logs while keeping error paths
- [x] 3.4 `FormalCheckpoint`: after successful activation, call the new checkpoint-notification hook (one line)
- [x] 3.5 Compile and confirm zero console errors

## 4. Scene Data Backfill

- [x] 4.1 Backfill `arrivalTransitionDoorName` tokens into the five successor entries of the route catalog serialized in `FormalPersistent.unity`
- [x] 4.2 Verify in editor the controller's serialized catalog matches code defaults (no duplicate/divergent entries)

> Editor verification: deserialized catalog reads Level01→``, Level02→`ToLevel02`, Level03→`ToLevel03`, Level04→`ToLevel04`, Level04.5→`ToLevel045`, Level05→`ToLevel05`.

## 5. New Behavior Tests (EditMode)

- [x] 5.1 Test single-entry semantics: each source type produces exactly one advance; repeated fires are idempotent; a disagreeing serialized successor hint is ignored

> Covered at controller level (`RequestRouteAdvanceDefersWhenBusyAndDrainsExactlyOnce`: collapse + drain-once; stale-hint case removed structurally by deleting `FormalHumanKey.successorScene`). Physical-trigger variants are exercised in task 6.2 play pass.

- [x] 5.2 Test busy-window retention: forced in-progress operation + request → exactly one deferred execution; repeated requests collapse; stale-origin request is discarded
- [x] 5.3 Test registered door lookup: correct door per edge resolves; unknown token warns without state change or exception

> Includes `RegisteredDoorTokenOverridesGenericSubstringOrder` proving the registry beats substring order against real shared-art content.

- [x] 5.4 Test arrival sequence: activating the L045 checkpoint starts human-only + orbit binding and delayed forced chase; non-policy checkpoints do not; reset restores patrol then restarts sequence

> EditMode covers policy gating (`NotifyCheckpointActivatedStartsSequenceOnlyForPolicyLevels`: non-policy level no-op, policy level starts sequence); reset-replays-sequence and chase-completion verified in play pass.

- [x] 5.5 Test final-arrival cleanup: advancing to FormalLevel05 unloads FormalLevel04 (monsters included) while keeping {L05, L045}

> Verified live in Play Mode (see 6.2): after the crate-door advance, scene set was {Persistent, SharedArt_L04_L045, SharedArt_L045_L05, FormalLevel045, FormalLevel05}; FormalLevel04 unloaded together with both monsters; pendingUnload=FormalLevel045.

## 6. Regression & Acceptance

- [x] 6.1 Run full EditMode suites; only pre-baseline failures may remain

> Result: 32 tests / 24 passing including all 8 new protocol tests; remaining 8 failures identical to the recorded baseline (pre-existing scene-contract issues).

- [x] 6.2 Play-mode pass: L01 key → L02 handoff; plate-driven edge; L04 → L045 checkpoint chase start; L045 crate door → L05 load with Level04/monster unload; Keypad5 reset unchanged; Keypad8 lands at spawn

> Full acceptance executed 2026-08-24 via play-mode physics simulation (players/crate teleported into trigger volumes so real OnTriggerEnter paths ran):
> - Boot from FormalPersistent: {Persistent, SharedArt_L01_L02, FormalLevel01}, current=FormalLevel01.
> - Physical key pickup (human teleported onto L01_HumanKey): key deactivated, `ToLevel02_door4 (1)` opened, FormalLevel02 loaded, predecessor retained.
> - Plate-driven edge: stepping L02 `Pedal` opened `ToLevel03_door4 (2)`; entering `SuccessorCheckpoint` advanced to FormalLevel03 with SharedArt_L03_L04 preloaded.
> - Keypad6-equivalent advance reached FormalLevel04; normal advance entered FormalLevel045 with pendingUnload=FormalLevel04.
> - **Wiring defect found & fixed during acceptance**: `L045_Checkpoint` still had `successorRegistrationPoint=true`, so touching it advanced straight to Level05 and unloaded Level04 before the chase could matter. Fixed by unchecking that flag in `FormalLevel045.unity` (1-line diff); the crate-door trigger is now the sole L045→L05 handoff, matching the agreed design.
> - Re-run after fix: touching `L045_Checkpoint` starts the sequence (dog orbit follower added, pursuit routine active) WITHOUT route advance; ~10s later both retained monsters reported `forcedChase=True chaseTarget=Human`.
> - Crate teleported onto `L045_CrateDoorTrigger`: local door opened, route advanced to FormalLevel05, FormalLevel04 + monsters unloaded, {L05, L045} kept.
> - Keypad8 equivalent returned to FormalLevel045 and placed the human at the spawn anchor (horizontal distance 0).
> - Keypad5 equivalent reset to the activated checkpoint anchor as before.
> - Zero console errors across the whole session. Final EditMode regression: 32 tests, only the 8 pre-baseline failures remain.
