## 1. Level 5 control handoff

- [x] 1.1 Restore normal human/dog switching when L05_Checkpoint commits Level 5, without moving either actor.
- [x] 1.2 Preserve forced dog-follow and human-only control until that checkpoint handoff.

## 2. Dynamic forced pursuit

- [x] 2.1 Extend forced monster pursuit to evaluate both active player actors and choose the nearer valid target per monster.
- [x] 2.2 Retarget during forced pursuit when proximity changes, while preserving the existing delay, navigation, attack, and fallback behavior.

## 3. Verification

- [x] 3.1 Add focused edit-mode coverage for Level 5 control release and nearest-actor forced pursuit selection.
- [x] 3.2 Run focused tests and OpenSpec validation, then verify in Play Mode that Level 5 restores dog switching and pursuit monsters dynamically attack the nearer human or dog.
