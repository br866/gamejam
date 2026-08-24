using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;

/// <summary>
/// 面包人 UI 框架搭建工具（编辑器专用，不参与打包）。
///
/// 用法：在 Unity 里先打开要搭 UI 的场景，然后点顶部菜单 Tools/面包人 UI/。
///   ① 搭建 UI 框架 —— 生成 HUD / PauseMenu / SettingsPanel 并自动接线
///   ② 修复焦虑条   —— 新建一个真正的 Slider 接到 GameManager.anxietyBarSlider
///   ③ 检查接线     —— 在 Console 列出当前场景里还空着的槽位
///
/// 三个菜单都支持 Ctrl+Z 撤销。不满意就撤销，不存盘什么都没发生。
/// 可以重复点，已经存在的对象会复用，不会生成重复的一堆。
/// </summary>
public static class BreadManUIBuilder
{
    const string MENU = "Tools/面包人 UI/";
    const string UNDO = "搭建 UI 框架";

    // ---------- 美术资源路径 ----------
    const string P_PAUSE_PANEL   = "Assets/SuperBreadMan/ui/3暂停界面/pause-menu-empty-panel-transparent (1).png";
    const string P_BTN_CONTINUE  = "Assets/SuperBreadMan/ui/3暂停界面/button_continue_standard.png";
    const string P_BTN_SETTINGS  = "Assets/SuperBreadMan/ui/3暂停界面/button_settings_standard.png";
    const string P_BTN_RESTART   = "Assets/SuperBreadMan/ui/3暂停界面/button_restart_standard.png";
    const string P_BTN_MAINMENU  = "Assets/SuperBreadMan/ui/3暂停界面/button_main_menu_standard.png";
    const string P_HUD_SETTINGS  = "Assets/SuperBreadMan/ui/1游戏开始界面/ui_settings.png";
    const string P_WHITE         = "Assets/MoMing/UI/Tex_White.png";

    const string P_UI_SYSTEM_PREFAB     = "Assets/MoMing/Prefabs/UI_System.prefab";
    const string P_SETTINGS_PANEL_PREFAB = "Assets/MoMing/Prefabs/SettingsPanel.prefab";

    // ---------- 场景流程 ----------
    const string S_START    = "Assets/MoMing/Scenes/Game/Start.unity";
    const string S_CUTSCENE = "Assets/MoMing/Scenes/Game/Cutscene_Intro.unity";
    const string S_LEVEL    = "Assets/Scenes/Test/superbreadman 1.unity";
    const string S_END      = "Assets/MoMing/Scenes/Game/End.unity";
    const string LEVEL_SCENE_NAME = "superbreadman 1";

    // ---------- 配色 ----------
    static readonly Color PANEL_BG = new Color(0f, 0f, 0f, 0.45f);
    static readonly Color DIM_BG   = new Color(0f, 0f, 0f, 0.63f);
    static readonly Color BAR_BG   = new Color(0.10f, 0.10f, 0.11f, 0.85f);
    static readonly Color BAR_FILL = new Color(0.78f, 0.24f, 0.18f, 1f);

    static Font _font;
    static Font UIFont
    {
        get
        {
            if (_font == null) _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (_font == null) _font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            return _font;
        }
    }

    // =====================================================================
    //  ① 搭建 UI 框架
    // =====================================================================
    [MenuItem(MENU + "① 搭建 UI 框架", false, 1)]
    public static void BuildFramework()
    {
        if (GuardPlayMode()) return;
        Undo.IncrementCurrentGroup();
        int group = Undo.GetCurrentGroup();

        Canvas canvas = EnsureCanvas();
        if (canvas == null)
        {
            EditorUtility.DisplayDialog("搭建失败", "场景里没有 Canvas，也找不到 UI_System.prefab。请先把 Assets/MoMing/Prefabs/UI_System.prefab 拖进场景。", "知道了");
            return;
        }
        EnsureEventSystem();

        GameObject canvasGO = canvas.gameObject;

        // --- 两个管理脚本 ---
        var hud = canvasGO.GetComponent<GameHUDManager>();
        if (hud == null) hud = Undo.AddComponent<GameHUDManager>(canvasGO);

        var settingsCtrl = canvasGO.GetComponent<SettingsPanelController>();
        if (settingsCtrl == null) settingsCtrl = Undo.AddComponent<SettingsPanelController>(canvasGO);

        // --- HUD 分组 ---
        RectTransform hudRoot = FindOrCreate(canvas.transform, "HUD");
        Stretch(hudRoot);

        // 左上角：目标
        RectTransform objPanel = FindOrCreate(hudRoot, "ObjectivePanel");
        Anchor(objPanel, new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1), new Vector2(24, -24), new Vector2(360, 108));
        EnsureImage(objPanel, White(), PANEL_BG);

