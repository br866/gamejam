# Proposal: add-formal-level04-monster2-attack

## Why

FormalLevel04 目前只有两只怪物（MonsterA/MonsterB），monster2 美术资产（模型 + 动画控制器）已就绪但未投入使用；同时现有怪物追到玩家身边即瞬杀（`MonsterPatrol.TryCatch` 在 XZ 距离 ≤ catchRadius 时直接 `OnPlayerCaught`），没有攻击动作与前摇，被追杀时玩家没有任何反应窗口，体验生硬。

## What Changes

- **新增怪物**：仿照 MonsterA/MonsterB 的层级结构，把 monster2 资产作为第三只怪物加入 FormalLevel04：
  - 根物体（Quad 预制体实例 → Capsule、Renderer 关闭、Layer NavStatic），挂 `MonsterAnimatorDriver` / `LevelMonsterNavigation` / `AudioSource` / `MonsterPatrol`，子物体挂 monster2 模型并覆盖为 `Monster2 Animator Controller`。
  - 新建 `Monster2WayPoint` 容器与 5 个子路点，在大房间内布一条新巡逻线（与 A/B 同房间边界）。
- **攻击机制改造（全局）**：所有使用 `MonsterPatrol` 的怪物的击杀流程从"接触即杀"改为"先攻击再杀"：
  - 追击中进入攻击距离后，怪物停止位移并面向目标；被锁定的目标玩家也停止水平位移与交互，随后播放攻击动画（前摇 windup）。
  - 前摇结束锁定必中：调用既有捕杀流程（`GameManager.OnPlayerCaught()`，L045 场景走 `FormalGameFlowController.ResetCurrentLevel()`）。
  - 安全区只在攻击开始前提供保护；攻击开始后，目标不可通过位移进入安全区来取消本次处决。攻击取消、关卡重置或目标销毁时必须恢复目标移动。
  - `BeginForcedChase` 强制追杀路径同样走攻击流程；`ResetPatrol` 清理攻击状态与目标移动锁。
- **动画驱动配合**：`MonsterAnimatorDriver` 新增状态锁接口，保证攻击动画在前摇期间不被速度判定切回 idle/walk。
- **各怪物配置攻击动画名**：L04 MonsterA/B → `monster1 find1`；L02 怪物 → `monster3 drag`；新 Monster2 → `monster2 find`。
- **顺手修复**：修正 MonsterA 的 AnimatorDriver 状态名错配（当前写的是 `monster1 *`，但其模型是 Monster3 Controller，应改为 `monster3 idle1/walk/run`）。

## Capabilities

### New Capabilities

- `formal-monster-attack-catch`: 定义 MonsterPatrol 驱动的怪物"先攻击再捕杀"行为契约：攻击触发距离、前摇、目标硬控、锁定必中、攻击开始前的安全区保护、强制追杀路径一致性、状态复位，以及动画驱动器的状态锁要求。
- `formal-level04-monster-roster`: 定义 FormalLevel04 第三只怪物（Monster2）的场景结构契约：与 MonsterA/B 同构的层级与组件配置、monster2 模型/控制器绑定、独立巡逻路点组、房间边界一致。

### Modified Capabilities

<!-- 无：现有 spec 中没有涉及怪物捕杀行为的 capability -->

## Impact

- **脚本**：`Assets/MoMing/Scripts/Enemy/MonsterPatrol.cs`（Attack 状态、目标移动锁生命周期）、`Assets/MoMing/Scripts/Enemy/MonsterAnimatorDriver.cs`（状态锁接口）、正式玩家移动控制/Actor（可恢复的目标移动锁）。全局行为变更影响 Level02 与 L045 的怪物表现。
- **场景**：`Assets/MoMing/FormalLevels/FormalLevel04.unity`（新增 Monster2、Monster2WayPoint 组、A/B 攻击动画名与 MonsterA 状态名修复）、`FormalLevel02.unity`（怪物 attackStateName 配置）。
- **资产引用（只读）**：`main monster2.fbx`、`Monster2 Animator Controller.controller`、Quad 基础预制体（b45a1db8...）。
- **运行时依赖不变**：继续使用 A* Pathfinding（`LevelMonsterNavigation` 自建 GridGraph）与 `GameManager` 重生流程。
