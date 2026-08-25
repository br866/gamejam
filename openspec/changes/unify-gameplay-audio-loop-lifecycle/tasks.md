# Tasks: unify-gameplay-audio-loop-lifecycle

## 1. Shared Gameplay Gate

- [x] 1.1 Add a centralized read-only Unity gameplay simulation state.
- [x] 1.2 Cover pause menu, generic zero-timescale panels, tutorial, and death.

## 2. Crate Loop Lifecycle

- [x] 2.1 Connect the free-push crate loop to the shared state.
- [x] 2.2 Connect the rail-mover crate loop to the shared state.
- [x] 2.3 Preserve movement-driven restart after gameplay resumes.

## 3. Verification

- [x] 3.1 Verify OpenSpec structure and requirement coverage.
- [x] 3.2 Run static diff checks and compile the Unity runtime assembly.
- [ ] 3.3 Verify pause, tutorial, and death transitions in Play mode.
