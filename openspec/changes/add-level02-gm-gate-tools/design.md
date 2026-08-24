## Context

See proposal.md. The Level 2 door now requires a dog-operated pedal, two occupants in the safe zone, and E from the human. Its components have independent runtime state, while the dog stores its normal movement speed on the actor component.

## Goals / Non-Goals

**Goals:**

- Expose a concise, attributable Console report for the exact Level 2 gate failure.
- Toggle only the dog's runtime speed multiplier with keypad 7.

**Non-Goals:**

- Change L2 puzzle conditions, save the GM state, or affect human movement speed.
- Add production UI or global cheats for other levels.

## Decisions

### Use keypad 4 for the gate-status command

Keypad 7 is reserved by the user for the dog-speed toggle; keypad 4 reports the L2 gate. The report reads the actual pedal/safe-zone/door-interaction components rather than inferring readiness from route state.

### Preserve configured dog speed as the toggle baseline

The dog actor retains a separate runtime multiplier, so keypad 7 applies 5x without overwriting its serialized walking speed. A second press resets the multiplier to 1x.

## Risks / Trade-offs

- [Missing L2 references] → Report the missing component explicitly instead of throwing.
- [GM keys are pressed outside L2] → Gate status reports that L2 is inactive; dog-speed toggle remains a deliberate test-only control.

## Migration Plan

1. Add the diagnostic and speed-toggle hooks with safe missing-reference reporting.
2. Run direct Level 2 startup and use keypad 4 before and after each prerequisite.
3. Toggle keypad 7 twice and verify the dog returns to normal speed.
