# 10 新手 Play Test 教程（从零开始）

> 面向 Unity 新手：教你怎么打开场景、跑起来、用键盘操作、测试提示镜头和音乐。
> 记住一句话：**Play Test = 点上方那个 ▶ 播放键，在编辑器里直接玩。**

---

## 1. 先认识 Unity 界面（几个关键区域）

- **Hierarchy（层级）**：左侧，列出当前场景里所有物体。
- **Scene（场景）视图**：中间，用来搭建/查看场景（不是在玩，是在“布置”）。
- **Game（游戏）视图**：和 Scene 同一块区域切换的标签，**这是玩家真正看到的画面**。
- **Inspector（检视）**：右侧，选中一个物体后，改它的参数就在这里。
- **Project（项目）**：下方，就是 `Assets/` 文件夹，所有文件在这。

最上方中间有三个按钮：**▶（播放）**、⏸（暂停）、⏭（单帧）。

---

## 2. 打开一个场景

在 **Project** 窗口里找到场景文件（`.unity`），**双击**它就会加载。可以先玩：

| 场景 | 路径 | 说明 |
|------|------|------|
| MVP 原型 | `Assets/MoMing/Scenes/Test/Test_MoMing.unity` | 最小可玩，适合第一次上手 |
| 教学关 | `Assets/MoMing/Scenes/Test/Test_Level1.unity` | 推箱→拾取→双人踏板 |
| 白盒测试场景 | `Assets/MoMing/Scenes/Test/superbreadman.unity` | — |
| 美术组搭好的场景 | `Assets/Scenes/Test/superbreadman 1.unity` | 注意在 `Assets/Scenes`，不是 `Assets/MoMing/Scenes` |

> 分不清哪个是完整可玩场景时，看 Hierarchy 里有没有角色和 `GameManager` / `PlayerManager`。

---

## 3. 跑起来：点播放键

1. 双击打开场景。
2. 点最上方 **▶ 播放键**（或 `Ctrl/Cmd + P`）。
3. 视图自动切到 **Game** 标签，就能玩了。
4. 再点一次 ▶ **停止**，回到编辑状态。

> ⚠️ 新手最容易踩的坑：**在“播放中”改的东西，一停止就全没了。** 想永久改，请**先停止**再改。

---

## 4. 操作键位（本游戏）

| 按键 | 作用 | 备注 |
|------|------|------|
| `W A S D` | 移动当前角色 | 方向相对镜头 |
| `Space` | 跳跃 | |
| `Tab` | 切换控制人 / 狗 | 镜头会平滑切到另一个角色 |
| `Q` | 进入/退出联动模式（两人一起动） | 需两人**在一起**才能进 |
| `E` | 拾取 / 丢弃道具 | **仅人** |
| `F` | 触发开关 | **仅人**；联动模式下用来“挂住”箱子推 |
| `左 Shift` | 狗疾跑 | **仅狗**、**分离状态**下有效 |
| 鼠标移动 | 转动镜头 | 默认锁在窗口里 |

> 鼠标被锁住想弄出光标：Play 模式下按 `Esc` 解锁，或停止播放。

---

## 5. 怎么测“音乐系统”

1. 场景里 `GameObject → Create Empty`，命名 `MusicManager`，挂上 `MusicManager` 脚本（见 `08`）。
2. Play，看 **Console**（`Window → General → Console`）：
   - 有音频文件：听到 `bgm_puzzle`，两人分开后自动切过渡/分离/焦虑曲。
   - 还没放音频：Console 打印“暂无音频文件…静默跳过”——**正常**，说明表读到了、只是缺文件。
3. 快速验证自动切歌：Play 后把人和狗**拉开距离**，看 Console 有没有切 Id。

---

## 6. 怎么测“提示镜头”

1. 摆一个提示区（详见 `11-提示镜头接线TODO.md`）：空物体 + Box Collider(勾 Is Trigger) + 挂 `HintTrigger` + 把要“指”的机关拖到 `Focus Target`。
2. 确认角色 Tag = `Player`。
3. Play，控制角色**走进触发区**：镜头应平滑飞到机关、停一下、再飞回；期间只能**走**、不能疾跑。
4. 也可按 Tab 切人/狗，看镜头平滑转场。

看不到效果的排查：角色 Tag 是不是 `Player`？`Focus Target` 拖了吗？触发盒勾 `Is Trigger` 了吗、范围够大吗？

---

## 7. 看报错：Console 窗口

`Window → General → Console`。颜色含义：

- 🔴 **红色 = Error**：会导致功能不工作，**必须处理**。
- 🟡 **黄色 = Warning**：多数能忽略，重点看红色。（在 2022.3 上本项目代码不会产生过时警告。）
- ⚪ 白色 = Log：普通信息，如 `[GameManager] Level reset.`。

Play 前先 Clear 一遍 Console，跑完看有没有新红色，是最快的自检。

---

## 8. 新手常见问题

- **点了 Play 什么都不动 / 角色不受控**：多半缺 `PlayerManager` 或角色没接好。先玩 `Test_MoMing`。
- **角色掉下去了**：地板没接好或有缝，停止后在 Scene 视图检查。
- **改了参数没生效**：确认不是在 Play 模式下改的（会还原）。
- **镜头乱飞 / 走路方向不对**：本作移动相对镜头，先别乱转鼠标，习惯一下。

---

## 9. 上手路线（建议顺序）

1. 双击 `Test_MoMing.unity` → ▶ → 用 WASD / Tab / Q 走一圈找手感。
2. 开 Console，边玩边看 `[PlayerManager] Switched to Dog` 之类日志。
3. 加 `MusicManager`，Play，观察音乐切换。
4. 摆一个 `HintTrigger`，测提示镜头。
5. 都正常后，Console 无红色报错就算通过（全组都在 **2022.3**，你本地验收即可）。
