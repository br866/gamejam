## Context

See proposal.md. `FormalCheckpoint.OnTriggerEnter` currently records checkpoint state and, when `successorRegistrationPoint` is set, calls the flow controller's route-advance notification. In Level 2, direct initial placement overlaps that checkpoint. The Level 2 pedal and `L02_CooperativeSafeZoneTrigger` are also currently configured to request advancement directly; no E interaction exists in Level 2.

## Goals / Non-Goals

**Goals:**

- Give every checkpoint one explicit owning level identity and prevent duplicate ownership registration.
- Separate checkpoint/respawn state from route progression.
- Preserve the Level 2 cooperative interaction as the only door-opening path.

**Non-Goals:**

- Relocate initial respawn anchors or transition-door geometry.
- Change the pedal/safe-zone completion conditions, player controls, or route catalog.
- Make checkpoints open any shared transition door.

## Decisions

### Register checkpoint ownership before player placement

When a formal level becomes active, its checkpoint components register their owning-level association with a level-scoped registry. The registry accepts each checkpoint identity once for the duration that level is loaded. Player placement occurs after this registration, so a startup overlap cannot be interpreted as a new level/route event.

This is preferred to a global boolean because it remains correct when different levels are loaded additively and when a level is unloaded then loaded again.

### Remove route-advance semantics from checkpoints

`FormalCheckpoint` continues to supply local checkpoint/respawn behavior only. Its former successor-registration behavior must not call route-advance APIs; therefore it cannot open `ToLevel03` from a spawn overlap or later entry.

This is preferred to adding timing guards around `OnTriggerEnter`, because the checkpoint is never an authorized completion source for the Level 2 door.

### Make E in the cooperative safe zone the transition operation

Clear direct route-advance settings from the Level 2 pedal and safe-zone components. Add the existing door-interaction behavior to `L02_CooperativeSafeZoneTrigger`, with the pedal and safe zone itself as prerequisites. Its E operation opens the L2-to-L3 door and invokes the normal route transition only after those conditions are met.

Using the existing safe-zone collider is preferred over adding a second interaction object because players already must remain inside that volume at the moment of the cooperative interaction.

## Risks / Trade-offs

- [A scene lacks an owning-level configuration] → Emit a clear validation error and refuse to register it.
- [Old scenes still mark checkpoints as successor-registration points] → Migrate or ignore that setting so it cannot request route advancement.
- [Registration state persists across an unload/reload] → Scope registry lifetime to the loaded level instance and clear it on unload.

## Migration Plan

1. Add owning-level registration data and one-time registration handling to formal checkpoints.
2. Configure formal checkpoints to their containing formal level and remove their route-advance role.
3. Clear direct advancement from the Level 2 pedal and safe zone, then attach/configure the safe-zone E door interaction.
4. Directly boot Level 2 and confirm `SuccessorCheckpoint` cannot open `ToLevel03`.
5. Verify that only the pedal, safe-zone, E-key sequence opens the door and begins the Level 3 transition.
