## Why

Formal Level 3 must support the same established plate-to-door conventions as Levels 1 and 2, but currently its interior doors are wired to nothing, no plate can open the shared Level 3 / Level 4 door, and there is no "any one prerequisite" completion mode. In addition, forward route progression never unloads old levels and never closes their doors, which leaves stale geometry that can drop a lingering dog into the void.

## What Changes

- Add a prerequisite mode to `FormalActuatorTrigger`: `RequireAll` (default, current behavior) or `RequireAny` so a plate can complete when any one prerequisite completes while existing plates stay unchanged.
- Add an `opensTransitionDoor` option to `FormalActuatorTrigger` so a completing plate can open the shared boundary door through the existing `FormalGameFlowController.OpenTransitionDoor` path already used by the Level 1 key and checkpoint handoff.
- Fix the duplicated successor-scene loading block inside `FormalActuatorTrigger.CompleteTrigger`.
- Add arrival cleanup to `FormalGameFlowController`: when the route advances to the next level, close every door inside the previous two level scenes and unload all older level scenes; shared-art pruning keeps using the existing unused-shared-art logic.
- Configure Formal Level 3 scene objects by hand in the Inspector: plates open specific interior doors via direct `actuators` references, and one plate opens the shared Level 3 / Level 4 door via the new option.

Explicitly out of scope: checkpoint auto-open behavior stays as-is, `FormalPrerequisiteActuator` is not extended, and the broken null actuator entry on the Level 2 dog plate is left untouched.

## Capabilities

### New Capabilities

- `formal-level03-plate-door-contract`: Defines how Formal Level 3 plates complete (role requirements, prerequisite modes) and how they open interior doors or the shared transition door following the Level 1 / Level 2 conventions.
- `formal-route-level-retention`: Defines the forward-progression cleanup contract for the formal route: closing doors of the two most recent levels and unloading older levels when a newer level arrives.

### Modified Capabilities

## Impact

- Code: `Assets/MoMing/Scripts/LevelRuntime/FormalActuatorTrigger.cs`, `Assets/MoMing/Scripts/LevelRuntime/FormalGameFlowController.cs`.
- Scene configuration (manual, by owner): `Assets/MoMing/FormalLevels/FormalLevel03.unity` plate triggers (`L03GameplayRoot`) and interior doors (`door5 (2)`..`door5 (5)`).
- Verification: Unity compile, existing EditMode traversal validation tests, Play Mode walkthrough of Level 3 into Level 4.
