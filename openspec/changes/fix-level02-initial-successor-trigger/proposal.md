## Why

Direct startup into `FormalLevel02` places both players inside the Level 2 successor checkpoint's trigger volume. The checkpoint therefore requests the Level 2 to Level 3 transition during scene loading and opens the shared exit door before Level 2 is played.

## What Changes

- Ensure Formal Level 2's direct-start player placement cannot activate its successor checkpoint.
- Preserve the intended successor checkpoint as the sole Level 2 to Level 3 completion source once players intentionally reach it.
- Retain direct `FormalLevel02` startup as a supported test path.

## Capabilities

### New Capabilities

- `formal-level02-initial-progress-gate`: Keeps Level 2's initial spawn outside its successor checkpoint while preserving intentional progression.

### Modified Capabilities

- None.

## Impact

- Formal Level 2 scene spawn/checkpoint placement and direct-start play behavior.
- No route catalog, shared-door implementation, or unrelated level progression changes.
