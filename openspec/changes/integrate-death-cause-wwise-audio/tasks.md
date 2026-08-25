# Tasks: integrate-death-cause-wwise-audio

## 1. Runtime Lifecycle

- [x] 1.1 Expose a safe gameplay-music stop operation.
- [x] 1.2 Add the shared death-audio component and cause-to-State mapping.
- [x] 1.3 Trigger death audio from the existing death screen exactly once.
- [x] 1.4 Stop death music and the active stinger before restart/title exit.

## 2. Unity Hookup

- [x] 2.1 Add Unity Wwise references for the generated death Events.
- [x] 2.2 Configure the component on the existing `Audio_Music` GameObject.

## 3. Verification

- [x] 3.1 Verify Wwise Event, State, media, and SoundBank dependencies.
- [x] 3.2 Run static reference and compile-oriented checks.
- [ ] 3.3 Verify both death causes, restart, and return-to-title in Play mode.
