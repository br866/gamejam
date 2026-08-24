using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 第三关公告牌的一键摆放工具。
///
/// 用法（两步）：
///   1. 打开 FormalLevel03 场景，在 Scene 视图里把镜头对准想贴牌子的那面墙
///      （牌子会生在视图正中间，并自动转向面对你的镜头）；
///   2. 点菜单 Tools / SuperBreadMan / 第三关 / 创建或刷新公告牌。
///
/// 生成的结构：
///   L03_NoticeBoard   (FormalNoticeBoard，阅读范围球心就在这儿)
///     └ Face          (SpriteRenderer = scence.png，牌面)
///
/// 再跑一次不会重复建，也不会挪动已经摆好的牌子，只重新挂一遍三张图。
/// 想重新定位就用「把公告牌移到当前视角中心」那一条。
/// 跑完只标脏不自动存盘，位置微调好之后自己 Ctrl+S。
/// </summary>
public static class FormalNoticeBoardBuilder
{
    private const string Level03ScenePath = "Assets/MoMing/FormalLevels/FormalLevel03.unity";
    private const string ArtFolder = "Assets/SuperBreadMan/ui/7第三关公告牌/";
    private const string BoardSpritePath = ArtFolder + "scence.png";
    private const string HumanPagePath = ArtFolder + "boy.png";
    private const string DogPagePath = ArtFolder + "dog.png";

    private const string BoardObjectName = "L03_NoticeBoard";

    /// <summary>牌面在世界里的高度（米）。想大想小直接在 Inspector 里改 Face 的 Scale。</summary>
    private const float BoardWorldHeight = 1.6f;

    /// <summary>牌面离墙留一点缝，免得和墙面 z-fighting 闪烁。</summary>
    private const float WallClearance = 0.02f;

    [MenuItem("Tools/SuperBreadMan/第三关/创建或刷新公告牌")]
    public static void BuildNoticeBoard()
    {
        Scene scene;
        if (!TryResolveLevel03(out scene))
            return;

        Sprite boardSprite = Load(BoardSpritePath);
        Sprite humanPage = Load(HumanPagePath);
        Sprite dogPage = Load(DogPagePath);
        if (boardSprite == null || humanPage == null || dogPage == null)
            return;

        GameObject board = Find(scene, BoardObjectName);
        bool created = false;
        if (board == null)
        {
            board = new GameObject(BoardObjectName);
            SceneManager.MoveGameObjectToScene(board, scene);
            Undo.RegisterCreatedObjectUndo(board, "Create Notice Board");
            PlaceAtSceneViewCenter(board.transform);
            created = true;
        }

        FormalNoticeBoard notice = board.GetComponent<FormalNoticeBoard>();
        if (notice == null)
            notice = Undo.AddComponent<FormalNoticeBoard>(board);

        SpriteRenderer renderer = EnsureFace(board.transform);
        Undo.RecordObject(renderer, "Assign Notice Board Sprite");
        renderer.sprite = boardSprite;
        FitFace(renderer.transform, boardSprite);

        SerializedObject serialized = new SerializedObject(notice);
        serialized.FindProperty("humanPage").objectReferenceValue = humanPage;
        serialized.FindProperty("dogPage").objectReferenceValue = dogPage;
        serialized.ApplyModifiedProperties();

        EditorSceneManager.MarkSceneDirty(scene);
        Focus(board);

        Debug.Log("[NoticeBoard] " + (created ? "已新建" : "已刷新") + " " + BoardObjectName +
                  "，位置 " + board.transform.position.ToString("F2") +
                  "。贴着墙微调好之后 Ctrl+S 保存第三关。", board);
    }

