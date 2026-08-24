## 1. GM Runtime Hooks

- [x] 1.1 Add a runtime-only movement multiplier to the dog actor that preserves its configured walk speed as the baseline.
- [x] 1.2 Add keypad 7 handling to toggle the available dog's multiplier between 1x and 5x and log the active state.

## 2. Level 2 Gate Diagnostics

- [x] 2.1 Expose read-only gate state from the existing L2 pedal, safe-zone interaction, and target-door components without changing the cooperative rules.
- [x] 2.2 Add keypad 4 handling that logs the L2 gate's pedal, two-player safe-zone, E-interaction, and target-door resolution state, including safe handling of missing references.

## 3. Verification

- [x] 3.1 Add or update focused editor tests for dog speed toggling and L2 diagnostic state reporting where practical.
- [x] 3.2 Build the runtime and editor assemblies and validate the OpenSpec change.
- [ ] 3.3 Run the direct L2 Play Mode checklist: checkpoint does not progress, pedal plus both players in safe zone permits human E to open the door, and both GM commands log the expected results.
