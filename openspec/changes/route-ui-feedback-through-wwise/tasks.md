# Tasks: route-ui-feedback-through-wwise

## 1. Runtime Integration

- [x] 1.1 Add a persistent router with strongly typed Hover and Click Event references.
- [x] 1.2 Scan loaded scene roots for active and inactive Buttons without modifying their existing callbacks.
- [x] 1.3 Add pointer enter, left-press, selection, and submit handling for interactable Buttons.
- [x] 1.4 Refresh periodically using unscaled time so runtime-created Buttons are covered while paused.
- [x] 1.5 Post both Events from one registered 2D Wwise emitter.

## 2. Editor Setup

- [x] 2.1 Create the shared Resources settings asset from the authored Event GUIDs when missing.
- [x] 2.2 Preserve Wwise Auto-Defined SoundBank references and avoid scene/prefab reserialization.

## 3. Verification

- [x] 3.1 Compile the Unity project without new C# errors.
- [ ] 3.2 Verify `Start` main-menu buttons receive hover and click feedback.
- [ ] 3.3 Verify `FormalPersistent` pause, settings, tutorial, and death buttons receive feedback while `Time.timeScale` is zero.
- [ ] 3.4 Verify disabled Buttons stay silent and existing `onClick` behavior remains intact.
