## Why

The project needs one durable record of the agreed SuperBreadMan level behavior before implementation decisions are made. The level-plan PDF is one input among several; this change captures the decisions made in discussion without treating the current runtime code as proof that any behavior is complete or correct.

## What Changes

- Capture the five-level route as a reference requirement, including the first four puzzle stages and the fifth-level controlled escape stage.
- Define per-level checkpoint behavior: every level has a checkpoint, and either character activating it establishes the current respawn point.
- Define temporary progress (keys and movable-object positions) separately from permanent progress (completed mechanisms, opened doors, and checkpoints).
- Define hard character eligibility for keys, character-specific mechanisms, two-character mechanisms, and cooperative pushing.
- Define doors as physical blockers that play an opening animation, then permanently remove their blocking physics for the current level lifetime.
- Define ordinary hiding as visual obstruction only, while a safe space is physically inaccessible to monsters and prevents attacks.
- Define the fifth-level escape as controlled: no character switching, no voluntary separation, fixed camera, newly unlocked running ability, real-time monster pursuit, and cooperative physical cabinet pushing.
- Record unresolved decisions without implementing or silently choosing them.

## Capabilities

### New Capabilities

- `superbreadman-final-level-requirements`: Captures the agreed gameplay contract, progress lifetime, character eligibility, monster behavior, and five-level route.

### Modified Capabilities

- None. Existing OpenSpec changes remain historical/planning context and are not rewritten by this capture.

## Impact

- Future impact areas include `Assets/MoMing/Scripts/`, the target level scenes, checkpoint/door/interaction configuration, monster behavior, camera control, and level lifecycle management.
- This capture intentionally does not modify runtime scripts, scenes, prefabs, assets, controls, or navigation.
- The fifth-level checkpoint location, whether the cabinet area is a safe space, and the final completion presentation remain open decisions.
