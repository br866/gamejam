using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 第四关两只怪的模型对调。
///
/// 现状（工具会在 Console 里再确认一遍）：
///   MonsterA  在 (-102.29, 12.94, 1.32)  用 main monster3.fbx + Monster3 Animator Controller
///   Monster2  在 (-111.41, 12.94, -19.67) 用 main monster2.fbx + Monster2 Animator Controller
///
/// 模型不是单独一层皮：每个模型自带骨骼比例、Animator 控制器、还有攻击动作名
/// （monster3 drag / monster2 drag）。只换网格会让攻击动画对不上，所以这个工具
/// 把「模型 + 局部变换 + Animator 控制器 + 动作状态名」当成一整套一起换。
///
/// 换法不是删了重建，而是「把对方的模型加进来 + 把自己原来的模型停用」：
/// 删除预制体实例里的子物体会被 Unity 拦下来（要先 Unpack），而加子物体、
/// 改 active 都是合法的覆盖。旧模型只是隐藏着，随时能退回去。
///
/// 再点一次就换回来（工具会认出之前隐藏的那个模型，直接重新启用，不会越加越多）。
/// 只标脏不自动存盘，看过效果之后自己 Ctrl+S。
/// </summary>
public static class FormalLevel04MonsterSwap
{
    private const string Level04ScenePath = "Assets/MoMing/FormalLevels/FormalLevel04.unity";

    private class MonsterVisual
    {
        public MonsterPatrol Patrol;
        public GameObject ActiveModel;
        public GameObject SourceAsset;
        public RuntimeAnimatorController Controller;
        public bool ApplyRootMotion;
        public Vector3 LocalPosition;
        public Quaternion LocalRotation;
        public Vector3 LocalScale;
        public int Layer;
        public string ModelName;
    }

    [MenuItem("Tools/SuperBreadMan/第四关/交换两只怪的模型")]
    public static void SwapMonsterModels()
    {
        Scene scene = SceneManager.GetSceneByPath(Level04ScenePath);
        if (!scene.isLoaded)
        {
            EditorUtility.DisplayDialog("交换怪物模型",
                "先打开第四关场景（FormalLevel04），再点这个菜单。", "知道了");
            return;
        }

        List<MonsterPatrol> monsters = new List<MonsterPatrol>();
        foreach (GameObject root in scene.GetRootGameObjects())
            monsters.AddRange(root.GetComponentsInChildren<MonsterPatrol>(true));

        if (monsters.Count != 2)
        {
            Debug.LogError("[怪物换模] 第四关里找到 " + monsters.Count +
                           " 个 MonsterPatrol，期望正好 2 个。先确认场景对不对。");
            return;
        }

        MonsterVisual first = Capture(monsters[0]);
        MonsterVisual second = Capture(monsters[1]);
        if (first == null || second == null)
            return;

        Debug.Log("[怪物换模] 换之前：\n" +
                  "  " + first.Patrol.name + " -> " + first.ModelName + " (" + AssetName(first.SourceAsset) + ")\n" +
                  "  " + second.Patrol.name + " -> " + second.ModelName + " (" + AssetName(second.SourceAsset) + ")");

        Undo.RegisterFullObjectHierarchyUndo(first.Patrol.gameObject, "Swap Monster Models");
        Undo.RegisterFullObjectHierarchyUndo(second.Patrol.gameObject, "Swap Monster Models");

        // 先把两边的旧模型都停掉，再各自装上对方的，免得中间状态互相干扰
        first.ActiveModel.SetActive(false);
        second.ActiveModel.SetActive(false);

        ApplyVisual(first.Patrol, second);
        ApplyVisual(second.Patrol, first);

        SwapAnimationStateNames(first.Patrol, second.Patrol);

        EditorSceneManager.MarkSceneDirty(scene);
        Selection.objects = new Object[] { first.Patrol.gameObject, second.Patrol.gameObject };

        Debug.Log("[怪物换模] 换完了：\n" +
                  "  " + first.Patrol.name + " -> " + AssetName(second.SourceAsset) + "\n" +
                  "  " + second.Patrol.name + " -> " + AssetName(first.SourceAsset) + "\n" +
                  "旧模型没有删除，只是停用了（Hierarchy 里灰掉的那个），再点一次菜单可以换回来。\n" +
                  "确认没问题之后 Ctrl+S 保存第四关。");
    }

