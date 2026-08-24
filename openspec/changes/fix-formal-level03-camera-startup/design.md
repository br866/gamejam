## Context

FormalPersistent creates and owns FormalMainCamera with CameraFollow when no follow camera exists. FormalLevel03 also contains a legacy Main Camera with no follow target. Both are enabled, tagged MainCamera, and render at depth zero.

## Goals / Non-Goals

**Goals:**
- Give the formal runtime one unambiguous main camera.
- Preserve the persistent camera-follow path and its human target.

**Non-Goals:**
- Retune camera distance, angle, collision, or controls.
- Delete the legacy GameObject or modify the reference source scene.

## Decisions

### Disable and untag the Level 3 prototype camera

The scene-local camera remains present for edit-time reference but its Camera component is disabled and its tag changed to Untagged. FormalMainCamera remains the only active main camera supplied by the persistent formal flow.

Alternative considered: delete the local camera. Rejected because preserving the existing object minimizes scene churn and retains the original reference setup.

## Risks / Trade-offs

- [A future direct scene test expects the prototype camera] -> Formal testing is intentionally entered through FormalPersistent, whose follow camera is now the authoritative view.
