## Context

See proposal.md. The diagnostic reproduction established that the two Level 2 initial respawn anchors lie within the successor checkpoint trigger volume. Level loading places the persistent players on those anchors, and the trigger's empty prerequisite list allows the route to advance immediately.

## Goals / Non-Goals

**Goals:**

- Make Level 2's initial player placement spatially disjoint from the successor checkpoint trigger.
- Preserve the direct-Level-2 test configuration and normal checkpoint-triggered progression.

**Non-Goals:**

- Change route-advance code, checkpoint prerequisite semantics, door animation, or the L2/L3 shared-art scene.
- Alter player controls, colliders, or the intended position of the successor checkpoint.

## Decisions

### Relocate only the Level 2 initial respawn anchors

Update the two `FormalLevel02` initial respawn-anchor transforms to valid Level 2 entrance positions that lie outside the successor checkpoint's world-space trigger bounds. The checkpoint remains at its intended exit location and retains its current completion behavior.

This is preferred over disabling the checkpoint during loading because the latter adds timing-dependent route state and could suppress an intentional entry. It is also preferred over adding checkpoint prerequisites because direct startup is a spatial-placement defect, not an unmet gameplay condition.

### Verify physical separation and route behavior

Use the direct `FormalLevel02` boot path to verify that startup produces no successor-checkpoint request and leaves `ToLevel03` closed. Then enter the checkpoint intentionally to confirm normal advancement remains available.

## Risks / Trade-offs

- [New anchor lacks a valid ground surface] → Validate ground resolution and player placement in Play Mode.
- [Anchor is outside the checkpoint but not an intended entry area] → Review both player positions in the Level 2 scene before recording the change complete.

## Migration Plan

1. Adjust only the two Level 2 initial respawn-anchor transforms.
2. Run direct Level 2 startup and confirm the shared L2-to-L3 door stays closed.
3. Verify an intentional successor-checkpoint entry still opens the normal progression path.
4. Revert the two transform values if either placement is invalid.
