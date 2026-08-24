## Design

### Pivot math

Old: root = capsule center; capsule h=2, center offset 0 → bottom = root.y − 1.
New: root = feet; capsule center offset = +h/2 → bottom = root.y.
Equivalence rule: `root_new = authored_point − MoverAttachPoint.localPosition` with `MoverAttachPoint.localPosition = (0, +1, 0)` reproduces old body positions bit-for-bit for identical inputs. The anchor Y is a semantic constant (legacy half-height); FocusAnchor defaults to the same value but is a free tuning knob.

Anchor local offset has zero X/Z, so actor yaw (spawn rotation, mover-facing rotation) never displaces the anchor — plain subtraction is rotation-safe.

### Wiring points

- `FormalPlayerActor`: expose `MoverAttachOffset` (Vector3) and `FocusAnchor` transform; both null-safe (`Vector3.zero` / root fallback) so unwired actors behave exactly as before.
- `FormalLevelController.MovePlayer` (both overloads): subtract offset before `SetPosition(AndRotation)`.
- `FormalPushableCrate.TryEngage`, `KeepActorsAtPoints`; `FormalCooperativeRailMover` engage snap + `ResetTemporaryState`: same substitution. The crate's crate-motion delta (`transform.position - GetPointPosition(humanPoint)`) compares the crate to the authored point and stays untouched because actors keep coinciding with those points every frame.
- `FormalPlayerControl.SetCameraTarget`: pass `activeActor.FocusAnchor`.

### Prefab edits (FormalPlayerActors.prefab)

- Human capsule: height 1.7, radius 0.35, center y 0.85.
- Dog capsule: height 0.9, radius 0.3, center y 0.45.
- Four new child GameObjects (Transform-only): `FocusAnchor`, `MoverAttachPoint` under each actor at (0, +1, 0).
- Actor initial local poses: y 1 → 0 (container children rest feet-on-container-plane before first teleport).
- Serialized fields on each `FormalPlayerActor` component: wire the two anchors.

### Deliberate non-changes

- Visual prefabs and loader scale/offset fields untouched — designer tunes model size there.
- No scene files edited; spawn/checkpoint/grip data keeps its legacy meaning ("attach point lands here").
- Jump/gravity/grounding logic untouched: `IsGrounded` uses capsule bounds and adapts automatically.

### Risks

- Slimmer dog capsule may open unintended low gaps — accepted by stakeholder.
- Any future code that places actors by raw root position must use the coincidence rule; noted in code comments.