        Text objTitle = EnsureText(objPanel, "ObjectiveTitle", 22, FontStyle.Bold, TextAnchor.UpperLeft, "目标");
        Anchor(objTitle.rectTransform, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1), new Vector2(0, -10), new Vector2(-28, 30));

        Text objContent = EnsureText(objPanel, "ObjectiveContent", 16, FontStyle.Normal, TextAnchor.UpperLeft, "探索当前区域");
        Anchor(objContent.rectTransform, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1), new Vector2(0, -44), new Vector2(-28, 56));
        objContent.horizontalOverflow = HorizontalWrapMode.Wrap;

        // 右上角：设置按钮
        Button settingsBtn = EnsureButton(hudRoot, "SettingsButton", LoadSprite(P_HUD_SETTINGS), "设置");
        Anchor(settingsBtn.GetComponent<RectTransform>(), new Vector2(1, 1), new Vector2(1, 1), new Vector2(1, 1), new Vector2(-24, -24), new Vector2(132, 52));

        // 右下角：操作提示（ControlsHelp 在 UI_System.prefab 里已经存在，但它自己就是一个 Text 且被摆在左上角）
        RectTransform controls = FindInChildren(canvas.transform, "ControlsHelp");
        if (controls == null) controls = FindOrCreate(hudRoot, "ControlsHelp");
        // 故意不把它挪进 HUD：它是 UI_System.prefab 里的物件，
        // 移动 prefab 实例的子物体会被 Unity 拦下来（Cannot restructure Prefab instance）。
        // 只改锚点，这属于允许的属性覆盖。
        Anchor(controls, new Vector2(1, 0), new Vector2(1, 0), new Vector2(1, 0), new Vector2(-24, 24), new Vector2(272, 168));

        // 它自带的那份写死的文字会和新建的两行重影，清空
        var oldText = controls.GetComponent<Text>();
        if (oldText != null && !string.IsNullOrEmpty(oldText.text))
        {
            Undo.RecordObject(oldText, UNDO);
            oldText.text = "";
            EditorUtility.SetDirty(oldText);
        }

        // Text 和 Image 不能挂在同一个物体上，所以底板做成子物体并放到最底层
        RectTransform ctrlBg = FindOrCreate(controls, "Bg");
        Stretch(ctrlBg);
        EnsureImage(ctrlBg, White(), PANEL_BG);
        ctrlBg.SetAsFirstSibling();

        Text commonText = EnsureText(controls, "CommonControls", 16, FontStyle.Bold, TextAnchor.UpperRight, "WASD 移动\nSpace 跳跃");
        Anchor(commonText.rectTransform, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1), new Vector2(0, -12), new Vector2(-24, 52));

        Text specialText = EnsureText(controls, "SpecialControls", 15, FontStyle.Normal, TextAnchor.UpperRight, "E 拾取/丢弃\nF 触发开关\nTab 切换角色\nQ 联动模式");
        Anchor(specialText.rectTransform, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1), new Vector2(0, -70), new Vector2(-24, 88));

        // --- 暂停菜单 ---
        RectTransform pause = FindOrCreate(canvas.transform, "PauseMenu");
        Stretch(pause);

        RectTransform dim = FindOrCreate(pause, "Dim");
        Stretch(dim);
        Image dimImg = EnsureImage(dim, White(), DIM_BG);
        dimImg.raycastTarget = true; // 挡住底下的点击

        Sprite panelSprite = LoadSprite(P_PAUSE_PANEL);
        RectTransform panel = FindOrCreate(pause, "Panel");
        Vector2 panelSize = FitSize(panelSprite, 860f, new Vector2(640, 820));
        Anchor(panel, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, panelSize);
        Image panelImg = EnsureImage(panel, panelSprite != null ? panelSprite : White(), panelSprite != null ? Color.white : PANEL_BG);
        panelImg.preserveAspect = panelSprite != null;

        Button btnContinue = MakePauseButton(panel, "ContinueButton", LoadSprite(P_BTN_CONTINUE), "继续", 0);
        Button btnSettings = MakePauseButton(panel, "PauseSettingsButton", LoadSprite(P_BTN_SETTINGS), "设置", 1);
        Button btnRestart  = MakePauseButton(panel, "RestartButton",  LoadSprite(P_BTN_RESTART),  "重新开始", 2);
        Button btnMainMenu = MakePauseButton(panel, "MainMenuButton", LoadSprite(P_BTN_MAINMENU), "回主菜单", 3);

        // --- 设置面板 ---
        GameObject settingsPanel = EnsureSettingsPanel(canvas.transform);

        // --- 接线 ---
        var so = new SerializedObject(hud);
        WireObj(so, "objectiveTitleText",    objTitle);
        WireObj(so, "objectiveContentText",  objContent);
        WireObj(so, "commonControlsText",    commonText);
        WireObj(so, "specialControlsText",   specialText);
        WireObj(so, "controlsHelpRoot",      controls.gameObject);
        WireObj(so, "settingsButton",        settingsBtn);
        WireObj(so, "pauseMenuRoot",         pause.gameObject);
        WireObj(so, "continueButton",        btnContinue);
        WireObj(so, "settingsButtonInPause", btnSettings);
        WireObj(so, "restartButton",         btnRestart);
        WireObj(so, "returnToMenuButton",    btnMainMenu);
        WireObj(so, "settingsPanel",         settingsPanel);
        so.ApplyModifiedProperties();

        var so2 = new SerializedObject(settingsCtrl);
        WireObj(so2, "settingsPanel", settingsPanel);
        so2.ApplyModifiedProperties();

        // GameManager 的 darknessOverlay 顺手补一下
        var gm = Object.FindObjectOfType<GameManager>();
        if (gm != null && gm.darknessOverlay == null)
        {
            RectTransform overlay = FindInChildren(canvas.transform, "DarknessOverlay");
            if (overlay != null)
            {
                var img = overlay.GetComponent<Image>();
                if (img != null)
                {
                    Undo.RecordObject(gm, UNDO);
                    gm.darknessOverlay = img;
                    EditorUtility.SetDirty(gm);
                }
            }
        }

        // --- 默认隐藏（接线在前，隐藏在后，顺序反了会很难找）---
        Undo.RecordObject(pause.gameObject, UNDO);
        pause.gameObject.SetActive(false);
        if (settingsPanel != null)
        {
            Undo.RecordObject(settingsPanel, UNDO);
            settingsPanel.SetActive(false);
        }

        EditorSceneManager.MarkSceneDirty(canvasGO.scene);
        Undo.CollapseUndoOperations(group);
        Selection.activeGameObject = canvasGO;

        Debug.Log("[面包人UI] 框架搭好了。选中 Canvas 看 GameHUDManager 的槽位是不是都填上了。" +
                  "\n下一步：点 Tools/面包人 UI/② 修复焦虑条，再点 ③ 检查接线。" +
                  "\n不满意直接 Ctrl+Z 撤销。");
    }

    // =====================================================================
    //  ② 修复焦虑条
    // =====================================================================
    [MenuItem(MENU + "② 修复焦虑条（新建 Slider）", false, 2)]
    public static void BuildAnxietySlider()
    {
        if (GuardPlayMode()) return;
        Undo.IncrementCurrentGroup();
        int group = Undo.GetCurrentGroup();

        Canvas canvas = EnsureCanvas();
        if (canvas == null)
        {
            EditorUtility.DisplayDialog("失败", "场景里找不到 Canvas。", "知道了");
            return;
        }

        var gm = Object.FindObjectOfType<GameManager>();
        if (gm == null)
        {
            EditorUtility.DisplayDialog("失败", "场景里找不到 GameManager。", "知道了");
            return;
        }

        RectTransform hudRoot = FindOrCreate(canvas.transform, "HUD");
        Stretch(hudRoot);

        // 旧的 Image 版焦虑条停用，避免和新的重叠
        RectTransform oldBar = FindInChildren(canvas.transform, "AnxietyBarPanel");
        if (oldBar != null && oldBar.gameObject.activeSelf)
        {
            Undo.RecordObject(oldBar.gameObject, UNDO);
            oldBar.gameObject.SetActive(false);
            Debug.Log("[面包人UI] 已停用旧的 AnxietyBarPanel（Image 版），改用新的 Slider。想还原就把它勾回来。");
        }

        RectTransform bar = FindOrCreate(hudRoot, "AnxietyBarSlider");
        Anchor(bar, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 42), new Vector2(400, 24));

        RectTransform bg = FindOrCreate(bar, "Background");
        Stretch(bg);
        EnsureImage(bg, White(), BAR_BG);

        RectTransform fillArea = FindOrCreate(bar, "Fill Area");
        Stretch(fillArea);

        RectTransform fill = FindOrCreate(fillArea, "Fill");
        Stretch(fill);
        EnsureImage(fill, White(), BAR_FILL);

        var slider = bar.GetComponent<Slider>();
        if (slider == null) slider = Undo.AddComponent<Slider>(bar.gameObject);
        Undo.RecordObject(slider, UNDO);
        slider.transition   = Selectable.Transition.None;
        slider.targetGraphic = null;
        slider.handleRect   = null;
        slider.fillRect     = fill;
        slider.direction    = Slider.Direction.LeftToRight;
        slider.minValue     = 0f;
        slider.maxValue     = 1f;
        slider.wholeNumbers = false;
        slider.value        = 0f;
        slider.interactable = false; // 玩家不该能拖动焦虑值
        EditorUtility.SetDirty(slider);

        Undo.RecordObject(gm, UNDO);
        gm.anxietyBarSlider = slider;
        EditorUtility.SetDirty(gm);

        EditorSceneManager.MarkSceneDirty(canvas.gameObject.scene);
        Undo.CollapseUndoOperations(group);
        Selection.activeGameObject = bar.gameObject;

        Debug.Log("[面包人UI] 焦虑条已接到 GameManager.anxietyBarSlider。进 Play 让焦虑值涨起来验证一下。");
    }

    // =====================================================================
    //  ④ 配置场景流程：主菜单 → 过场视频 → 关卡 → 结局
    // =====================================================================
    [MenuItem(MENU + "④ 配置场景流程（主菜单→过场→关卡）", false, 3)]
    public static void SetupSceneFlow()
    {
        if (GuardPlayMode()) return;

        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(S_LEVEL) == null)
        {
            EditorUtility.DisplayDialog("失败", "找不到关卡场景：\n" + S_LEVEL, "知道了");
            return;
        }

        bool created = false;
        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(S_CUTSCENE) == null)
        {
            CreateCutsceneScene(S_CUTSCENE);
            created = true;
        }
        else
        {
            Debug.Log("[面包人UI] 过场场景已存在，没有覆盖它：" + S_CUTSCENE);
        }

        EnsureInBuildSettings(new[] { S_START, S_CUTSCENE, S_LEVEL, S_END });

        string msg = "[面包人UI] 场景流程配好了。\n"
            + (created ? "已新建过场场景：" + S_CUTSCENE + "\n" : "")
            + "Build Settings 顺序：Start → Cutscene_Intro → " + LEVEL_SCENE_NAME + " → End\n"
            + "\n还剩两件事要你手动做（都是几次点击）：\n"
            + "1. 把做好的视频文件拖进 Assets 里（建议放 Assets/MoMing/Video/），\n"
            + "   然后打开 Cutscene_Intro 场景，选中 CutsceneSystem，\n"
            + "   把视频拖进 Video Player 的 Video Clip 槽位。\n"
            + "2. 打开 Start 场景，选中「开始游戏」按钮，在 Inspector 的 On Click 里\n"
            + "   把函数改成 SceneLoader.LoadScene，参数填 Cutscene_Intro。";
        Debug.Log(msg);
        EditorUtility.DisplayDialog("场景流程已配置", msg.Replace("[面包人UI] ", ""), "知道了");
    }

    static void CreateCutsceneScene(string path)
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);

        // --- 相机（纯黑底，视频贴在近裁剪面上）---
        var camGO = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
        camGO.tag = "MainCamera";
        var cam = camGO.GetComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = Color.black;
        SceneManager.MoveGameObjectToScene(camGO, scene);

        // --- 视频 ---
        var sysGO = new GameObject("CutsceneSystem");
        var vp = sysGO.AddComponent<VideoPlayer>();
        vp.playOnAwake      = false;          // 由 CutscenePlayer 控制起播时机
        vp.renderMode       = VideoRenderMode.CameraNearPlane;
        vp.targetCamera     = cam;
        vp.aspectRatio      = VideoAspectRatio.FitInside;   // 比例不对也不会被拉变形
        vp.audioOutputMode  = VideoAudioOutputMode.Direct;
        vp.waitForFirstFrame = true;
        vp.isLooping        = false;
        var player = sysGO.AddComponent<CutscenePlayer>();
        SceneManager.MoveGameObjectToScene(sysGO, scene);

        // --- UI：黑幕 + 跳过提示 ---
        var canvasGO = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        var cv = canvasGO.GetComponent<Canvas>();
        cv.renderMode = RenderMode.ScreenSpaceOverlay;
        cv.sortingOrder = 100;
        var scaler = canvasGO.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        SceneManager.MoveGameObjectToScene(canvasGO, scene);

        var fadeRT = new GameObject("Fade", typeof(RectTransform)).GetComponent<RectTransform>();
        fadeRT.SetParent(canvasGO.transform, false);
        StretchPlain(fadeRT);
        var fadeImg = fadeRT.gameObject.AddComponent<Image>();
        fadeImg.color = Color.black;          // sprite 留空就是一块纯色，够用
        fadeImg.raycastTarget = false;

        var hintRT = new GameObject("SkipHint", typeof(RectTransform)).GetComponent<RectTransform>();
        hintRT.SetParent(canvasGO.transform, false);
        hintRT.anchorMin = hintRT.anchorMax = hintRT.pivot = new Vector2(1, 0);
        hintRT.anchoredPosition = new Vector2(-48, 44);
        hintRT.sizeDelta = new Vector2(260, 40);
        var hint = hintRT.gameObject.AddComponent<Text>();
        hint.font = UIFont;
        hint.fontSize = 20;
        hint.alignment = TextAnchor.LowerRight;
        hint.color = new Color(1f, 1f, 1f, 0.75f);
        hint.text = "按任意键跳过";
        hint.raycastTarget = false;
        hint.horizontalOverflow = HorizontalWrapMode.Overflow;
        hint.verticalOverflow = VerticalWrapMode.Overflow;

        // --- 接线 ---
        player.videoPlayer    = vp;
        player.fadeImage      = fadeImg;
        player.skipHint       = hintRT.gameObject;
        player.nextSceneName  = LEVEL_SCENE_NAME;

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, path);
        EditorSceneManager.CloseScene(scene, true);
        AssetDatabase.Refresh();
    }

    static void EnsureInBuildSettings(string[] paths)
    {
        var list = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        bool changed = false;

        foreach (var p in paths)
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(p) == null)
            {
                Debug.LogWarning("[面包人UI] 找不到场景，跳过：" + p);
                continue;
            }

            bool found = false;
            foreach (var s in list)
            {
                if (s.path != p) continue;
                found = true;
                if (!s.enabled) { s.enabled = true; changed = true; }
                break;
            }

            if (!found)
            {
                list.Add(new EditorBuildSettingsScene(p, true));
                changed = true;
                Debug.Log("[面包人UI] 已加进 Build Settings：" + p);
            }
        }

        if (changed) EditorBuildSettings.scenes = list.ToArray();
    }

    // =====================================================================
    //  ③ 检查接线
    // =====================================================================
    [MenuItem(MENU + "③ 检查接线", false, 20)]
    public static void CheckWiring()
    {
        var report = new List<string>();
        int missing = 0;

        missing += Inspect<GameHUDManager>(report, "objectiveTitleText", "objectiveContentText",
            "commonControlsText", "specialControlsText", "controlsHelpRoot", "settingsButton",
            "pauseMenuRoot", "continueButton", "settingsButtonInPause", "restartButton",
            "returnToMenuButton", "settingsPanel");

        missing += Inspect<SettingsPanelController>(report, "settingsPanel");
        missing += Inspect<SettingsManager>(report, "musicSlider", "sfxSlider", "closeButton");
        missing += Inspect<GameManager>(report, "anxietyBarSlider", "darknessOverlay");

        // 场景层面的检查
        if (FindAnywhere<EventSystem>() == null)
        {
            report.Add("✗ 场景里没有 EventSystem —— UI 按钮一个都点不了。");
            missing++;
        }
        if (FindAnywhere<Canvas>() == null)
        {
            report.Add("✗ 场景里没有 Canvas。");
            missing++;
        }

        var hudMgr = FindAnywhere<GameHUDManager>();
        if (hudMgr != null)
        {
            var soCheck = new SerializedObject(hudMgr);
            var sceneName = soCheck.FindProperty("mainMenuSceneName");
            if (sceneName != null)
            {
                bool inBuild = false;
                foreach (var s in EditorBuildSettings.scenes)
                {
                    if (!s.enabled) continue;
                    if (System.IO.Path.GetFileNameWithoutExtension(s.path) == sceneName.stringValue) { inBuild = true; break; }
                }
                if (!inBuild)
                    report.Add("✗ mainMenuSceneName = \"" + sceneName.stringValue + "\"，但 Build Settings 里没有这个场景 —— 点「回主菜单」会报错。");
                else
                    report.Add("✓ 主菜单场景 \"" + sceneName.stringValue + "\" 已在 Build Settings 里。");
            }
        }

        // 场景流程检查
        foreach (var p in new[] { S_START, S_CUTSCENE, S_LEVEL, S_END })
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(p) == null)
            {
                report.Add("✗ 场景文件不存在：" + p);
                missing++;
                continue;
            }
            bool listed = false;
            foreach (var s in EditorBuildSettings.scenes)
                if (s.path == p && s.enabled) { listed = true; break; }

            if (listed) report.Add("✓ " + System.IO.Path.GetFileNameWithoutExtension(p) + " 已在 Build Settings 里");
            else { report.Add("✗ " + p + " 不在 Build Settings 里 —— 跳不过去。点 ④ 配置场景流程。"); missing++; }
        }

        // 过场视频检查
        var cutscene = AssetDatabase.LoadAssetAtPath<SceneAsset>(S_CUTSCENE);
        if (cutscene != null)
        {
            var cp = FindAnywhere<CutscenePlayer>();
            if (cp != null && cp.videoPlayer != null
                && cp.videoPlayer.clip == null && string.IsNullOrEmpty(cp.videoPlayer.url))
            {
                report.Add("✗ 当前场景的 VideoPlayer 还没放视频 —— 把视频文件拖进 Video Clip 槽位。");
                missing++;
            }
        }

        string head = missing == 0
            ? "[面包人UI] 体检通过，没有空槽位。\n"
            : "[面包人UI] 发现 " + missing + " 处没接上：\n";

        Debug.Log(head + string.Join("\n", report));
    }

    static int Inspect<T>(List<string> report, params string[] fields) where T : Component
    {
        var comp = FindAnywhere<T>();
        if (comp == null)
        {
            report.Add("✗ 场景里没有 " + typeof(T).Name + "。");
            return 1;
        }

        var so = new SerializedObject(comp);
        int missing = 0;
        foreach (var f in fields)
        {
            var p = so.FindProperty(f);
            if (p == null)
            {
                report.Add("? " + typeof(T).Name + "." + f + " —— 字段不存在（脚本改过？）");
                continue;
            }
            if (p.propertyType == SerializedPropertyType.ObjectReference && p.objectReferenceValue == null)
            {
                report.Add("✗ " + typeof(T).Name + "." + f + " 是空的");
                missing++;
            }
        }
        return missing;
    }

    // =====================================================================
    //  工具函数
    // =====================================================================

    /// <summary>
    /// 找场景里的组件，<b>包括挂在隐藏物体上的</b>。
    /// Unity 的 FindObjectOfType 默认跳过 inactive 物体，
    /// 而 SettingsPanel / PauseMenu 本来就该是隐藏的，用默认版本会误报「找不到」。
    /// </summary>
    static T FindAnywhere<T>() where T : Component
    {
        var all = Object.FindObjectsOfType<T>(true);
        return all.Length > 0 ? all[0] : null;
    }

    /// <summary>Play 模式下改场景是白改的（一停止就没了），直接拦住。</summary>
    static bool GuardPlayMode()
    {
        if (!EditorApplication.isPlayingOrWillChangePlaymode) return false;
        EditorUtility.DisplayDialog(
            "请先停止运行",
            "现在是 Play 模式（游戏正在运行）。\n\n"
            + "Play 模式里做的任何改动，一按停止就会全部消失，白做一遍。\n\n"
            + "请先点上方的 ▶ 停止运行，再点这个菜单。",
            "知道了");
        return true;
    }

    /// <summary>给新场景用的 Stretch，不写 Undo（新场景没必要，也不该污染撤销栈）。</summary>
    static void StretchPlain(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        rt.localScale = Vector3.one;
    }

    static Canvas EnsureCanvas()
    {
        var canvas = Object.FindObjectOfType<Canvas>();
        if (canvas != null) return canvas.rootCanvas != null ? canvas.rootCanvas : canvas;

        // 场景里没有，尝试把 UI_System.prefab 拖进来
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(P_UI_SYSTEM_PREFAB);
        if (prefab != null)
        {
            var inst = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            Undo.RegisterCreatedObjectUndo(inst, UNDO);
            Debug.Log("[面包人UI] 场景里没有 UI，已自动拖入 UI_System.prefab。");
            return inst.GetComponentInChildren<Canvas>();
        }
        return null;
    }

    static void EnsureEventSystem()
    {
        if (Object.FindObjectOfType<EventSystem>() != null) return;
        var go = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        Undo.RegisterCreatedObjectUndo(go, UNDO);
        Debug.Log("[面包人UI] 场景里没有 EventSystem，已补上（没有它按钮点不动）。");
    }

    static GameObject EnsureSettingsPanel(Transform canvas)
    {
        RectTransform existing = FindInChildren(canvas, "SettingsPanel");
        if (existing != null) return existing.gameObject;

        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(P_SETTINGS_PANEL_PREFAB);
        if (prefab == null)
        {
            Debug.LogWarning("[面包人UI] 找不到 " + P_SETTINGS_PANEL_PREFAB + "，设置面板这块跳过了。");
            return null;
        }

        var inst = (GameObject)PrefabUtility.InstantiatePrefab(prefab, canvas);
        Undo.RegisterCreatedObjectUndo(inst, UNDO);
        inst.name = "SettingsPanel";
        // 不动它自己的锚点和尺寸 —— prefab 里已经排好版了，改锚点反而会把布局拉坏
        var rt = inst.GetComponent<RectTransform>();
        if (rt != null) rt.localScale = Vector3.one;
        return inst;
    }

    static Button MakePauseButton(RectTransform panel, string name, Sprite sprite, string label, int index)
    {
        Button btn = EnsureButton(panel, name, sprite, label);
        Vector2 size = FitSize(sprite, 92f, new Vector2(320, 84));
        float startY = 168f;
        float spacing = 112f;
        Anchor(btn.GetComponent<RectTransform>(),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0, startY - index * spacing), size);
        return btn;
    }

    static Button EnsureButton(Transform parent, string name, Sprite sprite, string fallbackLabel)
    {
        RectTransform rt = FindOrCreate(parent, name);
        Image img = EnsureImage(rt, sprite != null ? sprite : White(), sprite != null ? Color.white : new Color(1, 1, 1, 0.18f));
        img.preserveAspect = sprite != null;
        img.raycastTarget = true;

        var btn = rt.GetComponent<Button>();
        if (btn == null) btn = Undo.AddComponent<Button>(rt.gameObject);
        Undo.RecordObject(btn, UNDO);
        btn.targetGraphic = img;
        EditorUtility.SetDirty(btn);

        // 图缺失时放一行文字，至少看得见这是个什么按钮
        if (sprite == null && !string.IsNullOrEmpty(fallbackLabel))
        {
            Text t = EnsureText(rt, "Label", 20, FontStyle.Bold, TextAnchor.MiddleCenter, fallbackLabel);
            Stretch(t.rectTransform);
        }
        return btn;
    }

    static Image EnsureImage(RectTransform rt, Sprite sprite, Color color)
    {
        var img = rt.GetComponent<Image>();
        if (img == null) img = Undo.AddComponent<Image>(rt.gameObject);
        Undo.RecordObject(img, UNDO);
        img.sprite = sprite;
        img.color = color;
        img.type = Image.Type.Simple;
        EditorUtility.SetDirty(img);
        return img;
    }

    static Text EnsureText(Transform parent, string name, int size, FontStyle style, TextAnchor align, string content)
    {
        RectTransform rt = FindOrCreate(parent, name);
        var t = rt.GetComponent<Text>();
        if (t == null) t = Undo.AddComponent<Text>(rt.gameObject);
        Undo.RecordObject(t, UNDO);
        t.font = UIFont;
        t.fontSize = size;
        t.fontStyle = style;
        t.alignment = align;
        t.color = Color.white;
        t.raycastTarget = false;
        t.horizontalOverflow = HorizontalWrapMode.Overflow;
        t.verticalOverflow = VerticalWrapMode.Overflow;
        t.supportRichText = true;
        if (string.IsNullOrEmpty(t.text)) t.text = content;
        EditorUtility.SetDirty(t);
        return t;
    }

    static RectTransform FindOrCreate(Transform parent, string name)
    {
        // 同名子物体可能有多个（比如工具被中断后重跑），只留第一个，其余删掉，
        // 否则会看到两份 UI 重叠在一起。
        RectTransform keep = null;
        var dupes = new List<GameObject>();
        for (int i = 0; i < parent.childCount; i++)
        {
            Transform c = parent.GetChild(i);
            if (c.name != name) continue;
            var childRt = c as RectTransform;
            if (childRt == null) continue;
            if (keep == null) keep = childRt;
            else dupes.Add(c.gameObject);
        }
        foreach (var d in dupes)
        {
            Debug.Log("[面包人UI] 删掉重复的 " + name + "（同名的有好几个，只留一个）。");
            Undo.DestroyObjectImmediate(d);
        }
        if (keep != null) return keep;

        var go = new GameObject(name, typeof(RectTransform));
        Undo.RegisterCreatedObjectUndo(go, UNDO);
        var rt = go.GetComponent<RectTransform>();
        rt.SetParent(parent, false);
        rt.localScale = Vector3.one;
        return rt;
    }

    /// <summary>在整棵子树里按名字找（ControlsHelp / DarknessOverlay 这些可能不在 HUD 下面）。</summary>
    static RectTransform FindInChildren(Transform root, string name)
    {
        foreach (var rt in root.GetComponentsInChildren<RectTransform>(true))
            if (rt.name == name) return rt;
        return null;
    }

    static void Anchor(RectTransform rt, Vector2 min, Vector2 max, Vector2 pivot, Vector2 pos, Vector2 size)
    {
        Undo.RecordObject(rt, UNDO);
        rt.anchorMin = min;
        rt.anchorMax = max;
        rt.pivot = pivot;
        rt.sizeDelta = size;
        rt.anchoredPosition = pos;
        rt.localScale = Vector3.one;
        rt.localRotation = Quaternion.identity;
        EditorUtility.SetDirty(rt);
    }

    static void Stretch(RectTransform rt)
    {
        Undo.RecordObject(rt, UNDO);
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        rt.localScale = Vector3.one;
        rt.localRotation = Quaternion.identity;
        EditorUtility.SetDirty(rt);
    }

    /// <summary>按图的原始比例算一个高度不超过 maxHeight 的尺寸；图缺失时用备用尺寸。</summary>
    static Vector2 FitSize(Sprite sprite, float maxHeight, Vector2 fallback)
    {
        if (sprite == null) return fallback;
        float w = sprite.rect.width;
        float h = sprite.rect.height;
        if (h <= 0f) return fallback;
        float scale = Mathf.Min(1f, maxHeight / h);
        return new Vector2(w * scale, h * scale);
    }

    static Sprite White()
    {
        return LoadSprite(P_WHITE);
    }

    /// <summary>加载图；如果它还是 Default 类型（拖不进 Image），顺手改成 Sprite 再重新导入。</summary>
    static Sprite LoadSprite(string path)
    {
        var sp = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        if (sp != null) return sp;

        var importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer == null)
        {
            Debug.LogWarning("[面包人UI] 找不到图：" + path + "（这块会用纯色占位）");
            return null;
        }

        if (importer.textureType != TextureImporterType.Sprite)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.SaveAndReimport();
            Debug.Log("[面包人UI] 已把 " + System.IO.Path.GetFileName(path) + " 的 Texture Type 改成 Sprite (2D and UI)。");
        }

        sp = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        if (sp == null) Debug.LogWarning("[面包人UI] " + path + " 转 Sprite 失败（这块会用纯色占位）");
        return sp;
    }

    static void WireObj(SerializedObject so, string field, Object value)
    {
        var p = so.FindProperty(field);
        if (p == null)
        {
            Debug.LogWarning("[面包人UI] 字段不存在：" + field + "（脚本被改过？）");
            return;
        }
        p.objectReferenceValue = value;
    }
}
