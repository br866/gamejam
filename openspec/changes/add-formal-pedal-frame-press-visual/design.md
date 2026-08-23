# Design: add-formal-pedal-frame-press-visual

## Context

`FormalActuatorTrigger` 是通用触发组件，7 处使用中只有 4 处是踏板（`Pedal.prefab`、L045_CooperationPlate、L05 左右 RoomPlate），其余为怪物安全区（L02/L03）与柜子推动目标（L05_CabinetPush）。踏板视觉现状：`Pedal.prefab` 有一个带静态标记的 Pedal1 美术网格子物体；场景中的三个踏板是完全没有视觉的裸触发器（美术在各自 Content prefab 里）。

门侧已有成熟范式：`GateController.Update()` 用 `Vector3.MoveTowards` 让门体每帧向 `isOpen ? openPos : closedPos` 靠拢。触发逻辑上"踩过不能再踩"已存在（`complete` 后 `OnTriggerEnter` 拒绝；permanent=1 时 `ResetTemporaryState` 直接 return），缺的只是视觉表达。

**分工约束**（用户明确要求）：所有 Prefab/场景编辑器操作由用户手动完成——agent 直接改 `.prefab`/`.unity` YAML 过慢且易错。agent 只负责脚本与测试。

## Goals / Non-Goals

**Goals**

- 按钮帽随 `complete` 状态做下沉/浮回动画，模式与 `GateController` 一致
- 视觉配置完全可选：空引用 = 零行为变化，保护共用此组件的安全区/柜子触发器
- 复位弹回复用现有 `ResetTemporaryState` 路径，不新增状态同步机制

**Non-Goals**

- 不实现"离开即弹回"的压力型模式（X 方案被否决前的 Y 方案）
- 不改 requirement 判定、actuator 联动、`opensTransitionDoor` 等任何触发语义
- 不做变色/发光反馈与音效（音效已有）
- 不调整各实例 `permanent` 数值（由用户按关卡意图后续自调）

## Decisions

### D1: 可选字段挂在 `FormalActuatorTrigger` 上，而非新建专用组件

- **选择**: 新增 `[SerializeField] Transform buttonPivot` + `float pressDepth = 0.1f` + `float pressSpeed = 3f`
- **理由**: 字段留空即无副作用，一处代码覆盖全部踏板；和 `FormalDoor` 的"行为+视觉一体"风格一致
- **备选被否**: 新建 `FormalPedalVisual` 组件轮询 `IsComplete` —— 多一个组件、多一层耦合，收益仅是职责分离，对 7 个使用点的项目过重

### D2: 动画用 Update 内 MoveTowards，不用 Animator / 协程

- **选择**: `Update()` 中 `buttonPivot.localPosition = Vector3.MoveTowards(current, target, pressSpeed * dt)`，target.y 由 `IsComplete` 决定
- **理由**: 与 `GateController` 完全同构，无 AnimatorController 配置成本，状态翻转天然正确（含重置弹回）
- **注意**: 用 `localPosition` 而非世界坐标——prefab 子物体有非均匀缩放，父级空间下位移才稳定

### D3: 下沉基准位在 Awake 时采样一次

- **选择**: Awake 记录 `raisedLocalY = buttonPivot.localPosition.y`，沉底位 = `raisedLocalY - pressDepth`
- **理由**: 不假设按钮帽初始摆放在哪个高度，适配任意美术摆放

### D4: 用户手动扩建 prefab + 原位替换裸触发器

- **选择**: agent 出结构清单（见 tasks U 组），用户在编辑器执行；裸触发器替换时逐项抄录原组件配置
- **理由**: YAML 手改风险高（fileID 重排、引用断裂）；编辑器拖拽快且可靠
- **关键清单项**: ButtonCap 必须清除静态标记（现 `m_StaticEditorFlags: 4`，运行时位移会破坏静态合批）；Frame 上不得有 Collider

### D5: 测试断言锚定 prefab 结构而非场景实例

- **选择**: `FormalTraversalValidationTests` 断言 `Pedal.prefab` 含 Frame/ButtonCap、ButtonCap 无静态标记、Frame 无 Collider、`buttonPivot` 已接线；场景实例仅断言存在 prefab 实例来源
- **理由**: 场景替换由人工完成，先锁 prefab 契约；既有测试已有同款 prefab 结构断言先例（trigger-only collider 等）

## Risks / Trade-offs

- [用户替换裸触发器时漏抄配置] → tasks 中给出逐字段核对表（requirement/permanent/actuators/successorScene），C 阶段测试兜底校验
- [pressDepth 过大导致按钮帽穿模进地面] → 默认 0.1，用户可在 Inspector 微调；动画纯视觉不碰碰撞体
- [安全区/柜子触发器误配 buttonPivot] → 字段默认 null 且文档注明仅踏板使用；无自动生成逻辑兜底（白盒方案已被否决）
- [L03_CentralCoopTrigger 归属不明] → 用户目检后决定是否同样处理；不影响本 change 其余任务

## Migration Plan

1. C1 脚本落地 → Unity 编译通过（字段新增向后兼容，旧序列化数据不受影响）
2. 用户执行 U1-U5（扩建 prefab、清静态标记、接线、替换场景触发器）
3. C2/C3：扩展并跑绿 EditMode 测试
4. 回滚策略：脚本改动可整体还原（纯新增字段）；prefab/场景由用户在编辑器撤销

## Open Questions

- Frame 的美术选型（Pedal2 / EmergencyButton / 正式材质组合边框）——用户在编辑器目检后自行定夺，不阻塞代码任务
