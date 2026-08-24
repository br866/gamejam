# Design: add-formal-level04-monster2-attack

## Context

- FormalLevel04 的 MonsterA/MonsterB 是同一 Quad 基础预制体（guid `b45a1db8...`）的实例：Mesh 换成内置 Capsule（fileID 10208）、Renderer 关闭、Layer 7（NavStatic），以 AddedComponents 挂 `MonsterAnimatorDriver`/`LevelMonsterNavigation`/`AudioSource`/`MonsterPatrol`，以 AddedGameObjects 挂模型预制体实例（scale 2.688、y ≈ -3.1、Animator 覆盖对应控制器、根运动关闭）。
- 路点结构为"空容器 + 5 个子 Transform"，容器挂在 `L04GameplayRoot` 下，`MonsterPatrol.waypoints` 直接引用 5 个子 Transform。
- 现有击杀链路：`MonsterPatrol.Update → CheckForPlayers/TryCatch`，XZ 距离 ≤ `catchRadius`(1.2) 时调 `GameManager.OnPlayerCaught()`；L045 场景特判走 `FormalGameFlowController.ResetCurrentLevel()`。`BeginForcedChase`（由 `FormalGameFlowController` 触发）同样直接 `TryCatch`。
- `MonsterAnimatorDriver.LateUpdate` 每帧按移动速度 CrossFade 到 idle/walk/run——任何"播一个动作再回状态机"的需求都需要锁，否则攻击动画一帧即被覆盖。
- 各怪物模型与控制器：MonsterB=monster1、MonsterA=monster3（driver 状态名错写成 monster1 前缀，动画一直失效）、L02 怪物=monster3、新 Monster2=monster2。各控制器均有 find 类状态可作攻击动作。
- 用户决策：攻击动画用 `monster2 find`；全局直接改（不加开关）；攻击开始后怪物与被锁定目标均不得位移，锁定必中；Monster2 同大房间、新路点由实现者布置。

## Goals / Non-Goals

**Goals:**

- MonsterPatrol 内新增 Attack 状态，统一覆盖普通追击与强制追击两条路径的捕杀入口。
- MonsterAnimatorDriver 提供最小状态锁 API，保证前摇期间动画不被速度判定覆盖。
- 攻击前摇期间仅冻结被锁定目标的移动与交互，并在全部退出路径恢复该目标。
- Monster2 场景组装完全复用 A/B 的实例化模式，不新建脚本、不新建预制体资产。
- 修正 MonsterA driver 状态名错配。

**Non-Goals:**

- 不做攻击开始后的可躲避判定（用户已选锁定必中）、不加行为开关（用户已选全局生效）。
- 不引入新的攻击音效、粒子、UI 表现；`catchClip/detectClip` 维持现状。
- 不改动安全区的进入判定本身、不改动 A* 寻路与房间边界数值。
- 不重构 TryCatch 的重生分发逻辑（GameManager / L045 特判保持原样，仅改变调用时机）。

## Decisions

### D1. Attack 状态内嵌在 MonsterPatrol，而不是独立组件
捕杀判定、寻路控制、目标管理全部集中在 MonsterPatrol，独立组件需要反向侵入 TryCatch 的同步调用链，复杂度更高。方案：
- `State` 增加 `Attack`；新增字段 `attackRange=2f`、`attackWindup=0.5f`、`attackCooldown=1f`、`attackStateName=""`（每只怪在 Inspector 配动画名，留空则只停顿前摇不播动画）。
- Chase 与 ForcedChase 中当 XZ 距离 ≤ attackRange 时进入 Attack：`navigation.ClearDestination()`、面向目标、锁定目标移动与交互、通过驱动器锁播放 `attackStateName`、启动 windup 计时。
- windup 结束 → 复用既有捕杀代码路径（抽出的 `ExecuteCatch()`，含 L045 特判与 catchClip）；随后进入冷却再回 Patrol。
- 安全区仅在攻击开始前阻止目标被锁定；攻击期间目标失效或显式取消 → 解除目标移动锁并取消攻击。关卡重置与对象销毁也必须走同一解锁清理。
- 备选方案（独立 `MonsterAttack` 组件 + 事件回调）：被否——回调会把同步捕杀流程拆散到两个类里，且 ForcedChase 路径还要再接一遍。

### D2. 动画锁放在 MonsterAnimatorDriver 上，用时长锁而非状态查询
新增公开方法 `PlayLockedState(string stateName, float duration)` 与属性 `IsLocked`：锁定期间 LateUpdate 跳过自动状态选择；`ResetPatrol` 时调用解锁。用固定时长（≈windup+余量）而非等动画播完事件——项目内无 AnimationEvent 先例，时长锁最简单且够用。
- 锁内仍走 `ResolveAnimationState` 的 HasState 校验：名字不存在则不切动画但照常锁定计时（降级为纯停顿），避免报错刷屏。
- 备选（driver 直接感知 patrol.IsAttacking）：耦合更紧且无法表达"留空动画名仍锁 idle"的语义，弃。

