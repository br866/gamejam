## Context

See `proposal.md`. Existing components already implement isolated portions of the intended reusable behavior: `HoldSwitch` checks the human role, `PressurePlate` counts arbitrary colliders, `GateController` and several puzzle objects reset from `GameManager.OnLevelReset`, and `MonsterPatrol` plus `LevelMonsterNavigation` each apply rectangular bounds independently. `FormalLevel02` is deliberately not part of this change; it will configure these generic capabilities later.

## Goals / Non-Goals

**Goals:**
- Establish small reusable components and contracts that represent player roles, occupancy, prerequisites, reset state, hostile regions, and safe zones.
- Preserve existing public scene behavior unless a component is explicitly migrated to the common contract.
- Make component state testable without loading a particular formal level.

**Non-Goals:**
- Configure any Level 2 trigger, gate, checkpoint, exit, monster waypoint, or world-space boundary.
- Replace the character switching, linked movement, anxiety, or scene-loading systems.
- Create a general graph authoring tool, dynamic obstacle solution, or new navigation dependency.

## Decisions

### Model occupant identity by player role, not collider count

Trigger logic will resolve a collider to the player role it represents and track presence by role. This supports human-only, dog-only, either-role, and both-role puzzles without a second collider on one character satisfying a cooperative requirement.

Alternative considered: extend `PressurePlate.requiredCount` with more count flags. Rejected because counts cannot distinguish the human from the dog and lead to unreliable cooperative state.

### Separate generic state from level wiring

Generic components expose serialized role, prerequisite, persistence, and boundary configuration plus state needed by dependent objects. Formal levels provide their own references and scene coordinates in a later change.

Alternative considered: create a universal scene controller with lists of every puzzle object. Rejected because it makes otherwise reusable components depend on a level-specific object graph.

### Use a shared reset registration contract

Reusable progression objects register with the existing level-reset signal and clear occupants, completion, and dependent state through one deterministic reset path. Existing components can be adapted incrementally rather than globally replaced.

Alternative considered: let each component retain its custom reset method. Rejected because prerequisite state and gates can reset in inconsistent orders.

### Make safe zones first-class enemy constraints

Enemy movement and capture checks consult the same safe-zone configuration. Navigation receives the hostile region and excludes safe-zone geometry or nodes so route planning cannot contradict chase behavior.

Alternative considered: rely only on physical blockers or only on `MonsterPatrol` bounds. Rejected because either alone can allow paths or capture checks to cross the intended boundary.

## Risks / Trade-offs

- [Existing scenes rely on raw occupant counts] -> Preserve legacy behavior by default and migrate only explicitly configured components after focused tests.
- [Collider hierarchies do not resolve consistently to player roles] -> Centralize role resolution and test child-collider, multiple-collider, exit, and destruction cases.
- [Reset event ordering differs across old components] -> Give generic progression a stable reset order and test gates and prerequisites together.
- [Navigation cannot represent a safe-zone shape exactly] -> Start with explicit bounded regions and blocker-aware scanning; fail validation when the configured safe zone is still reachable.

## Migration Plan

1. Add and test generic role resolution and role-aware trigger occupancy without changing formal-level scene bindings.
2. Add generic prerequisite and reset interfaces, then adapt gates and plates behind compatibility-preserving serialized fields.
3. Add shared hostile-region and safe-zone constraints to monster movement and navigation.
4. Add focused EditMode and PlayMode tests for generic state transitions.
5. Implement `implement-formal-level02-mechanics` as the first scene consumer after generic behavior is verified.
