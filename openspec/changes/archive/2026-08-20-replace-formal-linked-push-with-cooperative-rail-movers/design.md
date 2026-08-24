## Context

The formal player controller currently implements Q by placing the dog at a fixed offset from the human. `FormalHumanCratePush` separately lets the human press F to apply force to a dynamic crate, which has free horizontal movement. This change replaces those formal-route behaviors with explicit cooperative node engagement while leaving the prototype stack untouched.

## Goals / Non-Goals

**Goals:**

- Make heavy-object cooperation legible through two physical positions and explicit F interaction.
- Guarantee cardinal four-direction bounded movement and deterministic reset for L01 and L05 movers.
- Keep existing single-active-actor controls usable after both actors lock into the mover.

**Non-Goals:**

- Change legacy prototype Q linked mode or `PushableBox`.
- Add general inverse kinematics, multiplayer networking, or arbitrary joint simulation.
- Change the human-only ownership of the Level 1 key route.

## Decisions

### Replace formal Q binding with node engagement

Formal Q no longer attaches the dog to the human. Each actor instead presses F near its own designated mover node. The second engagement starts cooperation; pressing F again from either node cancels it.

Automatic engagement was rejected because the player needs to understand that both actors are deliberately contributing. Requiring both actors to hold F continuously was rejected because the formal route has one active actor input stream.

### Select a cardinal axis from angle-selected direction groups

Each mover has four cardinal direction groups, each with a separately configurable Human and Dog GameObject point. An actor's F press compares the horizontal angle from the mover to the actor against the four group directions and selects the closest group within the interaction tolerance. The second actor must select the same group, but does not need to stand opposite the first actor. The group selects the horizontal movement axis. The currently active actor's camera-relative movement input is projected onto that axis, allowing movement in both directions while both actors stay at their configured group points.

Simultaneous per-actor input was rejected because the current game uses one keyboard-controlled active actor at a time.

### Use a kinematic cardinal transform

The mover remains kinematic and is advanced by two bounded local coordinates rather than force integration. Its model-local collider remains physical; attached actors are positioned at matching cardinal offsets while the mover advances. Reset restores the saved transform and clears all engagement state.

Free Rigidbody force was rejected because it allows diagonal drift, unstable friction behavior, and non-deterministic stopping. Configurable joints were rejected as unnecessary for a fixed single-axis puzzle.

### Configure direction points and limits per object

Each mover owns four direction groups of editable Human/Dog GameObject points, an interaction distance, angular tolerance, two-axis travel range, and movement speed. L01 and L05 share the component while retaining per-object limits.

Hard-coding world X was rejected because future movers may be oriented differently.

## Risks / Trade-offs

- [Attached actor collides with mover] -> Ignore only the attached actors' colliders against the mover while engaged and restore collisions on detach.
- [Mover ends inside geometry] -> Clamp to configured travel limits and validate start/end poses in the owning level scene.
- [Player death occurs while attached] -> Level reset detaches actors before repositioning them.
- [Control input is unclear] -> Keep F as the single explicit engage/cancel interaction and show a later UI hint if needed.

## Migration Plan

1. Add the reusable mover and formal-control integration with focused EditMode tests.
2. Replace the L01 human-force push behavior with configured crate nodes and rail bounds.
3. Verify human-only key reachability after cooperative crate placement and reset from engaged and disengaged states.
4. Configure the L05 cabinet with the same component during the controlled escape implementation.
5. Leave legacy prototype linked push intact for its legacy scenes.
