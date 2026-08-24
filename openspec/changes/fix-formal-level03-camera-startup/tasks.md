## 1. Camera Ownership

- [x] 1.1 Inventory enabled cameras, tags, render depths, and follow targets after FormalPersistent loads FormalLevel03.
- [x] 1.2 Disable and untag FormalLevel03's scene-local prototype camera without deleting it.
- [x] 1.3 Preserve FormalMainCamera and its formal human follow target.

## 2. Validation And Handoff

- [x] 2.1 Direct-load FormalLevel03 through FormalPersistent and verify exactly one enabled MainCamera.
- [x] 2.2 Verify the remaining main camera follows FormalHumanActor and the Level 3 prototype camera does not render.
- [x] 2.3 Restore FormalLevel01 as the default startup level and record the camera inventory and resolution.