    [MenuItem("Tools/SuperBreadMan/第三关/把公告牌移到当前视角中心")]
    public static void MoveNoticeBoardHere()
    {
        Scene scene;
        if (!TryResolveLevel03(out scene))
            return;

        GameObject board = Find(scene, BoardObjectName);
        if (board == null)
        {
            Debug.LogWarning("[NoticeBoard] 第三关里还没有 " + BoardObjectName + "，先跑一次「创建或刷新公告牌」。");
            return;
        }

        Undo.RecordObject(board.transform, "Move Notice Board");
        PlaceAtSceneViewCenter(board.transform);
        EditorSceneManager.MarkSceneDirty(scene);
        Focus(board);

        Debug.Log("[NoticeBoard] 已移动到 " + board.transform.position.ToString("F2") + "。", board);
    }

    /// <summary>第三关没开就先弹个框帮忙开，开完让用户去摆视角，不在同一次点击里硬生。</summary>
    static bool TryResolveLevel03(out Scene scene)
    {
        scene = SceneManager.GetSceneByPath(Level03ScenePath);
        if (scene.isLoaded)
            return true;

        bool open = EditorUtility.DisplayDialog(
            "公告牌",
            "第三关场景（FormalLevel03）还没打开。\n\n" +
            "要现在打开吗？打开之后请先在 Scene 视图里把镜头对准想贴牌子的那面墙，再点一次这个菜单 —— " +
            "牌子会生在视图正中间并自动面向你的镜头。",
            "打开第三关", "取消");

        if (!open)
            return false;

        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            return false;

        EditorSceneManager.OpenScene(Level03ScenePath, OpenSceneMode.Single);
        return false;
    }

    /// <summary>放到 Scene 视图正中间，并绕 Y 轴转到正对镜头 —— 也就是正对你现在看的这面墙。</summary>
    static void PlaceAtSceneViewCenter(Transform target)
    {
        SceneView view = SceneView.lastActiveSceneView;
        if (view == null)
        {
            target.position = Vector3.zero;
            target.rotation = Quaternion.identity;
            Debug.LogWarning("[NoticeBoard] 没有活动的 Scene 视图，牌子先摆在原点，自己拖一下。");
            return;
        }

        target.position = view.pivot;

        Camera camera = view.camera;
        if (camera == null)
        {
            target.rotation = Quaternion.identity;
            return;
        }

        Vector3 towardCamera = camera.transform.position - target.position;
        towardCamera.y = 0f;
        target.rotation = towardCamera.sqrMagnitude > 0.0001f
            ? Quaternion.LookRotation(towardCamera.normalized)
            : Quaternion.identity;
    }

    static SpriteRenderer EnsureFace(Transform board)
    {
        Transform face = board.Find("Face");
        if (face == null)
        {
            GameObject faceObject = new GameObject("Face");
            faceObject.transform.SetParent(board, false);
            Undo.RegisterCreatedObjectUndo(faceObject, "Create Notice Board Face");
            face = faceObject.transform;
        }

        SpriteRenderer renderer = face.GetComponent<SpriteRenderer>();
        return renderer != null ? renderer : Undo.AddComponent<SpriteRenderer>(face.gameObject);
    }

    /// <summary>牌面按目标高度等比缩放，贴在根节点正前方一点点（根节点当作贴墙点）。</summary>
    static void FitFace(Transform face, Sprite sprite)
    {
        float spriteHeight = sprite.bounds.size.y;
        float scale = spriteHeight > 0.0001f ? BoardWorldHeight / spriteHeight : 1f;

        Undo.RecordObject(face, "Fit Notice Board Face");
        face.localScale = new Vector3(scale, scale, scale);
        face.localPosition = new Vector3(0f, 0f, WallClearance);
        face.localRotation = Quaternion.identity;
    }

    static void Focus(GameObject board)
    {
        Selection.activeGameObject = board;
        EditorGUIUtility.PingObject(board);

        SceneView view = SceneView.lastActiveSceneView;
        if (view != null)
            view.Repaint();
    }

    static Sprite Load(string path)
    {
        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        if (sprite == null)
            Debug.LogError("[NoticeBoard] 读不到图：" + path + "。确认这张图的 Texture Type 是 Sprite (2D and UI)。");

        return sprite;
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
