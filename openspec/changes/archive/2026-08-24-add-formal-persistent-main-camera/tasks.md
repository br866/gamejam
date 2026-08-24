# Tasks: add-formal-persistent-main-camera

## 1. Editor preparation

- [x] 1.1 Open `Assets/Scenes/Test/superbreadman 1.unity` and locate the `PlayerSystem` prefab instance's `MainCamera` child in Hierarchy
- [x] 1.2 Duplicate the `MainCamera` child node (`Ctrl+D`), drag it out of `PlayerSystem` to scene root, and confirm it keeps Camera + CameraFollow + AudioListener + URP Additional Camera Data
- [x] 1.3 Duplicate the scene-root `Global Volume` object the same way

## 2. Transfer into FormalPersistent

- [x] 2.1 Copy both duplicated objects, open/load `Assets/MoMing/FormalLevels/FormalPersistent.unity` (with FormalLevel01/02 additively as in normal testing), and paste them as new root objects; delete the temporary duplicates from superbreadman 1 without saving that scene
- [x] 2.2 On the pasted camera verify: Clear Flags = Solid Color, Background = black (alpha 255), FOV 50, Tag = MainCamera, enabled; reset transform to position (0, 10, 0) rotation (60, 0, 0) or equivalent sane default
- [x] 2.3 On the pasted Global Volume verify it is global, weight 1, and references `Assets/Scenes/Game/superbreadman 2/Global Volume Profile.asset`
- [x] 2.4 Confirm no Human/Dog/Canvas objects were carried over into FormalPersistent

## 3. Verification

- [x] 3.1 Enter Play mode on the formal multi-scene setup: game view background outside rooms is solid black (no blue)
- [x] 3.2 Confirm exactly one enabled MainCamera-tagged camera exists at runtime and it follows the active player actor (Tab switch included)
- [x] 3.3 Confirm post-processing is visible (bloom/vignette/grain present) and Console shows no new errors or double-AudioListener warnings
- [x] 3.4 Save FormalPersistent; run `openspec validate --all`
