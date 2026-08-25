# Proposal: route-level045-music

## Why

The Wwise project now contains `Play_Level5_Music` and `Stop_Level5_Music`,
but the formal route never posts them. The design document's “Level 5 long
corridor” is represented by `FormalLevel045`; the later square puzzle room is
the runtime's `FormalLevel05`.

## What Changes

- Treat `FormalLevel045` as the exclusive special-music scope.
- Stop the normal interactive gameplay music and post `Play_Level5_Music`
  when the flow controller commits arrival into `FormalLevel045`.
- Post `Stop_Level5_Music` and restore the existing gameplay-music State rules
  when the flow controller commits arrival into another level.
- Base the boundary on `FormalGameFlowController.CurrentLevelScene`, not on
  additive scene loading, so preloaded and retained scenes cannot switch music.
- Include the special track in death, restart, and cleanup lifecycle handling.

## Impact

- Extends `FormalWwiseMusicController` with one route-specific music mode.
- Adds two serialized Wwise Event references to `FormalPersistent`.
- Does not change level geometry, triggers, or transition rules.

