## 1. Pull-state dispatch

- [x] 1.1 Change the crate mover’s pull-animation dispatch to place its attached actor in the existing Pulling state.
- [x] 1.2 Change the cooperative rail mover’s pull-animation dispatch to place its attached actor in the existing Pulling state.

## 2. Validation

- [x] 2.1 Verify the project compiles without Console errors after the dispatcher changes.
- [ ] 2.2 In play mode, verify crate and rail-mover pull actions play Pull, while push and no-input actions continue to play Push and Idle.
