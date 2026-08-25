## Context

See proposal.md for motivation. `FormalPlayerActor` owns direct-control velocity and named animation-state playback for both actors. The formal dog controller currently exposes only Walk. `FormalDogOrbitFollower` is enabled for Level 4.5 forced follow and moves the dog independently, with a hard-coded follower speed and no animation-state updates.

## Goals / Non-Goals

**Goals:**

- Give the formal dog one looping Idle state that can be selected when it is at rest.
- Decelerate direct dog control without allowing residual horizontal velocity to persist.
- Make forced-follow pacing derive from the dog’s Inspector-configured walk speed and a configurable multiplier.
- Keep animation state consistent with actual movement in both control modes.

**Non-Goals:**

- Alter human locomotion, input bindings, pathfinding behavior, or the forced-follow route.
- Change the dog’s collision shape, root-motion setting, Wwise events, or model geometry.
- Introduce sprinting for the dog.

## Decisions

### Use a dog-specific `Idle` animator state

The dog’s controller will expose a state named `Idle`, backed by the existing looping `dog idle3` clip from `dog2.fbx`. The actor’s state-selection logic will request `Idle` for the dog rather than the human-specific `Idle1` / `Idle2` variation names.

This keeps the generic actor state machine while avoiding dummy duplicate idle states in the dog controller. Using the existing imported clip avoids adding or duplicating model assets.

### Decelerate horizontal velocity only during direct dog control

When direct dog input is zero, horizontal velocity will move toward zero at a serialized stopping-deceleration rate; vertical velocity remains under existing gravity handling. Idle is selected only below a small horizontal-speed threshold.

This avoids abrupt stopping while preventing the zero-drag rigidbody from retaining unbounded residual velocity. Rigidbody drag was considered, but would affect every movement mode and make the configured locomotion speed less direct.

### Use actor walk speed and a configurable multiplier as the forced-follow speed source

The follower will obtain its active dog’s configured walk speed for each movement update and multiply it by a serialized follow-speed multiplier whose default is 1.3. It will determine Walk versus Idle from the distance actually moved that frame.

This keeps the existing Inspector walk-speed field as the base pace while allowing pursuit follow to be tuned independently. Routing forced following through the direct-input movement method was considered but rejected because forced movement has separate path targets and rotation behavior.

## Risks / Trade-offs

- [A very low deceleration can make the dog feel unresponsive] → Expose a conservative serialized deceleration value and validate its stopping distance in play mode.
- [Transform movement on a non-kinematic Rigidbody can still produce physics inconsistencies] → Preserve the current follow mechanism in this scoped change and use observed displacement only for animation; address any collision redesign separately.
- [The idle clip’s Avatar compatibility could fail at import] → Verify the controller plays both Walk and Idle in the configured formal dog visual before accepting the change.

## Migration Plan

1. Update the formal dog controller and actor/follower behavior.
2. Open the formal scene and confirm direct stop, idle playback, configured-speed following, and follow idle behavior.
3. Roll back by reverting the controller and the two locomotion scripts; no scene data migration is required.
