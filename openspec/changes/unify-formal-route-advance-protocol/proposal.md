# Proposal: unify-formal-route-advance-protocol

## Why

"Finish this level, go to the next one" is currently implemented through 7 different code paths (key, actuator plate, exit checkpoint, crate trigger, Keypad2/6/8 GM shortcuts) with inconsistent behavior: two different sources of truth for the successor scene, four different ways to silently drop a request while a scene load is running, and door lookups by fragile name substring. These inconsistencies are the recurring source of route-flow bugs. Separately, arriving at Level04.5 normally never starts its chase sequence (only the Alpha2 debug assembly does), which was confirmed as an oversight, and backward GM travel leaves players standing in the previous level's coordinates.

## What Changes

- Add a single flow-controller entry point `RequestRouteAdvance()`; all gameplay sources (key, actuator trigger, exit checkpoint, crate door trigger) call it instead of poking doors/scenes directly.
- Derive the successor level solely from the route catalog (`routeCatalog[i+1]`); remove per-source hardcoded edges and serialized successor fields as behavioral inputs.
- Replace the `"ToLevel"` substring first-match door lookup with a registered per-edge door name token stored in the route catalog.
- Record advance requests that arrive while `operationInProgress` into a single pending slot (with originating scene) and drain it when routines finish; no request is silently lost.
- Introduce centralized edge-policy predicates (`ShouldRetainPredecessor`, `HasArrivalSequence`) replacing scattered `== "FormalLevel045"` string checks.
- Start the L045 chase sequence from the L045 checkpoint activation (arrival policy); remove the crate-door band-aid callback `NotifyLevel045DoorOpened`.
- **BREAKING** Remove the Alpha2 debug assembly (`StartGmLevel045`, `LoadGmLevel045Routine`, `gmLevel045Pending`, `CloseLevel045To05Doors`, `FindCheckpoint`, `OpenAllDoorsInScene(string)`).
- Route Keypad2 (next level) through the new protocol so GM travel matches gameplay behavior; make Keypad8 (previous level) place players at the target level spawn point.
- Preserve existing behavior: arriving at Level05 keeps {L05, L045} and unloads FormalLevel04 together with its monsters; Keypad5 reset behavior unchanged.

## Capabilities

### New Capabilities

- `formal-route-advance-protocol`: Defines the single route-advance entry point, catalog-derived successor selection, busy-window request retention, transition-door registration lookup, edge policies for predecessor retention and arrival sequences, L045 chase trigger via checkpoint activation, L05 arrival cleanup, and consistent GM travel behavior.

### Modified Capabilities

- None. (The prior `cross-level-transition-lifecycle` delta from `defer-cross-level-unload` was never synced to main specs; this change supersedes that lifecycle behavior under the new capability.)

## Impact

- `UnityProject/Assets/MoMing/Scripts/LevelRuntime/FormalGameFlowController.cs` (major rewrite of advance/restart plumbing)
- `FormalHumanKey.cs`, `FormalActuatorTrigger.cs`, `FormalCrateDoorTrigger.cs` (call-site slimming), `FormalCheckpoint.cs` (activation notification hook)
- `UnityProject/Assets/MoMing/FormalLevels/FormalPersistent.unity` (route-catalog door-name token backfill)
- New EditMode test file(s) for advance idempotency, busy-window retention, door registry resolution, L045 arrival sequence, and L05 arrival cleanup
- Does NOT touch `FormalDoor.cs` (avoids conflict with `complete-formal-route-playable-flow` task 3.3), legacy MoMing Puzzle system, or Keypad5 reset semantics
