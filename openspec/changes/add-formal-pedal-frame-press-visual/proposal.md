# Proposal: add-formal-pedal-frame-press-visual

## Why

Formal 关卡里的踏板（`Pedal.prefab` 及场景中的裸触发器踏板）目前没有任何视觉反馈：踩下后玩家无从得知机关已触发、也无法区分"已锁定永久"与"可复位"两种状态。需要为踏板补上按钮框 + 按钮帽结构，并让按钮帽随触发状态做下沉/弹回动画，与门（`GateController`/`FormalDoor`）的"状态驱动视觉动画"模式对齐。

## What Changes

- `FormalActuatorTrigger.cs` 新增可选视觉字段：`buttonPivot`（要下沉的按钮帽 Transform）、`pressDepth`、`pressSpeed`
- `Update()` 中按 `complete` 状态用 `Vector3.MoveTowards` 驱动 `buttonPivot.localPosition.y` 在浮起位与沉底位之间过渡；`buttonPivot == null` 时行为与现状完全一致
- 复位路径复用现有 `ResetTemporaryState`：非 permanent 触发器在关卡重置清零后按钮自动浮回（X 方案）；permanent=1 则永久保持沉底
- 用户在 Unity 编辑器中手动扩建 `Pedal.prefab`：新增 Frame 子物体（正式美术）、保留现有 Pedal1 网格作为 ButtonCap 并清除其静态标记、接线 `buttonPivot`
- 用户在编辑器中将场景裸触发器替换为扩建后的 prefab 实例并保留原配置：
  - `FormalLevel045` → `L045_CooperationPlate`
  - `FormalLevel05` → `L05_LeftRoomPlate` / `L05_RightRoomPlate`
  - `L03_CentralCoopTrigger` 先目检确认是否为踏板再决定
- 扩展 `Assets/Editor/FormalTraversalValidationTests.cs` 校验 prefab 结构与字段接线

**不做**：不改触发判定逻辑 / 不动老系统 `PressurePlate` / `SequenceGateController` / 不碰音效 / 不改各实例 `permanent` 数值 / 不处理失效的 `L01_Mechanism_Pedal.prefab` 测试引用（既有问题，另立 change）

## Capabilities

### New Capabilities

- `formal-pedal-press-visual`: Formal 踏板的按钮框/按钮帽视觉结构与按压状态动画（踩下沉底、重置弹回），以及未配置视觉时零行为变化的约定

### Modified Capabilities

<!-- 无：现有 spec 均不覆盖 FormalActuatorTrigger 的视觉行为 -->

## Impact

- **代码**：`UnityProject/Assets/MoMing/Scripts/LevelRuntime/FormalActuatorTrigger.cs`（唯一脚本改动）
- **测试**：`UnityProject/Assets/Editor/FormalTraversalValidationTests.cs`（新增断言）
- **资产**：`UnityProject/Assets/MoMing/FormalLevels/Prefabs/Pedal.prefab`（用户手动扩建）
- **场景**：`FormalLevel045.unity`、`FormalLevel05.unity`（用户手动替换裸触发器）
- **不受影响**：`SafeZone` 类触发器（L02/L03）、`L05_CabinetPush`、怪物巡逻逻辑——它们不配置 `buttonPivot`
- **分工**：代码与测试由 agent 完成；所有 Prefab/场景编辑器操作由用户完成（agent 操作场景文件过慢，用户已明确要求）
