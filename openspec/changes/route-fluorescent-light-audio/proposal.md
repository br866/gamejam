# Proposal: route-fluorescent-light-audio

## Why

The authored `Play_Fluorescent_Light` Wwise Event contains the continuous
electrical bed and intermittent ballast buzz, but no formal-level ceiling-lamp
models currently post it.

## What Changes

- Add the Wwise Event reference to the shared Resources audio settings so its
  Auto-Defined SoundBank is available at runtime.
- Add a persistent formal-light audio router that discovers every loaded
  `pendant_lamp` model, including additive level loads.
- Install one 3D Wwise emitter per discovered ceiling lamp and post the Event
  once while that lamp is active.
- Limit matching to pendant ceiling lamps; wall lamps, floor lamps, standalone
  Unity Light objects, and non-formal scenes are not included.
- Rely on Wwise attenuation and container behavior authored in the Event.

## Impact

- Two runtime audio scripts and one editor bootstrap update.
- The shared Wwise settings asset gains one serialized Event reference.
- No formal scene or lamp-model asset is reserialized.
- Current formal content resolves to 27 ceiling-lamp emitters: Level 1 (6),
  Level 2 (4), Level 4.5 (9), and Level 5 (8).
