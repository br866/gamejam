## Context

See proposal.md. `RequestRouteAdvance` is intentionally a direct-transition API: it loads a successor, marks it active, and places players at that level's respawn anchors. It is correct for GM keypad 2, 6, and 8, but not for an opened shared door that players must cross physically.

## Goals / Non-Goals

**Goals:**

- Preload L3 after L2's valid E door interaction without changing player transforms.
- Confirm L3 only when both actors enter a dedicated arrival volume on the L3 side of the shared door.
- Keep direct GM transitions authoritative and immediate.

**Non-Goals:**

- Change the L2 pedal, safe-zone, or human-E prerequisites.
- Use checkpoints as route-transition triggers.
- Modify prefab assets manually; the arrival volume is a scene-owned object.

## Decisions

### Maintain a separate physical-transition state

The flow controller will record a pending `FormalLevel02 -> FormalLevel03` preload separately from `currentLevelScene`. Preloading loads the successor additively but does not activate its scene, move players, or start predecessor cleanup. This preserves L2 as authoritative until physical arrival is confirmed.

Alternative: reuse `RequestRouteAdvance` with a no-teleport boolean. Rejected because its other side effects (active-scene change and cleanup) also occur too early and would make GM and physical semantics ambiguous.

### Use a dedicated L3-side two-player arrival trigger

A scene-owned trigger volume beyond `ToLevel03` requires both actors. It calls an explicit physical-arrival API only when the pending L2-to-L3 preload matches, then the controller changes the current level without placement.

Alternative: reuse `FormalCheckpoint`. Rejected because checkpoints are local respawn state by the confirmed level-binding contract and must never advance route progression.

### Let GM commands cancel pending physical transitions

Any direct GM route change clears a pending preload before performing its existing operation. This prevents a delayed L3 arrival trigger from committing an obsolete route state.

## Risks / Trade-offs

- [Arrival volume placement misses the walkable exit] → Validate crossing it with both actors after opening the shared door.
- [A player returns to L2 before both arrive] → The trigger only completes while both eligible actors are present.
- [GM interrupt during preload] → Clear pending state before the immediate GM operation.

## Migration Plan

1. Add the preload and physical-arrival APIs without changing direct GM transition methods.
2. Configure the L2 E interaction to preload rather than directly advance.
3. Add and position the L3-side two-player arrival trigger in the L3 scene.
4. Verify E opening, partial crossing, completed crossing, and each GM command.
