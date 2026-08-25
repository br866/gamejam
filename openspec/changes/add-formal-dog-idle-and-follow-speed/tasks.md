## 1. Dog idle animation

- [x] 1.1 Add the looping dog idle clip as an `Idle` state in the FormalDog animator controller while preserving the existing Walk state and root-motion configuration.
- [x] 1.2 Update dog animation-state selection so idle dog actors request `Idle`, while human idle variation behavior is unchanged.

## 2. Direct-control deceleration

- [x] 2.1 Add a serialized dog stopping-deceleration setting and reduce only horizontal velocity toward zero when direct dog movement input is absent.
- [x] 2.2 Keep the dog in Walk until its horizontal velocity reaches the resting threshold, then set it to Idle.

## 3. Forced-follow pacing and animation

- [x] 3.1 Add a serialized forced-follow speed multiplier (default 1.3) and move the dog using its configured walk speed multiplied by that value.
- [x] 3.2 Set forced-follow dog animation to Walk when actual displacement exceeds the resting threshold and Idle otherwise.

## 4. Validation

- [x] 4.1 Verify compilation and inspect the Unity Console for errors after the script and controller changes.
- [x] 4.2 In the Formal Level 4.5 flow, verify direct dog deceleration and Idle playback, configurable 1.3× default forced following, and forced-follow Idle at destination.
