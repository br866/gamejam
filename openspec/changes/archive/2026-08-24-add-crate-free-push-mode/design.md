# Design: add-crate-free-push-mode

## Context

`FormalPushableCrate`（UnityProject/Assets/MoMing/Scripts/Level01/FormalPushableCrate.cs）现状：
- 挂点（interactionPoints，箱子子物体，每面一个）→ `KeepActorsAtPoints` 每物理帧把人钉到挂点世界坐标 → Auto 模式下推轴 = 人→箱子方向并锁死，W 推 / S 拉。
- `travelLimit` + `engageOrigin` 的轴向限位检查仍残留在 FixedUpdate（create-crate-mechanics-test-scene 曾标记清理完成但代码未删净）。
- 推/拉动画由 `FormalPlayerControl.FixedUpdate` 按 W/S 原始按键切换，与移动轴解耦。
- 阻挡探测 `ProbeBlocked(Vector3)` 本就接受任意方向向量；拉动时会排除挂载角色碰撞体。

## Goals / Non-Goals

**Goals**
- 新增 Free 轴模式：挂点后箱子沿相机相对输入全向移动。
- 人箱相对位置恒定：完全复用 KeepActorsAtPoints（挂点是子物体，人随箱平移）。
- 删除 travelLimit / engageOrigin 及其检查块。

**Non-Goals**
- 不改挂点选择、旋转锁定、狗协作、IFormalPushMover 接口、FormalPlayerControl、门触发器。
- 不改任何场景/预制体数据（axisMode 切换由用户手动完成）。
- 不做轨道模型（人绕箱走位）或姿态检测模型——用户已否决。

## Decisions

1. **枚举追加 `Free` 到末尾**
   - 理由：axisMode 以 int 序列化，末尾追加保证既有场景/预制体反序列化零破坏；插中间会使 PlusX=1..MinusZ=4 整体偏移。
   - 备选（否决）：单独 bool 字段——两个正交维度混在组件上易产生非法组合。

2. **Free 分支复用现有 FixedUpdate 流程骨架**
   - 死区 → requiredPushers 校验 → ProbeBlocked(moveAxis) → 速度赋值，仅 moveAxis 来源不同：
     - 固定轴路径：`axis * inputSign`（不变）。
     - Free 路径：`ResolveWorldInput()`（新抽取的相机相对输入归一化方法）。
   - 理由：最小 diff；阻挡/受阻/动画状态机天然对任意方向生效。

3. **从 ResolveInputDirection 抽取 ResolveWorldInput() 公共方法**
   - 相机相对输入合成（含 CameraFollow/Camera.main 回退）两路共用；固定轴路径继续在其上做 ±0.35 投影判定。
   - 理由：避免相机回退逻辑复制两份。

4. **删除 travelLimit 时连同 engageOrigin 一并删除**
   - engageOrigin 仅被 travelLimit 检查块写入/读取，属同一机制。
   - TryEngage 中 `engageOrigin = transform.position;` 一行随之移除。

5. **修复共享箱体预制体的双组件冲突（实施期发现）**
   - `L01_MovableStep_WoodenCrate.prefab` 与 `L01_Content.prefab` 的箱子同时挂 `FormalPushableCrate` 和启用的 `FormalCooperativeRailMover`，而 `ToggleMoverEngagement` 先遍历轨道机关 → F 键永远被抢占，Free 永不生效；且推箱的 interactionPoints 原本全为 null。
   - 处理：两个预制体接通挂点并禁用轨道机关；`L045_Content.prefab` 以实例覆盖重新启用轨道机关（第4.5关现状即由轨道机关驱动，保持不变）。`CrateMechanicsTest.unity` 场景早已用 m_RemovedComponents 移除该组件佐证此冲突是已知问题。

## Risks / Trade-offs

- [侧移时人横蹭步且播推动画] → 用户已确认为预期直觉表现（"人焊在挂点上"），不做插值。
- [拉向移动时人可能被箱子带向墙体挤压] → 与现版 Auto 拉动语义一致，属继承行为，非本次引入。
- [斜向贴墙手感未知] → 动态刚体物理兜底不穿墙；实测异常再迭代，不在本变更内预设滑动逻辑。
- [狗+Free 组合未专门设计] → 第一关木箱 requiredPushers=1；机制上狗挂点同样随箱平移，够推人数校验照常生效。

## Migration Plan

纯增量代码变更，无部署步骤。回滚 = 还原单文件。预制体陈旧 travelLimit 序列化数据由 Unity 忽略，用户后续编辑预制体时自然清除。

## Open Questions

无。
