# Tasks: restart-gameplay-music-and-add-parchment-ui-audio

## 1. Gameplay Music Lifecycle

- [x] 1.1 Add the Stop Event and restart delay to `FormalWwiseMusicController`.
- [x] 1.2 Stop, reapply States, and replay gameplay music from the beginning.
- [x] 1.3 Call the music restart once from `ResetCurrentLevel`.

## 2. Parchment UI Audio

- [x] 2.1 Add the shared `FormalParchmentAudio` component.
- [x] 2.2 Route Pause and Settings transitions through the shared open/close Events.
- [x] 2.3 Route Tutorial/notice transitions through the shared open/close Events.
- [x] 2.4 Leave DeathScreen unchanged.

## 3. Unity Hookup and Verification

- [x] 3.1 Refresh/create Unity Wwise references for the four renamed/new Events.
- [x] 3.2 Configure `Audio_Music` in `FormalPersistent` without reserializing unrelated scene data.
- [ ] 3.3 Run static, reference, and compile checks.
- [ ] 3.4 Verify restart and panel behavior in Unity Play mode.
