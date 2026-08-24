# Tasks: integrate-formal-wwise-music-states

## 1. Runtime Controller

- [x] 1.1 Add `FormalWwiseMusicController` with required `AkAmbient` dependency and serialized Wwise State names/thresholds.
- [x] 1.2 Apply initial States before posting `Play_MusicMode` and avoid duplicate State calls through cached values.
- [x] 1.3 Cache active monsters, refresh on additive scene load/unload, and map any chase to Combat.
- [x] 1.4 Map formal normalized anxiety to Low/Mid/High and recover correctly after restart/state-instance changes.

## 2. Verification

- [ ] 2.1 Verify the new script compiles against the installed Wwise 2025.1 Unity API.
- [x] 2.2 Review the diff to ensure no user-owned scene or Wwise asset changes were overwritten.
- [x] 2.3 Document the `Audio_Music` Inspector hookup and Play Mode checks for no-monster, chase, anxiety, and restart cases.
