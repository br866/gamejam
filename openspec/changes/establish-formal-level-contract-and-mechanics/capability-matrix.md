# Formal Route Capability Matrix

This matrix separates reusable formal-level infrastructure from level-specific
business configuration. A capability belongs in the common layer when it is
required by at least two formal levels, even if the exact configuration differs.

## Common Layer

| Capability | Evidence | Common responsibility | Level-specific responsibility |
| --- | --- | --- | --- |
| Additive level lifecycle | All six formal levels and the persistent scene | Load, unload, transition, reset, and route ownership | Scene names and successor configuration |
| Shared art retention | Five adjacent shared-art scenes | Retain shared scenes while referenced by active or transitional levels | Explicit shared-scene membership |
| Formal player identity | Level 01 human-only interactions and Level 02 dog-only/cooperative interactions | Resolve Human/Dog roles from formal actor roots and child colliders | Which role a particular interaction accepts |
| Trigger occupancy | Level 01 pedal/checkpoint/exit and Level 02 pressure plates/checkpoint/exit | Deduplicate colliders, track enter/exit, and remove destroyed occupants | Trigger shape and completion prerequisites |
| Checkpoint and reset | Level 01 and Level 02 requirements, with the same route contract for all levels | Store paired respawn anchors and reset registered temporary state | Checkpoint placement and activation condition |
| Progress lifetime | Level 01 permanent doors versus temporary key/crate state; Level 02 persistent gate versus resettable route state | Permanent and resettable state policies with level-local registration | Which interaction uses which policy |
| Door/actuator contract | Level 01 doors and Level 02 route gate | Apply open/closed visual and blocking-collider state | Door asset, animation, and prerequisites |
| Resettable physics occupant | Level 01 crate and the captured Level 5 cabinet requirement | Restore transform and Rigidbody velocity on reset | Object placement and player eligibility |
| Formal scene validation | All six scenes require the same controller/spawn/content/collision contract | Validate required structure and build/catalog references | Level-specific anchors and gameplay references |

## Business Layer

| Level | Business behavior |
| --- | --- |
| Level 01 | Human pushes crate, reaches the key, uses the pedal, opens the route and exit doors |
| Level 02 | Dog-only footprint route and first plate, both-player second plate, monster region/safe space, route gate |
| Level 03 | To be recorded from the approved route before implementation; reuse common trigger/state/actuator contracts |
| Level 04 | To be recorded from the approved route before implementation; reuse common trigger/state/actuator contracts |
| Level 04.5 | To be recorded from the approved route before implementation; reuse common trigger/state/actuator contracts |
| Level 05 | Final-level escape, cabinet interaction, final checkpoint/safe-space/camera decisions remain business-specific |

## Boundary Rules

- Do not create a generic puzzle graph or a level-specific singleton in the common layer.
- Common components expose small contracts; each level owns its references and prerequisite wiring.
- Prototype `GameManager`, `PlayerManager`, `PlayerController`, and `Puzzle/*` remain outside the formal runtime unless a later migration explicitly consumes a behavior.
- Level 03 through Level 05 business behavior must be recorded before implementation, but must not block implementation of the already evidenced common contracts.
