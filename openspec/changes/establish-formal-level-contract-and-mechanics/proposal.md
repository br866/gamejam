## Why

The formal route now has six independently assembled scenes, but their shared gameplay contract is only partially encoded in runtime components and is not consistently enforced. Existing puzzle scripts still depend on the prototype's single-scene `GameManager` and `PlayerManager`, which conflicts with the formal route's persistent players and additive scene flow.

Establish a fast, explicit formal level runtime before adding more level-specific gameplay, so levels can be navigated, loaded, unloaded, reset, and tested consistently while shared art remains alive for every level that uses it.

## What Changes

- Define the required runtime structure for a formal playable level: controller, paired spawn anchors, content root, collision root, checkpoint, and exit integration.
- Define formal level lifecycle semantics for level entry, checkpoint activation, reset, successor loading, and prior-level unloading.
- Provide synchronous and asynchronous APIs to load or unload a named formal level, plus next-level, previous-level, and direct-jump GM commands.
- Define explicit additive shared-art scene ownership so content used by two levels is retained until no loaded or transitional level references it.
- Provide a reusable formal trigger policy that distinguishes either player, human-only, dog-only, and supported physics occupants without relying on prototype tags or controllers.
- Provide a reusable mechanism contract that separates state-producing interactions from environment actuators.
- Establish reusable formal implementations for a permanent human-only interaction, a permanent door, and resettable mechanism state.
- Migrate the existing Level 01 formal key, pedal, door, and pushable crate behavior to the common contracts without changing their player-visible behavior.
- Add editor validation for the common formal-level scene contract.
- Keep prototype `Puzzle/*`, `GameManager`, and `PlayerManager` operational but outside the formal route; do not migrate anxiety, UI, monsters, audio, or navigation in this change.

## Capabilities

### New Capabilities
- `formal-level-contract`: Required scene structure and lifecycle behavior for every formal playable level.
- `formal-level-runtime-management`: Route catalog, synchronous/asynchronous level APIs, GM navigation commands, and shared additive-scene retention.
- `formal-mechanic-foundation`: Reusable formal trigger, state, and actuator behavior for cooperative level mechanics.
- `formal-level-validation`: Editor validation of the formal scene contract and essential traversal setup.

### Modified Capabilities
- None.

## Impact

- Affected runtime scripts: `Assets/MoMing/Scripts/LevelRuntime/*` and the existing Level 01 formal mechanic scripts.
- Affected editor tooling: `Assets/Editor/FormalTraversalValidationTests.cs` and related formal scene validation helpers.
- Affected assets: `FormalLevel01.unity`, `FormalPersistent.unity`, and formal mechanic prefabs only as required to wire common components.
- No new package dependencies and no changes to prototype scene behavior.
