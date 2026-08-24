## 1. Cooperative Rail Mover Foundation

- [x] 1.1 Add a reusable cooperative rail mover with four editable cardinal direction point groups, two-axis travel limits, speed, engagement state, collision handling, and reset registration.
- [x] 1.2 Replace formal Q position binding with explicit F-based node engagement while preserving Tab character switching outside active cooperation.
- [x] 1.3 Route active formal actor input through an engaged mover's selected cardinal axis and keep both attached actors aligned to their matching sides.
- [x] 1.4 Add cancellation, interruption, endpoint, and reset behavior that stops and locks the mover deterministically.

## 2. Level 1 Crate Migration

- [x] 2.1 Remove the formal human-only force-push path from the Level 1 crate without changing prototype push behavior.
- [x] 2.2 Configure cardinal crate sides and bounded L01 travel that reaches the existing usable step placement.
- [x] 2.3 Verify the crate remains locked until both actors engage, moves only along its selected axis, and restores its initial transform on reset.
- [ ] 2.4 Verify the human can use the placed crate for the key route while the dog remains unable to collect the key.

## 3. Level 5 Reuse And Verification

- [ ] 3.1 Configure the Level 5 medicine cabinet to use the cooperative rail mover during controlled escape.
- [x] 3.2 Add EditMode tests for node eligibility, single-axis bounds, cancellation, actor attachment, reset, and L01 crate placement.
- [ ] 3.3 Run Play Mode verification for L01 engagement, push/pull, cancellation, endpoint behavior, reset, and route continuation.
- [ ] 3.4 Verify push and pull animation selection follows each actor's actual side and safely falls back when a controller lacks a named state.
