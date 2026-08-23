# BakedMeshes 去重报告（Phase 1）

日期：2026-08-23
范围：`UnityProject/Assets/MoMing/BakedMeshes/`（1337 个烘焙网格 .asset，212.8 MB）
检查点 commit：`9448cac`（改动前完整快照，可随时 `git checkout 9448cac -- UnityProject/Assets/MoMing` 恢复）

## 方法

1. **内容指纹**：对每个 `.asset` 去掉 `m_Name` 行后做 SHA256 → 完全相同的网格直接归组（Tier1）。
2. **拓扑指纹**：顶点数 + 索引数 + 索引缓冲哈希相同、且 AABB 每轴相对差 < 0.2% 的归为同一分量（Tier2，
   覆盖 `L03_Content_*` 与 `Level03ContentRoot_*` 双份烘焙等浮点噪声级重复）。
3. **并查集传递闭包**：Tier1/Tier2 边统一合并成连通分量，每个分量只保留一个规范资源
   （优先选"被引用最多、名字不以 Level 开头、名字最短"者），所有冗余成员**直接**重定向到根，杜绝链式引用。
4. **引用改写**：393 处 `{fileID: 4300000, guid: 旧, type: 2}` → 规范 GUID，全部位于
   `FormalLevels/Prefabs/L01/L02/L03/L04/L045/L05_Content.prefab`。
5. **删除**：751 个冗余/孤儿资源连同 .meta 删除。

## 结果

| 指标 | 改动前 | 改动后 |
|---|---|---|
| .asset 文件数 | 1337 | **586** |
| 占用体积 | 212.8 MB | **96.2 MB（-116.6 MB）** |
| 引用悬空 | — | 0 |
| Unity 控制台错误/警告 | — | 0 |

- 烘焙均为局部空间（同分量 AABB 一致），重定向**未修改任何 Transform**，渲染结果不变。
- 完整映射表见 `docs/bakedmeshes-dedup-mapping.csv`
  （格式：`动作|旧GUID|原引用文件|新GUID|被删文件名`），分量明细见 `docs/bakedmeshes-dedup-components.log`。

## 遗留问题（改动前即存在，与本次无关）

1. **预置坏引用**：Content Prefab 中存在指向占位符 `guid: 0000000000000000e000...` 的缺失引用
   （L03×4、L04×10、L045×1、L05×5 处；Unity 中表现为 wall5 (34~49)、medicine‑box (2)、metal tray (3)、
   rubbish bin (2) 等 MeshFilter 为空）。HEAD 与当前数量完全一致，系烘焙管线漏生成，需另行修复。
2. **同拓扑不同尺寸的变体未合并**：约 74 组（如不同长度的 pipe/wall5 分段）。若要合并必须同步修改
   场景物体 localScale 做"归一化"，风险较高，本轮未自动处理。

## Phase 2 计划（待执行：Prefab 整合）

把同一分量内各实例改接到**同一个源 Prefab**（复用现有 SharedModels 包装 Prefab，材质/碰撞体随实例
override 迁移），然后删除失去全部引用的包装 Prefab 文件。Instance override 词汇已确认为封闭集合：
Transform / m_Layer / m_Name / m_Mesh / m_Materials[0] / BoxCollider size+center。
