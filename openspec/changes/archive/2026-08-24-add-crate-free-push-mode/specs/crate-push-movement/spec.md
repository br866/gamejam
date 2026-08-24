# crate-push-movement — Delta Spec

## Purpose

定义 FormalPushableCrate（正式关卡可推木箱/柜子）的推动行为契约：固定轴模式与自由（Free）模式的移动规则、挂点期间人与箱子的相对位置约束、阻挡判定、推拉动画触发，以及行程限位机制的移除。

## ADDED Requirements

### Requirement: 自由方向推动模式
推箱组件 SHALL 提供 `Free` 轴模式：挂点后箱子沿相机相对输入向量全向移动（任意水平角度），不再锁定单一推轴。固定轴模式（Auto/PlusX/MinusX/PlusZ/MinusZ）的行为 SHALL 保持不变。

#### Scenario: 挂点后斜向移动
- **WHEN** 玩家按 F 挂住 Free 模式的箱子后输入相机相对的斜向方向
- **THEN** 箱子沿该输入方向匀速移动，无需松手重新挂点

#### Scenario: 固定轴模式不受影响
- **WHEN** 箱子的轴模式为 Auto 或任一固定轴且玩家挂点推动
- **THEN** 移动仍严格沿既有轴向逻辑进行

### Requirement: 挂点期间人箱相对位置恒定
Free 模式下，挂住的玩家 SHALL 始终保持在其初始吸附挂点相对箱子的位置上随箱刚体平移；系统 SHALL NOT 在移动过程中改变玩家绕箱的环绕方位或姿态。

#### Scenario: 全向移动中人跟随箱子
- **WHEN** Free 模式箱子连续转向移动
- **THEN** 玩家与其挂点的世界坐标保持一致，相对箱子的偏移不变

### Requirement: 推拉动画沿用按键切换
挂点状态下推/拉动画 SHALL 继续由 W/S 按键触发（W=推、S=拉），Free 模式不引入新的动画输入约定。

#### Scenario: 侧向移动时按键决定动画
- **WHEN** 玩家在 Free 模式下按住 W 并输入横向分量使箱子侧移
- **THEN** 玩家播放推动画，箱子正常移动

### Requirement: 阻挡判定对任意方向生效
Free 模式下系统 SHALL 对当前实际移动方向执行前方障碍探测；探测到非挂载角色的实体障碍时 SHALL 停止箱子移动并报告受阻状态。

#### Scenario: 斜向撞墙停止
- **WHEN** Free 模式箱子朝墙体方向移动至贴近墙面
- **THEN** 箱子速度归零并进入受阻状态，不穿墙

### Requirement: 行程限位机制移除
推箱组件 SHALL 不再包含 `travelLimit` 序列化字段及其轴向位移限制逻辑；固定轴模式的移动范围 SHALL 仅由场景几何约束。

#### Scenario: 固定轴持续推移无内部限位
- **WHEN** 玩家沿固定轴持续推动箱子且前方无障碍
- **THEN** 箱子不被任何内部行程上限截停
