## 1. Test Scene Setup

- [x] 1.1 Create an isolated crate mechanics scene outside the formal route.
- [x] 1.2 Add a walkable ground surface, directional light, and direct prefab instances for the formal actors and movable crate.

## 2. Verification

- [x] 2.1 Verify the scene hierarchy contains the required actors, crate, and ground without formal route scene dependencies.
- [x] 2.2 Enter Play Mode and verify the scene loads without console errors and presents the crate interaction environment.

## 3. Crate Interaction Corrections

- [x] 3.1 Remove fixed travel clamping from the shared crate mover and retain axis-only movement.
- [x] 3.2 Replace the backward `Pull` animation request with the attached idle state.
- [x] 3.3 Update the crate mover regression test to verify continuous travel and stable backward interaction.
- [x] 3.4 Verify the corrected interaction in the crate test scene.

## 4. Formal Level Application

- [x] 4.1 Remove obsolete travel-limit serialization from the formal crate prefabs.
- [x] 4.2 Verify FormalLevel01 loads with the corrected shared crate behavior.
- [x] 4.3 Run the full EditMode regression suite after applying the fix.

## 5. Test Scene Stability Fixes

- [x] 5.1 Remove competing Rigidbody and Transform writes that cause visible crate flicker.
- [x] 5.2 Stabilize the F engagement lock by freezing only player rotation while attached.
- [x] 5.3 Allow re-engagement from an interaction point and verify repeated engage, move, and cancel cycles in CrateMechanicsTest.

## 6. Renderer Flicker Correction

- [x] 6.1 Disable motion-vector generation on the manually transformed crate renderer.
- [x] 6.2 Disable Rigidbody interpolation for the manually transformed crate and verify its renderer remains stable during Play Mode movement.

## 7. Attached Actor Jitter Correction

- [x] 7.1 Remove the pre-movement attached-actor snap so the human is repositioned only after the crate moves.
- [x] 7.2 Verify the human and crate move together without an intermediate per-frame position jump.

## 8. Re-engagement Axis Reset

- [x] 8.1 Rebase crate movement origin and travel on every new engagement.
- [x] 8.2 Add regression coverage for changing movement axes without a position jump.
- [x] 8.3 Verify the corrected re-engagement behavior in CrateMechanicsTest.

## 9. F Interaction Diagnostics

- [x] 9.1 Remove the previous per-frame crate position log.
- [x] 9.2 Add F press, attach, release, and failure-reason diagnostics.
- [ ] 9.3 Reproduce a failed F attach and use the diagnostics to identify its cause.
