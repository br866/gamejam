# Proposal: route-checkpoint-audio

## Why

Formal-level checkpoint carpets currently save recovery positions without
posting the authored `Play_CheckpointSFX` Wwise Event, so successful checkpoint
activation has no matching audio confirmation.

## What Changes

- Add an optional Wwise checkpoint Event reference to `FormalCheckpoint`.
- Post the Event exactly once when a checkpoint successfully commits its human
  and dog recovery anchors.
- Assign `Play_CheckpointSFX` to every placed formal checkpoint carpet in Levels
  2, 3, 4, 4.5, and 5.
- Add the generated Wwise Event object reference required for AutoBank loading.
- Do not add a checkpoint to Level 1; that level intentionally uses its initial
  spawn as the recovery fallback.

## Impact

- One formal checkpoint runtime script changes.
- Four formal scenes and the Level 2 checkpoint prefab gain one serialized Event
  reference.
- One Wwise Event ScriptableObject and its `.meta` file are added.
- Checkpoint save, tutorial, route-transition, and respawn behavior remain
  unchanged.
