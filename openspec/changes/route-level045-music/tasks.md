# Tasks: route-level045-music

## 1. Wwise references

- [x] 1.1 Add Unity Wwise references for `Play_Level5_Music` and `Stop_Level5_Music`.
- [x] 1.2 Assign both Events on the persistent formal music controller.

## 2. Route switching

- [x] 2.1 Detect committed entry into and exit from `FormalLevel045`.
- [x] 2.2 Crossfade from normal gameplay music into the corridor track.
- [x] 2.3 Stop the corridor track and restore normal State-driven music on exit.
- [x] 2.4 Ignore additive preload/unload state when the current route level has not changed.

## 3. Lifecycle and verification

- [x] 3.1 Stop the active soundtrack on death and restart the correct soundtrack after reset.
- [x] 3.2 Compile the runtime script.
- [ ] 3.3 Playtest Level 4 -> 4.5 -> 5 transitions in Unity.
