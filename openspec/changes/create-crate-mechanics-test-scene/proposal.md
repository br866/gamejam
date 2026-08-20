## Why

The wooden-crate interaction is still unstable when tested inside FormalLevel01, where unrelated level layout and progression systems make reproduction difficult. A focused scene is needed so movement, engagement, collision, and reset behaviour can be observed and adjusted without changing the formal route.

## What Changes

- Add an isolated crate mechanics test scene containing the existing formal player actors, the existing movable wooden crate, and a ground plane.
- Provide the minimal level controller, spawn anchors, lighting, and camera setup required to run the existing player and crate interaction in Play Mode.
- Keep the scene separate from the formal level route while using its feedback to correct the shared crate interaction.
- Remove the crate's fixed travel limits so a connected human can move it continuously along the selected axis.
- Restore stable backward movement by using the attached idle state instead of the `Pull` animation.

## Capabilities

### New Capabilities
- `crate-mechanics-test-scene`: Provides a standalone, repeatable environment for testing the formal wooden-crate interaction.

### Modified Capabilities

- None.

## Impact

- Adds a Unity scene under `Assets/MoMing/Scenes/Test/`.
- Reuses `FormalPlayerActors`, `L01_MovableStep_WoodenCrate`, `FormalPlayerControl`, and `FormalCooperativeRailMover`.
- Updates shared crate movement and the crate interaction regression test.