### D3. Monster2 场景组装完全镜像 A/B 实例
- 根：同 Quad 预制体实例，Mesh→Capsule、Renderer 关闭、Layer 7、改名 `Monster2`，位置约 (-120, 12.94, -15) 一带（避开 A x≈-102 / B x≈-108 的出生区，最终以编辑器目测落位为准）。
- 组件参数：roomCenter/roomSize、detectionRange/fieldOfView/catchRadius/chaseSpeed 等与 A/B 相同；`attackStateName="monster2 find"`。
- 子模型：main monster2.fbx（guid `2e0c76fd...`）实例，命名 `Monster2Model`，scale 2.6882353、y 偏移从 -3.16 起调至脚底贴地，Animator 覆盖为 Monster2 Animator Controller（guid `1b6a5f45...`），根运动关闭。
- Driver 状态名：idle=`monster2 idle1`、walk=`monster2 walk`、run=`monster2 run1`（控制器中实际存在的状态名）。

### D4. 新巡逻线沿大房间西侧走廊布置
大房间 x∈[-197.9,-77.9]、z∈[-36.84,13.36]。A/B 巡逻线集中在中东部（x≈-95~-110），Monster2 的 5 个路点布在西半区（x≈-125~-140 一带，y=12.94 地面高度），形成东西互补的封锁感；具体坐标实现时结合场景几何微调，确保都在导航网格区域内。

### D5. 各场景动画名配置随本变更一并落
L04 MonsterA：顺修 driver 状态名为 `monster3 idle1/walk/run` 并配 `attackStateName="monster3 find"`；MonsterB 配 `monster1 find1`；FormalLevel02 怪物配用户确认的攻击动画 `monster3 drag`。全部是场景序列化字段修改。

### D6. 用正式玩家移动锁冻结处决目标，而非暂停世界
`FormalPlayerControl` 是正式关卡的输入分发入口，`FormalPlayerActor` 持有移动刚体和交互状态。为保证冻结立即生效且可恢复，攻击流程应向被锁定的 `FormalPlayerActor` 请求可重入的临时移动锁：清零水平速度、阻止新的移动输入与互动，并保留重力/镜头/另一名玩家的正常更新。

- MonsterPatrol 必须持有本次锁定的目标，并只在成功获得锁后视为处决已开始。
- `EndAttack`、`CancelAttack`、`ResetPatrol`、目标销毁和捕杀重置路径都必须幂等地释放该锁，避免重生后无法行动。
- 备选的 `Time.timeScale = 0` 被否：它会停止攻击动画、影响相机和另一名玩家；直接禁用玩家组件被否：会遗留刚体速度且难以与交互状态恢复协调。

## Risks / Trade-offs

- [全局改行为影响 Level02/L045 已有体验] → 行为变化方向一致（多了前摇窗口，玩家只会更容易活），无接口破坏；验收时两关各跑一遍确认巡逻/追杀正常。
- [锁定处决可能显得不公平] → 用户明确选择；安全区仍在攻击开始前提供保护，且只冻结被锁定目标而非全体玩家。
- [移动锁未在所有退出路径释放] → 使用幂等释放接口，并对取消、重置、目标销毁和捕杀重置分别验收。
- [attackRange(2m) > catchRadius(1.2m)，若玩家贴脸绕背] → 命中判定不依赖出手瞬间距离（锁定必中），不存在绕背逃逸漏洞。
- [windup 固定时长与动画长度不匹配] → 时长可调字段；后续如需精确对齐可换动画事件，不在本期。
- [场景 YAML 手改风险（PrefabInstance 结构复杂）] → 优先通过 Unity Editor 操作生成序列化数据并保存场景，而非纯文本编辑；改后跑 MeshColliderSyncAuditor/打开场景验证无报错。
- [MonsterA 修复会改变其现有表现（开始有动画）] → 属预期修复，spec 已声明。

## Migration Plan

1. 扩展正式玩家控制/Actor 的临时移动锁，并接入 MonsterPatrol Attack 生命周期，编译通过。
2. 编辑器内为各怪物配置能完整呈现攻击动作的前摇时长，保存场景。
3. Play 验证第二关、第四关和强制追击路径；回滚即 revert 场景与脚本提交，无数据迁移。

## Open Questions

- 无（攻击动画、必中规则、生效范围、路点布置均已由用户决策或授权实现者定夺）。
