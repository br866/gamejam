# Design: route-monster-footsteps-through-wwise

## Approach

`MonsterPatrol` measures horizontal distance travelled after its movement update. Reaching the configured stride distance posts the assigned Wwise Event on the monster GameObject. Patrol and chase use independent thresholds so the faster chase can retain a heavier, longer stride.

This is implemented in the shared behavior rather than an art prefab or animation event because the formal monsters are scene-authored objects with three different visual children. Distance triggering also avoids sounds while navigation is blocked.

## Defaults

- Patrol stride: 1.8 metres.
- Chase/forced-chase stride: 2.2 metres.
- Event: `Play_Footstep_Brutedoc`.

## Safety

The cadence origin is reset in `Awake` and `ResetPatrol`. A missing Event skips playback and emits only one warning per component instance.
