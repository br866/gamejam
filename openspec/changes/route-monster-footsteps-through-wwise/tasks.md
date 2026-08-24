# Tasks: route-monster-footsteps-through-wwise

## 1. Runtime Integration

- [x] 1.1 Add the serialized Wwise Event and patrol/chase stride settings to `MonsterPatrol`.
- [x] 1.2 Post footsteps from actual horizontal movement, including forced chase.
- [x] 1.3 Reset cadence across formal level resets and warn once for missing setup.

## 2. Unity Hookup and Verification

- [x] 2.1 Create the Unity Wwise Event reference for `Play_Footstep_Brutedoc`.
- [x] 2.2 Assign it to all three formal monster instances and the reusable Enemy Monster prefab.
- [x] 2.3 Compile the runtime assembly and verify all serialized references.
- [ ] 2.4 Audition patrol and chase cadence in Unity/Wwise Profiler.
