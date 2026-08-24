# formal-level04-monster-roster Specification

## Purpose

定义 FormalLevel04 第三只怪物（Monster2）的场景结构契约：与 MonsterA/MonsterB 完全同构的层级与组件配置、monster2 模型与动画控制器的正确绑定、独立巡逻路点组，以及共享同一房间边界的巡逻行为。

## ADDED Requirements

### Requirement: Monster2 与现有怪物层级同构
FormalLevel04 SHALL 存在名为 Monster2 的怪物根物体，其结构与 MonsterA/MonsterB 同构：碰撞/占位根物体（Capsule 网格、网格渲染器关闭、NavStatic 层）挂载巡逻、寻路、动画驱动与音频组件；子物体挂载 monster2 模型实例。

#### Scenario: 层级结构对照
- **WHEN** 在编辑器中检查 FormalLevel04 的 Monster2
- **THEN** 其根物体与子模型的结构、组件种类和层设置与 MonsterA/MonsterB 一致

### Requirement: monster2 模型与控制器绑定
Monster2 的子模型 SHALL 使用 main monster2.fbx 资产并命名为 Monster2Model，其 Animator 控制器 SHALL 覆盖为 Monster2 Animator Controller 且关闭根运动；动画驱动器的 idle/walk/run 状态名 SHALL 对应该控制器中真实存在的状态（monster2 前缀），保证移动动画正常切换。

#### Scenario: 移动动画正常播放
- **WHEN** Monster2 巡逻或追击时观察其动画
- **THEN** 待机/走/跑状态按速度正常切换，无找不到状态的报错

### Requirement: 独立巡逻路点组
场景 SHALL 新增独立的路点容器与 5 个子路点供 Monster2 巡逻引用；路点 SHALL 位于与 A/B 相同的大房间边界内且不与其他怪物的出生点或路点重叠，形成一条可循环的独立巡逻线。

#### Scenario: 循环巡逻不越界
- **WHEN** 游戏运行且玩家未进入房间时
- **THEN** Monster2 沿自己的 5 个路点循环巡逻，始终保持在房间边界内

### Requirement: 与 A/B 共享房间边界参数
Monster2 的巡逻房间中心/尺寸与导航区域 SHALL 与 MonsterA/B 保持一致，使三只怪物在同一大房间内活动。

#### Scenario: 三只怪物同房间检测
- **WHEN** 玩家进入该大房间的任意位置
- **THEN** 三只怪物均按各自的视野/听觉规则可能发现玩家（Monster2 不因配置差异而检测范围异常）

### Requirement: MonsterA 动画状态名修复
MonsterA 的动画驱动器状态名 SHALL 修正为其模型实际控制器（monster3 前缀）中存在的状态名，使 MonsterA 的待机/走/跑动画恢复切换能力。

#### Scenario: MonsterA 动画恢复切换
- **WHEN** MonsterA 从静止转为移动再停止
- **THEN** 其动画在 idle/walk/run 间正常切换，控制台无状态缺失错误
