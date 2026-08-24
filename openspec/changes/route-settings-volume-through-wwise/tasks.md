# Tasks: route-settings-volume-through-wwise

## 1. Audio Backend Boundary

- [x] 1.1 Preserve project-wide Unity audio disablement.
- [x] 1.2 Force cutscene embedded audio to direct platform output.
- [x] 1.3 Remove settings-menu calls into legacy Unity volume controls.

## 2. Wwise Volume RTPCs

- [x] 2.1 Resolve strongly typed `MusicVolume` and `SFXVolume` references in the shared Wwise settings asset.
- [x] 2.2 Map normalized slider values to the authored 0–100 Game Parameter range.
- [x] 2.3 Restore saved values after Wwise initialization and reinitialization.

## 3. Verification

- [x] 3.1 Validate OpenSpec artifacts.
- [x] 3.2 Compile the Unity project without new C# errors.
- [ ] 3.3 Verify the video direct-audio exception and both sliders in Play Mode.
