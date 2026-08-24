## 1. Trigger Code Changes (`FormalActuatorTrigger.cs`)

- [x] 1.1 Add `prerequisiteMode` enum field (`RequireAll` default / `RequireAny`) and branch `PrerequisitesComplete()` accordingly, keeping empty-list pass-through
- [x] 1.2 Add `opensTransitionDoor` bool field; when set, completion calls the flow controller to open the transition door from this scene toward the route successor
- [x] 1.3 Remove the duplicated `successorScene` loading block in `CompleteTrigger`

## 2. Flow Controller Changes (`FormalGameFlowController.cs`)

- [x] 2.1 Add public helper that resolves the route successor for a given scene from the catalog and opens its shared transition door (reusing `FindTransitionDoor` + `OpenPermanently`)
- [x] 2.2 Add arrival cleanup to the forward path of `LoadLevelRoutine`: close every door inside the previous two level scenes, then unload every level scene except the new current level and its direct predecessor
- [x] 2.3 Verify restart/reset paths (`RestartCurrentLevelRoutine`, `ResetCurrentLevel`) remain byte-for-byte behaviorally unchanged

## 3. Compile and Regression

- [x] 3.1 Refresh Unity compilation and confirm zero console errors
- [x] 3.2 Run EditMode traversal validation tests; adjust any expectation that encodes the old accumulate-forever lifecycle

## 4. Manual Level 3 Wiring (level owner, Inspector)

- [ ] 4.1 Set each plate's `requirement` (dog-only / human-only / both-player) on `L03GameplayRoot` triggers
- [ ] 4.2 Drag interior doors (`door5 (2)`..`door5 (5)`) into the chosen plates' `actuators`; set `permanent` per door intent
- [ ] 4.3 Tick `opensTransitionDoor` on the shared-door plate; optionally fill `successorScene = FormalLevel04` for preload

## 5. Play Mode Verification

- [ ] 5.1 In Level 3, verify an interior door opens only through its wired plate's eligibility rules
- [ ] 5.2 Verify the shared Level 3 / Level 4 door opens via the transition plate, and still opens via the checkpoint handoff without the plate
- [ ] 5.3 Advance two levels: confirm doors of the previous two levels are closed, the oldest scene is unloaded, and actors left behind do not fall out of the world
- [ ] 5.4 Verify restart and reset behave exactly as before the change

## 6. Pedal Component Unification

- [x] 6.1 Replace `FormalMechanismPedal` on `L01_Mechanism_Pedal.prefab` with `FormalActuatorTrigger` configured HumanOnly + permanent; wire `actuators[0] = L01_Door_Mechanism` in `FormalLevel01.unity` and drop the stale `linkedDoor` override
- [x] 6.2 Delete `FormalMechanismPedal.cs` and update `FormalTraversalValidationTests` references
- [x] 6.3 Re-run EditMode suite and confirm no new failures beyond the pre-existing Level 02 scene issues

## 7. Plate Simplification

- [x] 7.1 Remove `prerequisiteMode` / `prerequisites` / `completionState` and all gating branches from `FormalActuatorTrigger`; keep `requirement`, `actuators`, `permanent`, `opensTransitionDoor`, `successorScene`
- [x] 7.2 Remove the reflection assertion on the deleted prerequisites field from `FormalTraversalValidationTests`
- [x] 7.3 Strip orphan `FormalMechanismState` components co-located with triggers in Levels 02/03/04/04.5/05 (14 removed)
- [x] 7.4 Recompile and re-run EditMode suite; confirm only the pre-existing Level 02 scene failures remain
