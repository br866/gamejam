## Why

Formal crate and rail-mover pull interactions currently leave the attached character in Idle despite the actor controller supporting a Pull state. This makes a valid pull action lack its intended visual feedback.

## What Changes

- Trigger the actor Pull state when a crate or cooperative rail mover determines that its attached character is pulling.
- Preserve existing Push and Idle behavior for the same interactions.

## Capabilities

No specification delta: this corrects the existing `crate-push-movement` requirement that pull input triggers a pull animation; it does not change the input contract or introduce new behavior.

## Impact

- `FormalPushableCrate` and `FormalCooperativeRailMover` attached-animation dispatch.
- Existing FormalHuman animator Pull state; no animation assets or input mappings change.
