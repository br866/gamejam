## 1. Level 4.5 recovery reset

- [x] 1.1 Add a Level 4.5/Level 5 recovery operation that closes their shared transition door before player control resumes.
- [x] 1.2 Add a targeted reset for the Level 4.5 crate-exit trigger so the closed exit can be opened again.

## 2. Validation

- [x] 2.1 Verify Unity compiles with no Console errors.
- [x] 2.2 In Level 4.5, open the Level 5 exit, die before handoff, and verify the respawned retry has a closed, re-openable door.
- [x] 2.3 Complete the Level 5 handoff, die in Level 5, and verify recovery closes the shared Level 4.5-to-Level 5 door without disrupting Level 5 recovery.
