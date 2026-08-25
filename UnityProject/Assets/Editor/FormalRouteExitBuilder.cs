using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 在第五关末尾摆一块「通关触发区」。
///
/// 玩家（或人和狗）走进这块区域 -> FormalRouteAdvanceTrigger 请求推进路线 ->
/// 第五关后面没有下一关了 -> FormalGameFlowController 判定通关 ->
/// 切到结尾动画场景 Cutscene_End -> 动画播完自动回主菜单 Start。
///
/// 用法：打开 FormalLevel05，Scene 视图把镜头对准出口那块地，点菜单。
/// 触发区大小默认 6 x 4 x 6，自己在 Inspector 里改 Box Collider 的 Size。
///
/// 只标脏不自动存盘，摆好之后 Ctrl+S。
/// </summary>
public static class FormalRouteExitBuilder
{
    private const string Level05ScenePath = "Assets/MoMing/FormalLevels/FormalLevel05.unity";
    private const string ExitObjectName = "L05_RouteExitTrigger";

    [MenuItem("Tools/SuperBreadMan/第五关/在当前视角中心放通关触发区")]
    public static void BuildRouteExitTrigger()
    {
        Scene scene = SceneManager.GetSceneByPath(Level05ScenePath);
        if (!scene.isLoaded)
        {
            EditorUtility.DisplayDialog(
                "通关触发区",
                "先打开第五关场景（FormalLevel05），把 Scene 视图对准出口那块地，再点这个菜单。",
                "知道了");
            return;
        }

        GameObject existing = Find(scene, ExitObjectName);
        if (existing != null)
        {
            Undo.RecordObject(existing.transform, "Move Route Exit Trigger");
            PlaceAtSceneViewCenter(existing.transform);
            EditorSceneManager.MarkSceneDirty(scene);
            Selection.activeGameObject = existing;
            Debug.Log("[RouteExit] 第五关已经有 " + ExitObjectName + " 了，把它移到了 " +
                      existing.transform.position.ToString("F2") + "。", existing);
            return;
        }

        GameObject exit = new GameObject(ExitObjectName);
        SceneManager.MoveGameObjectToScene(exit, scene);
        Undo.RegisterCreatedObjectUndo(exit, "Create Route Exit Trigger");
        PlaceAtSceneViewCenter(exit.transform);

        BoxCollider box = Undo.AddComponent<BoxCollider>(exit);
        box.isTrigger = true;
        box.size = new Vector3(6f, 4f, 6f);
        box.center = Vector3.zero;

        Undo.AddComponent<FormalRouteAdvanceTrigger>(exit);

        EditorSceneManager.MarkSceneDirty(scene);
        Selection.activeGameObject = exit;
        EditorGUIUtility.PingObject(exit);

        Debug.Log("[RouteExit] 已在第五关放好 " + ExitObjectName + "（位置 " +
                  exit.transform.position.ToString("F2") + "）。\n" +
                  "玩家走进来就算通关，会切到结尾动画。\n" +
                  "想要求「必须先开门」就在 Inspector 里给 Required Open Door 填门，" +
                  "想要求人和狗都到齐就把 Requirement 改成 Both Players。摆好 Ctrl+S。", exit);
    }

    static void PlaceAtSceneViewCenter(Transform target)
    {
        SceneView view = SceneView.lastActiveSceneView;
        if (view == null)
        {
            target.position = Vector3.zero;
            target.rotation = Quaternion.identity;
            Debug.LogWarning("[RouteExit] 没有活动的 Scene 视图，先摆在原点，自己拖一下。");
            return;
        }

        target.position = view.pivot;
        target.rotation = Quaternion.identity;
    }

    static GameObject Find(Scene scene, string objectName)
    {
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            if (root.name == objectName)
                return root;

            Transform found = FindDeep(root.transform, objectName);
            if (found != null)
                return found.gameObject;
        }

        return null;
    }

    static Transform FindDeep(Transform parent, string objectName)
    {
        foreach (Transform child in parent)
        {
            if (child.name == objectName)
                return child;

            Transform found = FindDeep(child, objectName);
            if (found != null)
                return found;
        }

        return null;
    }
}
