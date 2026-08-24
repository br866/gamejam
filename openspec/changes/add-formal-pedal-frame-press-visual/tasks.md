# Tasks: add-formal-pedal-frame-press-visual

> 分工：C 组（代码/测试）由 agent 执行；U 组（Unity 编辑器操作）由用户手动执行。
> 强依赖顺序：C1 → U1-U5 → C2 → C3。

## 1. Agent：脚本改动（C1）

- [ ] 1.1 `FormalActuatorTrigger.cs` 新增序列化字段 `buttonPivot`（Transform，默认 null）、`pressDepth`（0.1f）、`pressSpeed`（3f），带 Header 注释标明仅踏板使用
- [ ] 1.2 `Awake()` 中当 `buttonPivot != null` 时采样 `raisedLocalY`；`Update()` 中用 `Vector3.MoveTowards` 驱动 `buttonPivot.localPosition.y` 在 `raisedLocalY` 与 `raisedLocalY - pressDepth` 间过渡（目标由 `IsComplete` 决定）
- [ ] 1.3 确认 `buttonPivot == null` 路径零行为变化；确认重置路径无需新代码（`ResetTemporaryState` 清 complete 后动画自动浮回）
- [ ] 1.4 Unity 编译无错误（read_console 校验）

## 2. 用户：Prefab 与场景操作（U1-U5）

- [ ] 2.1 U1: 扩建 `Pedal.prefab`——新增 Frame 子物体（美术自选：Pedal2 / EmergencyButton / 正式材质组合边框），保留现有 Pedal1 网格并更名为 ButtonCap
- [ ] 2.2 U2: 清除 ButtonCap 的静态标记（现 m_StaticEditorFlags=4，否则运行时位移破坏静态合批）
- [ ] 2.3 U3: Inspector 将 ButtonCap 拖入 `FormalActuatorTrigger.buttonPivot` 字段，按需微调 pressDepth/pressSpeed
- [ ] 2.4 U4: FormalLevel045 中将裸触发器 `L045_CooperationPlate` 原位替换为 Pedal.prefab 实例，逐项核对保留配置：requirement=BothPlayers、permanent=0、actuators 重连、successorScene=FormalLevel05
- [ ] 2.5 U4: FormalLevel05 中同样替换 `L05_LeftRoomPlate` 与 `L05_RightRoomPlate`，核对 requirement=BothPlayers、permanent=0 及原有联动
- [ ] 2.6 U5: 目检 `L03_CentralCoopTrigger` 判定是否为踏板；是则按同流程处理，否则记录跳过理由

## 3. Agent：测试扩展（C2）

- [ ] 3.1 扩展 `Assets/Editor/FormalTraversalValidationTests.cs`：断言 Pedal.prefab 含 Frame 与 ButtonCap 子物体
- [ ] 3.2 断言 ButtonCap 无静态标记（staticEditorFlags == 0）、Frame 子物体上无 Collider
- [ ] 3.3 断言 prefab 的 `FormalActuatorTrigger.buttonPivot` 已接线且指向 ButtonCap
- [ ] 3.4 断言 FormalLevel045/FormalLevel05 场景踏板来自 Pedal.prefab 实例且 permanent/requirement 配置与替换前一致

## 4. Agent：验证收尾（C3）

- [ ] 4.1 运行 EditMode 测试套件全绿
- [ ] 4.2 Play 模式抽查：踩下 FormalLevel045 合作踏板 → 按钮帽下沉并保持；关卡重置 → 弹回可再踩
- [ ] 4.3 抽查未配置视觉的触发器（L05_CabinetPush 或 SafeZone）行为无变化
