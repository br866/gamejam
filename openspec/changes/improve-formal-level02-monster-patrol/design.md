# Design: improve-formal-level02-monster-patrol

## Context

`MonsterPatrol` disables itself in `Awake` when `waypoints` contains no valid entries (`HasAssignedWaypoints`). The Level02 monster lives inside the `L02_Content` prefab (source guid `b45a1db82e4433d45900e1a5f107db48`) with `MonsterPatrol` and `AudioSource` added as prefab-instance components. Its serialized `waypoints` array holds seven null entries, so the monster never patrols.

The Level02 scene already provides two waypoint transforms under `Level02GameplayRoot`:

```text
L02_MonsterWaypointA  (-6.97, 15.82, 5.61)
L02_MonsterWaypointB  ( 8.00, 15.82, 5.61)
```

Both sit inside the monster room bounds (`roomCenter (-1, 16, 0)`, `roomSize (27, 10, 20)` → x in [-14.5, 12.5], z in [-10, 10]).

## Goals / Non-Goals

**Goals:**
- Make the Level02 monster active using existing scene objects only.
- Preserve the existing patrol → detect → chase → safe-zone → catch behavior chain.
- Add regression coverage so the monster can never silently lose its waypoints again.

**Non-Goals:**
- Rewriting `MonsterPatrol` or `LevelMonsterNavigation` logic.
- Adding A* navigation components or grid graphs in this change.
- Adding new waypoints, moving existing objects, or changing monster tuning values.
- Audio work (deferred per user instruction).

## Approach

Prefab instances may override component fields with references to scene objects. The change adds prefab-instance modifications on the `L02_Content` instance in `FormalLevel02.unity`:

```text
MonsterPatrol.waypoints.Array.size      = 2
MonsterPatrol.waypoints.Array.data[0]   = L02_MonsterWaypointA (scene Transform)
MonsterPatrol.waypoints.Array.data[1]   = L02_MonsterWaypointB (scene Transform)
```

The edit is applied through the Unity Editor serialization API (open scene additively, locate the `MonsterPatrol` inside the prefab instance, set the array, save) so Unity writes well-formed modification entries.

## Verification Model

1. EditMode regression test: the Level02 monster's `waypoints` array has exactly two entries, both non-null, and both transforms belong to the `FormalLevel02` scene.
2. Play Mode checks:
   - Monster patrols between A and B without errors.
   - Player entering the room triggers chase; leaving the room or entering the safe zone ends it.
   - Players inside `L02_CooperativeSafeZoneTrigger` are never caught.
   - Keypad 5 restart returns the monster to patrol at its start position.
3. Existing `FormalTraversalValidationTests` must keep passing.

## Risks

- Scene waypoint transforms referenced from a prefab instance are serialized as instance overrides; deleting the waypoints would null the references. The regression test guards this.
- If direct (non-pathfinding) movement proves insufficient around furniture, navigation wiring is recorded as follow-up work instead of being forced into this change.
