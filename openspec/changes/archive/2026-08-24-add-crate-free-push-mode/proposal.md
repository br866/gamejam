# Proposal: add-crate-free-push-mode

## Why

第一关木箱（`FormalPushableCrate`）在 Auto 模式下挂点后推轴被锁死为"人→箱子"方向，想换方向必须松手(F)、绕到另一面重新挂点，操作割裂且不符合"抓住箱子一起走"的直觉。需要一种挂点后人相对箱子不动、箱子可全向移动的模式；同时清理残留未用的 `travelLimit` 机制。

## What Changes

- `FormalPushableCrate.PushAxisMode` 枚举追加 `Free` 值（追加在末尾，序列化 int 向后兼容）：
  - 挂点流程不变（按 F 吸附最近 interactionPoint）。
  - 挂住后人的位置仍由 `KeepActorsAtPoints` 钉在挂点上（挂点是箱子子物体，人随箱刚体平移、相对位置恒定），不引入轨道/姿态切换。
  - 箱子沿相机相对输入向量全向移动；W/S 推拉动画沿用现有按键切换逻辑，`FormalPlayerControl` 零改动。
  - 死区、requiredPushers 校验、`ProbeBlocked` 阻挡探测流程与现有模式一致。
- 删除 `travelLimit` 与 `engageOrigin` 字段及 FixedUpdate 中的轴向限位检查块（收尾 `create-crate-mechanics-test-scene` 中标记完成但代码仍残留的清理项）。
- 修复箱体预制体的双组件冲突（实施期发现）：`L01_MovableStep_WoodenCrate.prefab` 与 `L01_Content.prefab` 上启用的 `FormalCooperativeRailMover` 会在 F 键竞争中抢占 `FormalPushableCrate`，导致 Free 模式永不生效——接通推箱挂点引用并禁用这两个预制体上的轨道机关；`L045_Content.prefab` 通过实例覆盖重新启用轨道机关，第4.5关固定轴行为保持不变。
- `Auto` / `PlusX` / `MinusX` / `PlusZ` / `MinusZ` 固定轴路径行为完全不变，柜子等单向机关继续可用。
- 场景/预制体的 axisMode 切换不在本变更范围内，由用户在 Inspector 手动完成。

## Capabilities

### New Capabilities

- `crate-push-movement`: 定义 FormalPushableCrate 的推动行为契约——固定轴模式与自由（Free）模式的移动规则、挂点期间人与箱子的相对位置约束、阻挡判定与推拉动画触发。

### Modified Capabilities

<!-- 无：openspec/specs 下暂无被本变更修改需求的既有能力 -->

## Impact

- 仅修改单文件：`UnityProject/Assets/MoMing/Scripts/Level01/FormalPushableCrate.cs`。
- 不改接口：`IFormalPushMover`、`FormalPlayerControl`、`FormalCrateDoorTrigger`、狗协作逻辑均不动。
- 预制体反序列化兼容：删除 `travelLimit` 序列化字段后 Unity 自动忽略陈旧数据；用户后续切预制体时自然清除。
