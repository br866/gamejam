## Context

See proposal.md. The formal route opens a registered shared transition door through the flow controller, while several scene-local components can request route advancement. Direct startup in FormalLevel02 preloads the L02/L03 shared-art scene, so the diagnostic must distinguish a loaded door from a door whose runtime state was changed.

## Goals / Non-Goals

**Goals:**
- Produce one attributable Console trail from a Level 2 completion request through door resolution and door opening.
- Keep diagnostic output constrained to the investigated Level 2 to Level 3 edge.

**Non-Goals:**
- Fix or redesign Level 2 progression conditions.
- Change initial-level configuration, scene load order, door animation, or trigger geometry.
- Add persistent analytics or logging dependencies.

## Decisions

### Instrument the transition boundary and its immediate sources

Add a diagnostic helper/flag at the route controller boundary and include object context from the Level 2 completion components that invoke it. Log the registered-door resolution and the door state transition, with a managed stack trace at the request/open boundary.

This is preferred to adding logs only to individual triggers: a new or overlooked caller would still appear at the controller and door boundaries. It is also preferred to unconditional logging across every level, which would obscure the reproduction with unrelated route traffic.

### Preserve direct Level 2 boot configuration

The current `FormalPersistent` initial-level setting remains untouched, so the reproduction stays focused on direct Level 2 startup. The logs distinguish preloading from an explicit open call instead of changing the startup path.

### Keep diagnostics opt-in and removable

Use a serialized or compile-time-local diagnostic control with an explicit default that avoids noisy production Console output. The diagnostic has no gameplay side effects and can be removed once the source is identified.

## Risks / Trade-offs

- [Stack traces can be verbose] → Emit them only for the investigated edge while diagnostics are enabled.
- [A caller bypasses the flow controller] → The door-level record still captures the operation and its call path.
- [Console timing changes perception of async loading] → Include active scene and operation state in each record.

## Migration Plan

1. Add the opt-in diagnostics without changing Level 2 startup.
2. Reproduce direct Level 2 boot and collect the first request/open sequence.
3. Disable or remove diagnostics after the responsible caller is confirmed; make any gameplay repair in a separate behavior-change proposal.
