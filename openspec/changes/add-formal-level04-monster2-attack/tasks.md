# Tasks: add-formal-level04-monster2-attack

## 1. 脚本：攻击机制

- [x] 1.1 `MonsterPatrol.cs`：新增 Attack 状态与序列化字段（attackRange=2、attackWindup=0.5、attackCooldown=1、attackStateName），抽出既有捕杀代码为 `ExecuteCatch()` 供攻击结束与原路径复用
- [x] 1.2 Chase/ForcedChase 中距离 ≤ attackRange 时进入 Attack：停寻路、面向目标、锁定被攻击正式玩家的移动与交互、经驱动器锁播放 attackStateName、启动 windup 计时；windup 结束锁定必中调 ExecuteCatch，随后冷却回 Patrol
- [x] 1.3 攻击取消、目标销毁、ResetPatrol 与捕杀重置路径：释放被攻击目标的移动锁，清理攻击计时与动画锁；安全区仅在攻击开始前阻止锁定
- [x] 1.4 `MonsterAnimatorDriver.cs`：新增 `PlayLockedState(string, float)` 与 `IsLocked`，锁内跳过速度自动切状态；状态名不存在时降级为纯停顿不报错
- [x] 1.5 编译通过（read_console 无错误）

## 2. 场景：FormalLevel04 新增 Monster2

- [x] 2.1 编辑器中实例化 Quad 基础预制体于 L04GameplayRoot 下：Mesh→Capsule、Renderer 关闭、Layer NavStatic(7)、改名 Monster2、出生位约 (-120, 12.94, -15) 目测落位
- [x] 2.2 挂组件并配参：MonsterAnimatorDriver（idle=`monster2 idle1` / walk=`monster2 walk` / run=`monster2 run1`）、LevelMonsterNavigation、AudioSource、MonsterPatrol（房间参数同 A/B，attackStateName=`monster2 find`）
- [x] 2.3 子物体挂 main monster2.fbx 实例命名 Monster2Model：scale 2.6882353、y 偏移调至脚底贴地（起点 -3.16）、Animator 覆盖 Monster2 Animator Controller、根运动关闭
- [x] 2.4 新建 Monster2WayPoint 容器 + 5 个子路点（西半区 x≈-125~-140、y=12.94、不与 A/B 路点重叠、均在导航区域内），接入 MonsterPatrol.waypoints
- [x] 2.5 MonsterA driver 状态名修正为 `monster3 idle1/walk/run` 并配 attackStateName=`monster3 find`；MonsterB 配 attackStateName=`monster1 find1`
- [x] 2.6 保存场景，确认无 .meta 遗留问题、无控制台报错

## 3. 场景：FormalLevel02 配置

- [x] 3.1 L02 怪物 MonsterPatrol 配用户确认的 attackStateName=`monster3 drag`，保存场景
- [x] 3.2 为 L02 Monster3 配置足以完整呈现 `monster3 drag` 攻击动作的 attackWindup，保存场景

## 4. 验证

- [ ] 4.1 FormalLevel04 Play：Monster2 结构/动画切换正常，沿新路点循环巡逻不越界；被 A、B 或 Monster2 锁定后，怪物与目标原地播放完整攻击动作，另一名玩家仍可行动，随后目标重生
- [x] 4.2 FormalLevel02 Play：Monster3 巡逻/追击正常，近身锁定 Human 后双方无水平位移、完整播放 `monster3 drag` 再重生；攻击取消或关卡重置后 Human 可立即恢复移动和互动
- [ ] 4.3 强制追击路径验证（L045 或触发点）：同样先冻结被锁定目标、播放攻击前摇再判死
- [ ] 4.4 关卡重置后怪物回巡逻点、无残留攻击状态或目标移动锁；控制台无报错
- [x] 4.5 `openspec validate --all` 通过
