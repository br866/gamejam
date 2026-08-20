## Context

The formal crate uses a reusable prefab with `FormalCooperativeRailMover`; player movement and interaction are supplied by `FormalPlayerActors`. See proposal.md for motivation and the accompanying crate-mechanics-test-scene spec for required behavior.

## Goals / Non-Goals

**Goals:**
- Reuse the production player and crate prefabs so the test exercises the same runtime components.
- Keep the scene small enough that crate behaviour can be reproduced without route, puzzle, or art dependencies.
- Supply the scene-owned prerequisites for physics, player placement, and a visible camera view.

**Non-Goals:**
- Changing player movement controls or formal-level scene layout.
- Adding the scene to the production build route.
- Creating a permanent crate test framework beyond this one focused scene.

## Decisions

### Reuse formal prefabs directly
The scene will instantiate `FormalPlayerActors` and `L01_MovableStep_WoodenCrate` rather than copy their objects. This keeps failures representative of the formal level and allows future prefab fixes to be exercised immediately. Duplicating these objects would make the test drift from production configuration.

### Add only scene-level prerequisites
The scene will contain a ground plane, spawn anchors, a level controller, a main camera with the existing follow behaviour, and a directional light. This is enough for actor physics, player control, crate reset registration, and visual verification. Puzzle triggers and formal route services are unnecessary.

### Keep the test out of build settings
The scene is an editor-facing diagnostic environment, so it remains outside the enabled route. Developers can open it directly without changing shipping scene flow.

### Use unbounded movement and an idle backward pose
The crate mover will accumulate travel without clamping it to authored minimum and maximum values. Forward movement retains the `Push` state; backward movement sets the attached actor to idle rather than invoking `Pull`, whose animation conflicts with the mover's forced interaction-point position. Keeping the rollback inside the mover avoids changing general player animation behavior.

## Risks / Trade-offs

- The test uses the existing input-driven player controls, so automated interaction validation is limited. → Verify configuration in Edit Mode and enter Play Mode for manual crate exercise.
- The player prefab includes both actors, while the crate is human-only. → The dog remains available to confirm that role switching does not unintentionally enable crate engagement.
- Unbounded motion can push the crate beyond a playable floor in a production scene. → Formal level layout must provide physical boundaries where a route needs them; the isolated test ground is intentionally broad.