    /// <summary>记下这只怪当前显示的那套外观。</summary>
    static MonsterVisual Capture(MonsterPatrol patrol)
    {
        Animator animator = patrol.GetComponentInChildren<Animator>();
        if (animator == null)
        {
            Debug.LogError("[怪物换模] " + patrol.name + " 底下没有启用中的 Animator，找不到模型。", patrol);
            return null;
        }

        GameObject model = animator.gameObject;
        GameObject source = PrefabUtility.GetCorrespondingObjectFromOriginalSource(model);
        if (source == null)
        {
            Debug.LogError("[怪物换模] " + patrol.name + " 的模型 " + model.name +
                           " 不是预制体实例（可能被 Unpack 过），这个工具接不了。", model);
            return null;
        }

        string path = AssetDatabase.GetAssetPath(source);
        GameObject asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (asset == null)
        {
            Debug.LogError("[怪物换模] 读不到模型资源：" + path, model);
            return null;
        }

        return new MonsterVisual
        {
            Patrol = patrol,
            ActiveModel = model,
            SourceAsset = asset,
            Controller = animator.runtimeAnimatorController,
            ApplyRootMotion = animator.applyRootMotion,
            LocalPosition = model.transform.localPosition,
            LocalRotation = model.transform.localRotation,
            LocalScale = model.transform.localScale,
            Layer = model.layer,
            ModelName = model.name,
        };
    }

    /// <summary>把 visual 这套外观装到 patrol 这只怪身上。</summary>
    static void ApplyVisual(MonsterPatrol patrol, MonsterVisual visual)
    {
        GameObject model = FindHiddenModel(patrol.transform, visual.SourceAsset);

        if (model == null)
        {
            model = (GameObject)PrefabUtility.InstantiatePrefab(visual.SourceAsset, patrol.gameObject.scene);
            model.transform.SetParent(patrol.transform, false);
            Undo.RegisterCreatedObjectUndo(model, "Add Monster Model");
        }

        model.name = visual.ModelName;
        model.transform.localPosition = visual.LocalPosition;
        model.transform.localRotation = visual.LocalRotation;
        model.transform.localScale = visual.LocalScale;
        SetLayerRecursive(model.transform, visual.Layer);
        model.SetActive(true);

        Animator animator = model.GetComponent<Animator>();
        if (animator == null)
            animator = model.GetComponentInChildren<Animator>(true);

        if (animator != null)
        {
            animator.runtimeAnimatorController = visual.Controller;
            animator.applyRootMotion = visual.ApplyRootMotion;
        }
        else
        {
            Debug.LogWarning("[怪物换模] " + model.name + " 上没有 Animator，动画接不上。", model);
        }
    }

    /// <summary>找之前被这个工具停用、来源正好是这个模型的子物体，有就复用，避免越换越多。</summary>
    static GameObject FindHiddenModel(Transform monsterRoot, GameObject sourceAsset)
    {
        foreach (Transform child in monsterRoot)
        {
            if (child.gameObject.activeSelf)
                continue;

            GameObject source = PrefabUtility.GetCorrespondingObjectFromOriginalSource(child.gameObject);
            if (source != null && AssetDatabase.GetAssetPath(source) == AssetDatabase.GetAssetPath(sourceAsset))
                return child.gameObject;
        }

        return null;
    }

    /// <summary>
    /// 攻击/待机/行走的状态名是跟着 Animator 控制器走的，模型换了名字也得跟着换，
    /// 不然 CrossFade 找不到状态，动作就不播了。
    /// </summary>
    static void SwapAnimationStateNames(MonsterPatrol a, MonsterPatrol b)
    {
        SwapSerializedString(a, b, "attackStateName");

        MonsterAnimatorDriver driverA = a.GetComponent<MonsterAnimatorDriver>();
        MonsterAnimatorDriver driverB = b.GetComponent<MonsterAnimatorDriver>();
        if (driverA == null || driverB == null)
            return;

        SwapSerializedString(driverA, driverB, "idleState");
        SwapSerializedString(driverA, driverB, "walkState");
        SwapSerializedString(driverA, driverB, "runState");
    }

    static void SwapSerializedString(Object a, Object b, string field)
    {
        SerializedObject soA = new SerializedObject(a);
        SerializedObject soB = new SerializedObject(b);
        SerializedProperty propA = soA.FindProperty(field);
        SerializedProperty propB = soB.FindProperty(field);
        if (propA == null || propB == null)
            return;

        string valueA = propA.stringValue;
        propA.stringValue = propB.stringValue;
        propB.stringValue = valueA;
        soA.ApplyModifiedProperties();
        soB.ApplyModifiedProperties();
    }

    static void SetLayerRecursive(Transform node, int layer)
    {
        node.gameObject.layer = layer;
        foreach (Transform child in node)
            SetLayerRecursive(child, layer);
    }

    static string AssetName(GameObject asset)
    {
        return asset != null ? System.IO.Path.GetFileNameWithoutExtension(AssetDatabase.GetAssetPath(asset)) : "?";
    }
}
