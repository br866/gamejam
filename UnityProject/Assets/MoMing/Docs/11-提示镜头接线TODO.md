# 11 提示镜头 —— 对照 TODO 清单

> 代码部分我已经写好了（见下方「已完成」）。你要做的是**在 Unity 编辑器里接线 + 测试**，照着「你要做的」逐条打勾即可。

---

## A. 代码改动（我已完成，✅ 供你核对）

- [x] `Scripts/Core/CameraFollow.cs`：新增提示镜头 `PlayHint()` / `StopHint()` / `IsHinting`；焦点改用 `SmoothDamp` 平滑；切角色时用 `switchSmoothTime` 做转场；提示期间冻结鼠标转向。
- [x] `Scripts/Core/PlayerManager.cs`：新增 `IsHintPlaying` / `SetHintActive()`；对外暴露 `CameraFollow` 引用。
- [x] `Scripts/Player/PlayerController.cs`：提示镜头播放时强制走路、屏蔽狗疾跑。
- [x] `Scripts/Environment/HintTrigger.cs`：**新增**，走进触发区自动播放提示镜头。
- [x] `Scripts/Puzzle/Checkpoint.cs`：新增可选 `hintTarget`，踩到存档点自动扫一眼下一步。

> 全部协程实现、无 `async/await`、只用 2022.3 原生 API。原有 Tab 切角色 / Q 联动 / 遮挡隐藏 / 光标锁定行为不变。

---

## B. 你要做的（在 Unity 里，逐条打勾）

### 前置检查

- [ ] 打开工程等 Unity 编译完，看 **Console 无红色报错**（黄色警告多数可忽略）。
- [ ] 选中「人」「狗」物体，确认 Inspector 顶部 **Tag = `Player`**（HintTrigger 靠它判断）。

### 做法 A：走进区域自动提示（最常用）

- [ ] `GameObject → Create Empty`，命名如 `Hint_Room1_Switch`。
- [ ] 加 `Box Collider`，勾 **Is Trigger**，把盒子放大到想触发提示的范围。
- [ ] 挂上 `HintTrigger` 脚本（会自动把碰撞体设成 Trigger）。
- [ ] 把要“指”的机关/道具拖到 **Focus Target** 字段。
- [ ] （可选）`Hold Time` 填停留秒数（留 -1 用相机默认 1.5s）；`Play Once` 决定只触发一次还是每次进入都触发。

### 做法 B：踩存档点扫一眼下一步（可选）

- [ ] 选中场景里已有的 Checkpoint 物体。
- [ ] 把 **Hint Target** 拖成下一步的目标对象（不填就不播放）。

### 做法 C：切角色转场（自动，无需接线）

- [ ] Play 后按 **Tab** 切人/狗，确认镜头**平滑过渡**到另一个角色（不是瞬移）。
- [ ] 想更慢更明显：在 `CameraFollow` 上把 **Switch Smooth Time** 调大。

---

## C. 测试验收（Play 模式）

- [ ] 走进 `HintTrigger` 区域 → 镜头**平滑飞到**机关，停一下，再**飞回**角色。
- [ ] 提示播放期间，操作狗按住 Shift **不会疾跑**（只能走路）。
- [ ] 提示播放期间，晃鼠标镜头**不乱转**（yaw 被冻结）。
- [ ] 按 Tab 切角色，镜头有平滑转场。
- [ ] Console 没有红色报错。

---

## D. 可调参数（都在 `CameraFollow` 的 Inspector）

| 字段 | 含义 | 建议值 |
|------|------|--------|
| Focus Smooth Time | 普通跟随平滑（越小越跟手） | 0.03 ~ 0.06 |
| Switch Smooth Time | 切角色转场时长 | 0.3 ~ 0.5 |
| Hint Move Time | 飞向/飞回提示点的时间 | 0.6 ~ 1.0 |
| Hint Hold Time | 默认停留时间 | 1.2 ~ 2.0 |
| Hint Focus Smooth Time | 提示运镜的平滑（越大越像电影） | 0.5 ~ 0.8 |

代码里手动触发（需要时）：

```csharp
cam.PlayHint(targetTransform);      // 停留默认时长
cam.PlayHint(targetTransform, 2f);  // 停留 2 秒
cam.StopHint();                     // 中途打断
```

---

## E. 收尾

- [ ] 一切正常后，确认 Console 无红色报错、能进 Play 即算通过（全组都在 **2022.3**，你本地验收即可）。
