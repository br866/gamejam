# Formal Route Audit

Audit date: 2026-08-18

## Confirmed Route Mismatches

| Area | Current state | Required resolution |
| --- | --- | --- |
| Build startup | The first build scene was `Scenes/Game/Start`, and its start button loaded legacy `Level1`. | Use `FormalPersistent` directly as the first enabled build scene; it loads `FormalLevel01`. |
| Transition lifecycle | `FormalGameFlowController` unloads every other formal level immediately after loading a target. | Retain the direct predecessor until the successor checkpoint activates; then unload the predecessor and unreferenced shared art. |
| Level identities | Level 03 uses `FormalLevel03`; Levels 04, 04.5, and 05 are blank. | Use the route-catalog identifiers `Level03`, `Level04`, `Level04.5`, and `Level05`. |
| Checkpoints | Only Level 02 and Level 03 contain formal checkpoints, and both have blank `successorScene` values. | Configure a checkpoint in every level and only commit handoff after the configured successor checkpoint; scene-level objects must be selected before wiring. |
| Exits | No formal level scene currently contains a `FormalLevelExit` instance. | Select and configure an approved exit trigger per non-final level, with gameplay prerequisites. |
| Validation | `FormalTraversalValidationTests` checks only Levels 01 and 02 entrance/checkpoint geometry. | Expand validation to all six route entries, their build registration, identity, roots, checkpoints, exits, shared art, and safe spawn space. |
| Final completion | Level 05 has no configured final completion behavior or player-facing ending. | Design and wire the approved Level 5 final-room trigger and completion presentation. |

## Existing Assets Confirmed Usable

- `FormalPersistent` holds the route catalog, persistent flow controller, player spawner, and formal Level 1 initial scene name.
- All six formal levels and five shared-art scenes are registered in Build Settings.
- Every formal level has a `FormalLevelController`, paired spawn anchors, a content root, and a collision root.
- Formal role resolution, occupancy triggers, resettable mechanism state, doors, checkpoints, and resettable physics occupants already exist as a foundation.

## Scope Boundary

This audit does not choose unapproved art objects as exits, plates, safe zones, monster regions, or Level 5 gameplay objects. Those assignments remain level-specific implementation tasks and require scene review before wiring.
