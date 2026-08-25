## Context

See proposal.md for motivation. Both supported mover types already distinguish Pull from Push while resolving input, and `FormalPlayerActor` already maps its Pulling state to the `Pull` controller state. Their pull-animation adapters incorrectly dispatch Idle instead.

## Goals / Non-Goals

**Goals:**

- Route each attached mover’s existing pull decision into the actor’s Pulling state.
- Preserve current Push and Idle dispatches.

**Non-Goals:**

- Change crate movement, rail movement, input conventions, or animator-controller assets.

## Decisions

The fix will use the existing mover-interaction state API with its pull argument, rather than adding a parallel animation path. This preserves the actor’s rotation lock and animation transition behavior for both mover types.

## Risks / Trade-offs

- [A mover may classify an unexpected direction as Pull] → Validate pull, push, and no-input interactions for both crate and rail movers after the dispatch correction.
