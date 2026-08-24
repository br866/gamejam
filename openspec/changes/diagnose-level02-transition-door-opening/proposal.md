## Why

Directly booting Formal Level 2 intermittently presents the Level 2 to Level 3 shared transition door as opened before the players reach an intended Level 2 completion trigger. The current route cannot identify which runtime source requested the opening, making the fault impossible to distinguish from scene state.

## What Changes

- Add opt-in diagnostic evidence for every Level 2 route-advance request and transition-door open operation.
- Record the initiating component, active route scene, target door, and runtime call site in the Unity Console.
- Preserve direct startup into `FormalLevel02` as the reproduction path; do not alter gameplay gating or the route catalog in this diagnostic change.

## Capabilities

### New Capabilities

- `formal-level02-transition-diagnostics`: Provides attributable runtime evidence when the Level 2 to Level 3 transition door is opened or Level 2 requests route advancement.

### Modified Capabilities

- None.

## Impact

- Affects the formal route flow controller, transition-door runtime component, and Level 2 completion sources as needed for diagnostic context.
- Adds Unity Console output only while diagnostics are enabled; no scenes, player placement, route order, or gate behavior change.
