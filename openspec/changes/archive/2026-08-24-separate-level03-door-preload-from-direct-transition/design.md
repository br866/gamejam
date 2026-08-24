## Context

See proposal.md for motivation. The current Level 3 cooperative exit is a `FormalActuatorTrigger` configured to open the shared transition door and request a successor. That request uses the direct route-transition path, whose intentional behavior includes immediately placing players in the successor level. The existing physical-transition state and the Level 4 entry seal can already distinguish a successor preload from a later physical-arrival confirmation.

## Goals / Non-Goals

**Goals:**

- Give the Level 3 cooperative exit an explicit preload-successor behavior that retains the current level and both player transforms.
- Reuse the existing Level 4 scene-owned two-player entry seal to commit only the matching pending L3-to-L4 traversal.
- Keep direct GM transitions immediate and able to cancel an incomplete physical transition.

**Non-Goals:**

- Change Level 3 puzzle prerequisites, door art, collision, or player controls.
- Add or modify prefab assets manually.
- Change the already verified L2-to-L3 transition behavior.

## Decisions

### Add an explicit preload mode to the cooperative exit trigger

The exit trigger will support a scene-configurable successor-preload action distinct from its existing direct route-advance action. The L3 scene instance will select preload mode; its existing direct advance configuration will be disabled.

Alternative: infer preload mode from the successor scene or generic door name. Rejected because route intent must be explicit per trigger and not depend on fragile scene naming.

### Keep physical confirmation generic and match the pending route

The existing Level 4 entry seal will continue to confirm a preloaded transition only when its scene matches the pending successor. This commits L3-to-L4 without player placement and delegates normal predecessor sealing/cleanup to the existing flow.

Alternative: add a Level-3-specific Level 4 trigger. Rejected because the existing generic entry seal already supplies the required two-player arrival semantics.

### Configure only the Level 3 scene instance

The L3 exit sits inside the `L03_Content` prefab instance. Its behavior will be changed through an override in `FormalLevel03`, using Unity Editor tooling, while leaving the source prefab asset unchanged.

Alternative: modify the `L03_Content` prefab. Rejected by the project constraint against manually modifying prefab assets.

## Risks / Trade-offs

- [The Level 4 entry seal could be reached by an unrelated preload] → Confirmation requires both pending source and successor to match the active route.
- [Serialized scene override may be missed or target the wrong prefab child] → Inspect the L3 scene instance in the Unity Editor and verify its effective fields before Play Mode.
- [GM command arrives during preload] → Retain the existing direct-transition cleanup of pending physical state and test it from L3.

## Migration Plan

1. Extend the trigger's route-output choices with a preload-successor option that does not invoke direct advance.
2. Apply the option as an override on the Level 3 scene instance; do not change its source prefab asset.
3. Add focused coverage for trigger-initiated preload and matching Level 4 arrival confirmation, plus GM interruption.
4. Verify the complete cooperative crossing flow in Play Mode and verify direct GM transitions remain immediate.
