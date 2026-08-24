## 1. Diagnostic Instrumentation

- [x] 1.1 Add an opt-in Level 2 to Level 3 route-advance diagnostic at the formal flow-controller boundary, including active route state and a call path.
- [x] 1.2 Add an opt-in state-change diagnostic for the registered shared transition door, including door and scene identity plus a call path.
- [x] 1.3 Add source-object context for Level 2 completion components that can request advancement, without altering their completion behavior.

## 2. Direct-Level-2 Reproduction

- [x] 2.1 Keep `FormalPersistent` configured to boot directly into FormalLevel02 and confirm diagnostics are enabled for that path.
- [x] 2.2 Run the direct Level 2 reproduction and capture the first route-advance or door-open Console sequence.

## 3. Verification And Follow-Up

- [ ] 3.1 Verify diagnostic-disabled behavior leaves the existing Level 2 route and door state unchanged.
- [ ] 3.2 Run relevant EditMode checks and inspect the Unity Console for compile errors.
- [x] 3.3 Record the responsible caller and create a separate behavior-fix change if progression gating requires alteration.
