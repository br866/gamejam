# formal-pedal-press-visual Delta

## Purpose

为 Formal 关卡踏板定义按钮框/按钮帽的视觉结构与按压状态动画契约：踩下后按钮帽沉入框内表示机关已触发，非永久踏板在关卡重置后弹回复位；未配置视觉的触发器行为保持不变。

## ADDED Requirements

### Requirement: 踏板按压视觉配置为可选
`FormalActuatorTrigger` 的按压视觉 SHALL 通过可选的按钮帽 Transform 引用启用；该引用为空时，触发器的判定、复位、联动行为 MUST 与改动前完全一致。

#### Scenario: 未配置视觉的触发器不受影响
- **WHEN** 一个 `FormalActuatorTrigger`（如怪物安全区、柜子推动目标）未设置按钮帽引用
- **THEN** 其触发、完成、重置行为与无任何视觉代码时一致，且不产生任何子物体或渲染变化

### Requirement: 踩下后按钮帽下沉
当触发器进入完成状态时，已配置的按钮帽 SHALL 从浮起位平滑移动到沉底位（浮起位下方 `pressDepth` 处），过渡速度由 `pressSpeed` 控制。

#### Scenario: 玩家踩上踏板
- **WHEN** 触发条件满足且触发器进入完成状态
- **THEN** 按钮帽在数帧内平滑下沉至沉底位并停在那里

### Requirement: 完成状态期间按钮帽保持沉底
只要触发器处于完成状态，按钮帽 SHALL 保持沉底位，即使玩家离开触发区域。

#### Scenario: 玩家踩下后走开
- **WHEN** 触发器已完成且所有占用者离开触发区域（permanent=1 或尚未发生重置）
- **THEN** 按钮帽保持沉底不下弹

### Requirement: 非永久踏板在关卡重置时弹回
非永久（permanent=0）触发器经历关卡重置、完成状态被清零后，按钮帽 SHALL 平滑回到浮起位。

#### Scenario: 死亡/重开导致关卡重置
- **WHEN** 一个 permanent=0 且已完成、按钮帽处于沉底位的踏板经历关卡重置
- **THEN** 其完成状态清零，按钮帽平滑浮回浮起位，可再次触发

#### Scenario: 永久踏板不因重置弹回
- **WHEN** 一个 permanent=1 且已完成的踏板经历关卡重置
- **THEN** 其完成状态与按钮帽位置均保持不变

### Requirement: 踏板预制件具备框与帽结构
`Pedal.prefab` SHALL 包含静态的按钮框子物体与可动的按钮帽子物体；按钮帽 MUST 不携带静态标记，按钮框 MUST 不带碰撞体以免干扰触发检测。

#### Scenario: 校验测试通过
- **WHEN** 运行 Formal 遍历验证测试套件
- **THEN** 断言确认 prefab 含 Frame 与 ButtonCap 子物体、ButtonCap 无静态标记、Frame 上无 Collider、按钮帽引用已接线

### Requirement: 场景裸触发器踏板替换为完整预制件实例
FormalLevel045 与 FormalLevel05 中的裸触发器踏板 SHALL 替换为扩建后的 `Pedal.prefab` 实例，且原触发器配置（requirement=BothPlayers、permanent 数值、actuators 联动、successorScene）MUST 逐一保留。

#### Scenario: 替换后关卡流程不变
- **WHEN** 在替换后的 FormalLevel045 中双人踩上合作踏板
- **THEN** 原有联动（actuators 开门、successorScene 进入 FormalLevel05）照常生效，同时按钮帽出现下沉动画
