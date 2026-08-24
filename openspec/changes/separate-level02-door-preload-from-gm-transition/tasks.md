## 1. Route-Flow Separation

- [x] 1.1 Add explicit Level 2-to-Level 3 preload and physical-arrival confirmation APIs that do not reposition players.
- [x] 1.2 Ensure GM keypad 2, 6, and 8 direct transitions clear pending physical transitions and retain existing immediate placement behavior.
- [x] 1.3 Update the L2 E door interaction to request successor preload rather than direct route advance.

## 2. Physical Arrival Setup

- [x] 2.1 Add a scene-owned two-player Level 3 arrival trigger beyond the L2-to-L3 shared door without modifying prefab assets.
- [x] 2.2 Wire the arrival trigger to confirm only the matching preloaded L2-to-L3 transition.

## 3. Verification

- [x] 3.1 Add focused tests for preload-without-placement, two-player arrival confirmation, and GM interruption behavior.
- [x] 3.2 Build runtime and editor assemblies and validate the OpenSpec change.
- [x] 3.3 In Play Mode, verify E keeps players in L2 while L3 loads, one actor cannot confirm arrival, both actors can confirm without teleporting, and keypad 2/6/8 remain direct transitions.
