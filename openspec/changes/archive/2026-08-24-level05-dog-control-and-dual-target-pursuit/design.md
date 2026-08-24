## Context

See proposal.md for motivation. `FormalPlayerControl` already exposes the forced human-only state used by Level 4.5, and retained-scene pursuit currently starts by assigning every monster the human transform once after a ten-second delay.

## Goals / Non-Goals

**Goals:**

- Release forced human-only input at the L05 checkpoint handoff without changing player positions.
- Re-evaluate each forced-pursuit monster's target throughout pursuit using the human and dog actors' current horizontal distances.

**Non-Goals:**

- Change the existing ten-second delay, monster room limits, attack wind-up, or normal non-forced detection behavior.
- Add scene objects, alter Prefab assets, or rebalance monster speeds and damage.

## Decisions

### Restore switching at the Level 5 checkpoint handoff

The Level 5 checkpoint already marks the end of the retained-L4 pursuit segment and establishes Level 5 recovery. The handoff will also clear forced human-only control there. This avoids relying on a later entrance seal and applies at the same point that the player perceives Level 5 has begun.

Alternative: clear the restriction in the Level 5 entrance seal. Rejected because the checkpoint must already establish Level 5 recovery and players can die before reaching the seal.

### Let each forced monster choose its own nearest actor repeatedly

Forced chase will receive or resolve the human-and-dog pair and choose the closest valid actor whenever it repaths. Selection is per monster, so different monsters may chase different actors. Horizontal distance will be used to avoid height differences changing apparent proximity.

Alternative: retarget only in the flow controller on a fixed global timer. Rejected because it introduces another scheduling path and makes response latency unrelated to the monster navigation loop.

## Risks / Trade-offs

- [Rapid distance ties cause visible target jitter] → retain the current target when distances are equal within a small tolerance.
- [A player actor is unavailable during a transition] → select the other valid actor; end forced chase only when neither actor is valid.
- [Restoring dog input before L4 cleanup completes] → tie it to the Level 5 checkpoint handoff, which is already the pursuit cleanup boundary.

## Migration Plan

1. Extend the Level 5 checkpoint handoff to clear forced human-only control.
2. Extend forced monster pursuit with dynamic dual-actor target selection.
3. Add focused coverage and verify both behaviors in Play Mode.

Rollback is limited to reverting the control release and dynamic target-selection changes; no scene migration is needed.
