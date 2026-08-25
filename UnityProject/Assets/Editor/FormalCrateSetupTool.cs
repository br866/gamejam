using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// 把场景里一个普通的方块美术改造成「可推箱」。
///
/// 用法：在 Hierarchy 里选中那个正方体，点
/// Tools / SuperBreadMan / 可推箱 / 把选中的物体改造成可推箱。
///
/// 它会补齐 FormalPushableCrate 需要的一整套东西：
///   - BoxCollider（没有就按网格自动贴合）
///   - Rigidbody（参数由 FormalPushableCrate.Awake 接管，这里只保证存在）
///   - FormalPushableCrate 组件
///   - 四个挂点 PushPoint_+X / -X / +Z / -Z，摆在箱子四个面外侧的地面高度上，
///     并填进 Interaction Points 数组（玩家走到挂点附近按 F 才挂得上）
///   - 清掉 Batching Static —— 静态合批的物体打包后动不了
///
/// 再点一次是幂等的：已经有的组件不会重复加，挂点会按当前箱体尺寸重新摆一遍。
/// 只标脏不存盘，确认之后自己 Ctrl+S。
/// </summary>
public static class FormalCrateSetupTool
{
    /// <summary>挂点离箱体表面留多远（米）。太近人会被箱子挤开，太远按 F 够不着。</summary>
    private const float PointPadding = 0.6f;

    [MenuItem("Tools/SuperBreadMan/可推箱/把选中的物体改造成可推箱")]
    public static void ConvertSelectionToCrate()
    {
        GameObject target = Selection.activeGameObject;
        if (target == null || !target.scene.IsValid())
        {
            Debug.LogError("[可推箱] 先在 Hierarchy 里选中场景中的那个方块（不是 Project 窗口里的资源）。");
            return;
        }

        Undo.RegisterFullObjectHierarchyUndo(target, "Convert To Pushable Crate");

        // 静态合批会把变换烘进合并网格，打包之后箱子就推不动了
        StaticEditorFlags flags = GameObjectUtility.GetStaticEditorFlags(target);
        if (flags != 0)
        {
            GameObjectUtility.SetStaticEditorFlags(target, 0);
            Debug.Log("[可推箱] 已清掉 " + target.name + " 的 Static 勾选（含 Batching Static），否则打包后推不动。", target);
        }

        BoxCollider box = target.GetComponent<BoxCollider>();
        if (box == null)
        {
            box = Undo.AddComponent<BoxCollider>(target);
            Debug.Log("[可推箱] 加了 BoxCollider（按网格自动贴合）。", target);
        }
        box.isTrigger = false;

        if (target.GetComponent<Rigidbody>() == null)
            Undo.AddComponent<Rigidbody>(target);

        FormalPushableCrate crate = target.GetComponent<FormalPushableCrate>();
        if (crate == null)
            crate = Undo.AddComponent<FormalPushableCrate>(target);

        Transform[] points = BuildInteractionPoints(target.transform, box);

        SerializedObject so = new SerializedObject(crate);
        SerializedProperty pointsProp = so.FindProperty("interactionPoints");
        pointsProp.arraySize = points.Length;
        for (int i = 0; i < points.Length; i++)
            pointsProp.GetArrayElementAtIndex(i).objectReferenceValue = points[i];

        // Free = 全向推，玩家可以把箱子推到想要的位置；想改成只能沿一条轴推就在 Inspector 里换
        so.FindProperty("axisMode").enumValueIndex = (int)FormalPushableCrate.PushAxisMode.Free;
        so.FindProperty("requiredPushers").intValue = 1;
        so.ApplyModifiedProperties();

        EditorSceneManager.MarkSceneDirty(target.scene);
        Selection.activeGameObject = target;

        Debug.Log("[可推箱] " + target.name + " 已经是可推箱了。\n" +
                  "玩家走到四个 PushPoint 任意一个附近按 F 挂上，再用 WASD 推。\n" +
                  "同场景里的 FormalCrateDoorTrigger 会在箱子进入它的触发区时开门 —— " +
                  "记得确认那个触发区摆在你希望「推到这儿就开门」的位置上。\n" +
                  "确认之后 Ctrl+S 保存场景。", target);

        WarnAboutCrateDoorTrigger(target);
    }

    /// <summary>在箱体四个面外侧建挂点，已经有的就挪位置，不重复建。</summary>
    static Transform[] BuildInteractionPoints(Transform crate, BoxCollider box)
    {
        Vector3 scale = crate.lossyScale;
        float halfX = box.size.x * 0.5f;
        float halfZ = box.size.z * 0.5f;
        float bottomY = box.center.y - box.size.y * 0.5f;

        // 挂点是箱子的子物体，位置写在局部空间里；padding 是米，要按缩放折算
        float padX = Mathf.Abs(scale.x) > 0.0001f ? PointPadding / Mathf.Abs(scale.x) : PointPadding;
        float padZ = Mathf.Abs(scale.z) > 0.0001f ? PointPadding / Mathf.Abs(scale.z) : PointPadding;

        (string, Vector3)[] layout =
        {
            ("PushPoint_+X", new Vector3(box.center.x + halfX + padX, bottomY, box.center.z)),
            ("PushPoint_-X", new Vector3(box.center.x - halfX - padX, bottomY, box.center.z)),
            ("PushPoint_+Z", new Vector3(box.center.x, bottomY, box.center.z + halfZ + padZ)),
            ("PushPoint_-Z", new Vector3(box.center.x, bottomY, box.center.z - halfZ - padZ)),
        };

        Transform[] result = new Transform[layout.Length];
        for (int i = 0; i < layout.Length; i++)
        {
            Transform point = crate.Find(layout[i].Item1);
            if (point == null)
            {
                GameObject created = new GameObject(layout[i].Item1);
                created.transform.SetParent(crate, false);
                Undo.RegisterCreatedObjectUndo(created, "Create Crate Push Point");
                point = created.transform;
            }

            point.localPosition = layout[i].Item2;
            point.localRotation = Quaternion.identity;
            point.localScale = Vector3.one;
            result[i] = point;
        }

        return result;
    }

    static void WarnAboutCrateDoorTrigger(GameObject crate)
    {
        FormalCrateDoorTrigger[] triggers = Object.FindObjectsOfType<FormalCrateDoorTrigger>(true);
        if (triggers.Length == 0)
        {
            Debug.LogWarning("[可推箱] 这个场景里没有 FormalCrateDoorTrigger —— 箱子推到哪儿都不会开门。", crate);
            return;
        }

        foreach (FormalCrateDoorTrigger trigger in triggers)
        {
            SerializedObject so = new SerializedObject(trigger);
            Object door = so.FindProperty("door").objectReferenceValue;
            Collider col = trigger.GetComponent<Collider>();
            Debug.Log("[可推箱] 找到开门触发区 " + trigger.name +
                      "，位置 " + trigger.transform.position.ToString("F2") +
                      "，连的门 = " + (door != null ? door.name : "【没连门！按 F 推过去也不会开】") +
                      "，触发盒 = " + (col != null ? col.bounds.size.ToString("F2") : "【没有 Collider】"),
                      trigger);
        }
    }
}
