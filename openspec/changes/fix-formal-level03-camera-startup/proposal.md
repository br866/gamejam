## Why

Formal Level 3 starts with both the persistent formal follow camera and a scene-local prototype camera enabled at the same render depth. The competing cameras produce an incorrect or unstable player view.

## What Changes

- Disable FormalLevel03's scene-local prototype camera for formal runtime loading.
- Remove its `MainCamera` tag so the persistent formal follow camera is the sole main camera.
- Verify direct FormalPersistent startup into FormalLevel03 has exactly one enabled main camera following the human actor.

## Capabilities

### New Capabilities
- `formal-level03-single-camera-startup`: Defines unambiguous formal-camera ownership when Formal Level 3 loads.

### Modified Capabilities
- None.

## Impact

- Affects the existing `Main Camera` component and tag in FormalLevel03.
- Does not modify camera-follow behavior, player controls, art, collision, or source scenes.
