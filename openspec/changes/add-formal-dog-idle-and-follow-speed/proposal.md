## Why

The formal dog has no idle state in its animator controller, so a stopped dog can remain visually in its walk animation. Its forced-follow movement also uses a separate hard-coded speed, causing it to drift out of sync with the dog’s configured walking pace.

## What Changes

- Add an explicit Idle animation state for the formal dog and use it after the dog has decelerated to rest.
- Replace abrupt dog stopping with configurable horizontal deceleration when there is no movement input.
- Drive forced-follow speed from the dog actor’s configured walk speed multiplied by a configurable follow-speed multiplier (default 1.3).
- Make forced-follow movement report Walk or Idle animation state according to actual movement.

## Capabilities

### New Capabilities
- `formal-dog-locomotion`: Defines responsive dog stopping, idle animation, and configured-speed forced-follow behavior.

### Modified Capabilities
- None.

## Impact

- `FormalPlayerActor` input stopping and dog animation-state selection.
- `FormalDogOrbitFollower` movement speed and animation-state updates.
- Formal dog animator controller and its animation clip reference.
