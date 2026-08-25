# Tasks: route-fluorescent-light-audio

## 1. Wwise Reference

- [x] 1.1 Add `Play_Fluorescent_Light` to the shared Resources settings.
- [x] 1.2 Preserve Auto-Defined SoundBank loading through `AK.Wwise.Event`.

## 2. Runtime Routing

- [x] 2.1 Discover pendant-lamp models in every loaded formal level.
- [x] 2.2 Post one Event instance from each active lamp's own GameObject.
- [x] 2.3 Cover additive scene loads and prevent duplicate installation.
- [x] 2.4 Exclude wall lamps, floor lamps, and standalone Unity Light objects.

## 3. Verification

- [x] 3.1 Verify the formal route resolves exactly 27 pendant-lamp models.
- [x] 3.2 Verify the new runtime scripts compile.
- [ ] 3.3 Playtest attenuation and simultaneous-voice density in Unity.
