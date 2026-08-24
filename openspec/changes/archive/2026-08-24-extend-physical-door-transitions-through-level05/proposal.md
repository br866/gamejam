## Why

Level 4 to Level 4.5 and Level 4.5 to Level 5 still use direct route advancement from their exit mechanisms. Completing an exit therefore places the players in the successor instead of letting them traverse the opened shared door. Level 4.5 also intentionally retains Level 4 for its pursuit sequence, so its physical-arrival confirmation must not require predecessor unloading.

## What Changes

- Extend physical shared-door progression through Level 4, Level 4.5, and Level 5: completion preloads the successor and opens the matching shared door without moving players.
- Confirm each transition only after both players physically reach the successor entry area.
- Introduce a scene-owned, generic exit policy that covers every configured route-producing exit in a source scene, including actuator and crate-door exits, without editing prefab assets.
- Add a Level 4.5 arrival area and make retained-predecessor arrival confirmation complete cleanly while retaining Level 4 for pursuit; release only that retained Level 4 when L05_Checkpoint is reached, while retaining Level 4.5.
- Make L05_Checkpoint commit Level 5 as the recovery level without moving players, and make Level 4.5 recovery restore its retained Level 4 pursuit scene if it is unexpectedly absent.
- Preserve keypad GM direct jumps (2, 6, and 8) as independent immediate transitions with normal player placement.

## Capabilities

### New Capabilities

- `formal-level04-through-level05-physical-door-transition`: Defines physical shared-door progression for Level 4 to Level 4.5 and Level 4.5 to Level 5, including the retained-Level-4 arrival behavior.

### Modified Capabilities

- None.

## Impact

- `FormalGameFlowController`, physical exit policy/binding behavior, and route-producing exit triggers.
- Scene-owned configuration in `FormalLevel04`, `FormalLevel045`, and a Level 4.5 entry seal; no prefab assets are manually edited.
- Focused edit-mode coverage for physical preload, retained predecessor arrival, crate exit handling, and GM interruption.
