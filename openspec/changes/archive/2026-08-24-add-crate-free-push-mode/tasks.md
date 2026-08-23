# Tasks: add-crate-free-push-mode

## 1. Implementation（FormalPushableCrate.cs）

- [x] 1.1 在 PushAxisMode 枚举末尾追加 `Free` 值
- [x] 1.2 删除 `travelLimit`、`engageOrigin` 字段、TryEngage 中 engageOrigin 赋值及 FixedUpdate 轴向限位检查块
- [x] 1.3 从 ResolveInputDirection 抽取相机相对输入合成方法 `ResolveWorldInput()`
- [x] 1.4 FixedUpdate 增加 Free 分支：moveAxis = ResolveWorldInput()，复用死区/requiredPushers/ProbeBlocked/速度流程
- [x] 1.5 更新类头 XML 注释，说明 Free 模式的全向移动与人箱相对位置约束
- [x] 1.6 修复 L01_MovableStep_WoodenCrate.prefab 双组件冲突：接通 FormalPushableCrate.interactionPoints（原为 null）、禁用 FormalCooperativeRailMover（原抢占 F 键导致 Free 永不生效）
- [x] 1.7 同步修复 L01_Content.prefab 内置箱子的相同冲突（接点 + 禁用其轨道机关组件）
- [x] 1.8 L045_Content.prefab 增加实例覆盖重新启用轨道机关，保持第4.5关固定轴行为不变
- [x] 1.9 修复禁用语义缺陷：FindObjectsOfType 不过滤组件 enabled，被禁用的轨道机关仍会抢占 F 键——在 FormalCooperativeRailMover 与 FormalPushableCrate 的 TryEngage 入口增加 enabled 守卫
- [x] 1.10 挂点对齐改为玩家子节点 MoverAttachPoint 对齐点位（FormalPlayerActor.SnapToMoverPoint，世界坐标差修正、节点缺失时回退根节点、引用仅首次解析缓存），推箱挂点/持续钉位与轨道机关钉位统一走该接口

## 2. Verification

- [x] 2.1 Unity 编译无错误（read_console 检查）
- [x] 2.2 固定轴/Auto 回归：确认既有路径代码未变且编译通过；L045 经实例覆盖保留轨道机关行为
- [x] 2.3 用户试玩验证通过（四向挂点全量推拉、推拉动画、撞墙受阻合理——W 向管道为真实障碍），[CrateDebug] 临时日志已全部移除
