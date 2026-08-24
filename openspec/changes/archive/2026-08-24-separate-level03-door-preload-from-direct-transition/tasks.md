## 1. Route Trigger Behavior

- [x] 1.1 Add a scene-configurable successor-preload route action to `FormalActuatorTrigger` that does not invoke direct advancement or move players.
- [x] 1.2 Preserve the trigger's existing direct-advance behavior for configurations that do not select successor preload.

## 2. Level 3 Scene Configuration

- [x] 2.1 Use Unity Editor tooling to configure the L3 cooperative exit scene instance for successor preload and disable its direct-advance route action.
- [x] 2.2 Verify the L3 scene instance override targets the intended prefab child while the source prefab asset remains unchanged.

## 3. Arrival and Direct-Transition Safety

- [x] 3.1 Verify the existing L4 entry seal confirms only the matching pending L3-to-L4 physical arrival without player placement.
- [x] 3.2 Add focused test coverage for L3 trigger preload, partial/full L4 arrival, and GM direct-transition interruption.

## 4. Verification

- [x] 4.1 Build runtime and editor assemblies and validate the OpenSpec change.
- [x] 4.2 In Play Mode, verify cooperative L3 exit opens/preloads without teleporting, one player cannot confirm, both players confirm by crossing, and direct GM transitions remain immediate.
