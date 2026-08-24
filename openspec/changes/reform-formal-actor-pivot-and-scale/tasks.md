## Tasks

## 0. Body 统一缩放层（后续追加）

- [x] 0.1 Prefab：每 actor 新增 Body 子物体（人 0.7 / 狗 1.5），胶囊移入 Body，锚点留根
- [x] 0.2 `FormalPlayerActor`：去 RequireComponent(CapsuleCollider)，capsule 改 GetComponentsInChildren
- [x] 0.3 `FormalPlayerActors` / `FormalPushableCrate` / `FormalCooperativeRailMover`：actor 碰撞体取子级
- [x] 0.4 `FormalPlayerVisualLoader`：删除 scale 字段与乘算，实例化父节点改 Body，偏移重算并实测校准（人 +1.19、狗 +0.349）
- [x] 0.5 新工具 `Tools/Formal/Actor/Audit Body Fit`
- [x] 0.6 验证：人 sink=2mm / 狗 1.4cm、双贴地、推箱锚点逐位重合；Body 1.5→0.75 冒烟：模型高度精确减半、胶囊同步(0.675)、仍贴地，已还原

## 1. Prefab

- [x] 1.1 Edit `FormalPlayerActors.prefab`: human capsule h1.7/r0.35/centerY0.85; dog capsule h0.9/r0.3/ centerY0.45
- [x] 1.2 Add `FocusAnchor` + `MoverAttachPoint` children (0,+1,0) under both actors; wire serialized refs on `FormalPlayerActor` components
- [x] 1.3 Set actor initial local Y 1 → 0

## 2. Scripts

- [x] 2.1 `FormalPlayerActor`: add serialized anchor fields + `MoverAttachOffset`, `FocusAnchor` accessors (null-safe)
- [x] 2.2 `FormalLevelController`: coincidence placement in both `MovePlayer` overloads
- [x] 2.3 `FormalPushableCrate`: coincidence in `TryEngage` + `KeepActorsAtPoints`
- [x] 2.4 `FormalCooperativeRailMover`: coincidence in engage snap + `ResetTemporaryState`
- [x] 2.5 `FormalPlayerControl.SetCameraTarget`: target FocusAnchor with root fallback

## 3. Verify

- [x] 3.1 Compile clean (no console errors)
- [x] 3.2 L01 play: spawn feet-on-floor (feet=9.904 == floor top), camera framing preserved via FocusAnchor(+1)
- [x] 3.3 Checkpoint respawn uses same `MovePlayer` coincidence path (code-verified; spawn path runtime-verified)
- [x] 3.4 Crate push/pull: programmatic engage → anchor world pos == point pos exactly; feet grounded
- [ ] 3.5 Dog capsule no longer clips floor; visual alignment acceptable after designer loader tuning (handoff note)
