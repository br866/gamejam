## Context

The retained Level 3 Pad is near `(-35.97, 10.74, -5.96)`. FormalLevel03 already owns separate human and dog spawn anchors, dedicated floor collision, and broad static collision coverage. See `proposal.md` for motivation.

## Goals / Non-Goals

**Goals:**
- Relocate only the two existing entry anchors to clear positions next to the Pad.
- Validate floor support, player-capsule overlap, and immediate movement clearance.

**Non-Goals:**
- Move, enable, or configure the Pad and its future mechanics.
- Change check-point or exit anchors, renderer placement, collision coverage, controls, or player ownership.

## Decisions

### Use supported positions on the Pad's western side

The anchors will sit beside the Pad rather than on its trigger/visual volume, with sufficient horizontal separation for the human and dog capsules. The local floor volume remains the supporting surface.

Alternative considered: place players directly on the Pad. Rejected because the Pad is reserved for later mechanic behavior and can overlap its future trigger region.

## Risks / Trade-offs

- [Nearby static prop collision prevents entry] -> Test both anchor capsules and movement away from the Pad before saving.
- [Changing startup state affects normal testing] -> Temporarily select FormalLevel03 only for direct Play Mode verification, then restore FormalLevel01.
