using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 一键在 FormalPersistent 场景里搭出正式关卡的 UI 层（HUD + 暂停菜单 + 设置面板）。
///
/// 为什么用编辑器脚本而不是直接改场景文件：UI 组件（Slider / Button / Canvas）
/// 的序列化结构很啰嗦，手写 YAML 极容易错。让 Unity 自己建更稳。
///
/// 用法：打开 FormalPersistent 场景 -> 菜单 Tools / 默名 / 构建正式关 UI。
/// 可以重复执行，会先把旧的 FormalUI 删掉再重建。
/// </summary>
public static class FormalUIBuilder
{
    const string RootName = "FormalUI";
    const string UiDir = "Assets/SuperBreadMan/ui/";
    const string PauseDir = UiDir + "3暂停界面/";
    const string AnxDir = UiDir + "实机界面ui拆分/1焦虑值/";
    const string TutorialDir = UiDir + "4玩法介绍界面/";
    const string DeathDir = UiDir + "9玩家死亡ui/";
    const string SettingsPrefabPath = "Assets/MoMing/Prefabs/SettingsPanel.prefab";

    [MenuItem("Tools/默名/构建正式关 UI (FormalPersistent)")]
    public static void Build()
    {
        var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        if (scene.name != "FormalPersistent")
        {
            EditorUtility.DisplayDialog("场景不对",
                "请先打开 Assets/MoMing/FormalLevels/FormalPersistent.unity 再执行。\n当前打开的是：" + scene.name,
                "知道了");
            return;
        }

        // 旧的先删掉，保证可重复执行
        var existing = GameObject.Find(RootName);
        if (existing != null)
            Object.DestroyImmediate(existing);

        EnsureEventSystem();
        EnsureAnxietyState();

        var root = new GameObject(RootName, typeof(RectTransform));
        var canvas = root.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        var scaler = root.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        root.AddComponent<GraphicRaycaster>();

        var hud = BuildHud(root.transform, out var hudCtl);
        var pause = BuildPauseMenu(root.transform, out var pauseBtns);
        var settings = BuildSettingsPanel(root.transform);

        var hudController = root.AddComponent<FormalHUDController>();
        hudController.anxietyBar = hudCtl.bar;
        hudController.anxietyFill = hudCtl.fill;
        hudController.anxietyGroup = hudCtl.anxietyGroup;
        hudController.anxietyStateText = hudCtl.stateText;
        hudController.objectiveText = hudCtl.objective;
        hudController.controlsText = hudCtl.controls;
        hudController.hintGroup = hudCtl.hintGroup;
        hudController.hintText = hudCtl.hintText;

        var pauseMenu = root.AddComponent<FormalPauseMenu>();
        pauseMenu.pauseRoot = pause;
        pauseMenu.hudRoot = hud;
        pauseMenu.settingsPanel = settings;
        pauseMenu.continueButton = pauseBtns.continueBtn;
        pauseMenu.settingsButton = pauseBtns.settingsBtn;
        pauseMenu.restartButton = pauseBtns.restartBtn;
        pauseMenu.mainMenuButton = pauseBtns.mainMenuBtn;
        pauseMenu.mainMenuSceneName = "Start";

        BuildDeathScreen(root.transform, root);
        BuildTutorialPopup(root.transform, root);

        // 焦虑的屏幕表现：污渍 + 红晕（挂在 Canvas 上，遮罩由脚本自己建）
        root.AddComponent<FormalAnxietyOverlay>();

        EnsureAnxietyPostFX();

        pause.SetActive(false);
        if (settings != null)
            settings.SetActive(false);

        Selection.activeGameObject = root;
        EditorSceneManager.MarkSceneDirty(scene);
        Debug.Log("[FormalUIBuilder] FormalUI 构建完成。记得 Ctrl+S 保存场景。");
    }

    // ---------------- HUD ----------------

    struct HudRefs
    {
        public Slider bar;
        public Image fill;
        public CanvasGroup anxietyGroup;
        public Text stateText;
        public Text objective;
        public Text controls;
        public CanvasGroup hintGroup;
        public Text hintText;
    }

    static GameObject BuildHud(Transform parent, out HudRefs refs)
    {
        refs = new HudRefs();

        var hud = NewUI("HUD", parent);
        Stretch(Rt(hud));

        // --- 焦虑条（顶部居中）---
        var anx = NewUI("AnxietyGroup", hud.transform);
        Place(Rt(anx), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -20f), new Vector2(900f, 160f));
        refs.anxietyGroup = anx.AddComponent<CanvasGroup>();
        refs.anxietyGroup.interactable = false;
        refs.anxietyGroup.blocksRaycasts = false;

        var title = NewText("Title", anx.transform, "焦虑值", 26, TextAnchor.MiddleCenter,
            new Color(0.88f, 0.84f, 0.74f, 0.9f));
        Place(Rt(title.gameObject), Center, Center, new Vector2(0f, 55f), new Vector2(300f, 40f));

        var childIcon = NewImage("ChildIcon", anx.transform, LoadSprite(AnxDir + "hooded-child-ui-icon.png"), Color.white, false);
        childIcon.preserveAspect = true;
        Place(Rt(childIcon.gameObject), Center, Center, new Vector2(-400f, 0f), new Vector2(84f, 84f));

        var dogIcon = NewImage("DogIcon", anx.transform, LoadSprite(AnxDir + "dog-ui-icon.png"), Color.white, false);
        dogIcon.preserveAspect = true;
        Place(Rt(dogIcon.gameObject), Center, Center, new Vector2(400f, 0f), new Vector2(84f, 84f));

        var barGo = NewUI("AnxietyBar", anx.transform);
        Place(Rt(barGo), Center, Center, new Vector2(0f, 0f), new Vector2(690f, 12f));
        var slider = barGo.AddComponent<Slider>();
        slider.transition = Selectable.Transition.None;
        slider.interactable = false;
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = 0f;
        slider.direction = Slider.Direction.LeftToRight;

        var barBg = NewImage("Background", barGo.transform, null, new Color(0.85f, 0.8f, 0.65f, 0.22f), false);
        Stretch(Rt(barBg.gameObject));

        var fillArea = NewUI("Fill Area", barGo.transform);
        Stretch(Rt(fillArea));
        var fill = NewImage("Fill", fillArea.transform, null, Color.white, false);
        Stretch(Rt(fill.gameObject));
        slider.fillRect = Rt(fill.gameObject);
        slider.targetGraphic = fill;
        refs.bar = slider;
        refs.fill = fill;

        refs.stateText = NewText("StateText", anx.transform, "", 22, TextAnchor.MiddleCenter,
            new Color(0.85f, 0.78f, 0.68f, 0.9f));
        Place(Rt(refs.stateText.gameObject), Center, Center, new Vector2(0f, -48f), new Vector2(400f, 34f));

        // --- 目标（左上）---
        refs.objective = NewText("Objective", hud.transform, "找到出口", 24, TextAnchor.UpperLeft,
            new Color(0.9f, 0.88f, 0.82f, 0.85f));
        Place(Rt(refs.objective.gameObject), new Vector2(0f, 1f), new Vector2(0f, 1f),
            new Vector2(40f, -32f), new Vector2(540f, 90f));

        // --- 操作提示（右下）---
        refs.controls = NewText("Controls", hud.transform, "", 20, TextAnchor.LowerRight,
            new Color(0.85f, 0.83f, 0.78f, 0.7f));
        Place(Rt(refs.controls.gameObject), new Vector2(1f, 0f), new Vector2(1f, 0f),
            new Vector2(-40f, 32f), new Vector2(640f, 100f));

        // --- 居中提示条 ---
        var hint = NewUI("HintGroup", hud.transform);
        Place(Rt(hint), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 170f), new Vector2(1000f, 70f));
        refs.hintGroup = hint.AddComponent<CanvasGroup>();
        refs.hintGroup.interactable = false;
        refs.hintGroup.blocksRaycasts = false;
        refs.hintGroup.alpha = 0f;

        var hintBg = NewImage("Backdrop", hint.transform, null, new Color(0f, 0f, 0f, 0.45f), false);
        Stretch(Rt(hintBg.gameObject));

        refs.hintText = NewText("HintText", hint.transform, "", 26, TextAnchor.MiddleCenter,
            new Color(0.95f, 0.93f, 0.88f, 1f));
        Stretch(Rt(refs.hintText.gameObject));

        return hud;
    }

    // ---------------- 暂停菜单 ----------------

    struct PauseButtons
    {
        public Button continueBtn;
        public Button settingsBtn;
        public Button restartBtn;
        public Button mainMenuBtn;
    }

    static GameObject BuildPauseMenu(Transform parent, out PauseButtons btns)
    {
        btns = new PauseButtons();

        var root = NewUI("PauseMenu", parent);
        Stretch(Rt(root));

        var backdrop = NewImage("Backdrop", root.transform, null, new Color(0f, 0f, 0f, 0.72f), true);
        Stretch(Rt(backdrop.gameObject));

        var panel = NewImage("Panel", root.transform,
            LoadSprite(PauseDir + "pause-menu-empty-panel-transparent (1).png"), Color.white, true);
        panel.preserveAspect = true;
        Place(Rt(panel.gameObject), Center, Center, Vector2.zero, new Vector2(720f, 615f));

        btns.continueBtn = MakeArtButton("BtnContinue", panel.transform,
            PauseDir + "button_continue_standard.png", new Vector2(0f, 100f));
        btns.settingsBtn = MakeArtButton("BtnSettings", panel.transform,
            PauseDir + "button_settings_standard.png", new Vector2(0f, 8f));
        btns.restartBtn = MakeArtButton("BtnRestart", panel.transform,
            PauseDir + "button_restart_standard.png", new Vector2(0f, -84f));
        btns.mainMenuBtn = MakeArtButton("BtnMainMenu", panel.transform,
            PauseDir + "button_main_menu_standard.png", new Vector2(0f, -176f));

        var esc = NewText("EscHint", panel.transform, "Esc 返回", 18, TextAnchor.MiddleCenter,
            new Color(0.35f, 0.31f, 0.26f, 0.85f));
        Place(Rt(esc.gameObject), Center, Center, new Vector2(0f, -250f), new Vector2(300f, 30f));

        return root;
    }

    /// <summary>
    /// 按钮图是浅色金属字，直接放到浅色纸面板上会看不见，
    /// 所以用 Button 的颜色 tint 把它乘暗成墨色，hover 时提亮。
    /// </summary>
    static Button MakeArtButton(string name, Transform parent, string spritePath, Vector2 pos)
    {
        var img = NewImage(name, parent, LoadSprite(spritePath), Color.white, true);
        img.preserveAspect = true;
        Place(Rt(img.gameObject), Center, Center, pos, new Vector2(420f, 88f));

        var btn = img.gameObject.AddComponent<Button>();
        btn.targetGraphic = img;
        btn.transition = Selectable.Transition.ColorTint;

        var colors = btn.colors;
        colors.normalColor = new Color(0.30f, 0.27f, 0.22f, 1f);
        colors.highlightedColor = new Color(0.52f, 0.46f, 0.36f, 1f);
        colors.pressedColor = new Color(0.18f, 0.16f, 0.13f, 1f);
        colors.selectedColor = new Color(0.30f, 0.27f, 0.22f, 1f);
        colors.disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
        colors.fadeDuration = 0.12f;
        btn.colors = colors;

        var nav = btn.navigation;
        nav.mode = Navigation.Mode.None;
        btn.navigation = nav;

        return btn;
    }

    // ---------------- 死亡画面 ----------------

    static void BuildDeathScreen(Transform parent, GameObject controllerHost)
    {
        var death = NewUI("DeathScreen", parent);
        Stretch(Rt(death));
        var group = death.AddComponent<CanvasGroup>();
        group.alpha = 0f;

        var bg = NewImage("Background", death.transform,
            LoadSprite(DeathDir + "death-screen-background-clean.png"), Color.white, true);
        Stretch(Rt(bg.gameObject));

        var title = NewImage("Title", death.transform, null, Color.white, false);
        title.preserveAspect = true;
        Place(Rt(title.gameObject), Center, Center, new Vector2(0f, 200f), new Vector2(880f, 300f));

        var restart = MakeDeathOption("RestartButton", death.transform,
            DeathDir + "restart-selected.png", new Vector2(0f, -20f), new Vector2(380f, 150f));
        var mainMenu = MakeDeathOption("MainMenuButton", death.transform,
            DeathDir + "main-menu.png", new Vector2(0f, -190f), new Vector2(320f, 150f));

        var hint = NewText("Hint", death.transform,
            "\u2191 \u2193 \u9009\u62E9\u3000\u3000Enter \u786E\u8BA4", 20,
            TextAnchor.MiddleCenter, new Color(0.72f, 0.69f, 0.64f, 0.7f));
        Place(Rt(hint.gameObject), Center, Center, new Vector2(0f, -430f), new Vector2(600f, 32f));

        var screen = controllerHost.AddComponent<FormalDeathScreen>();
        screen.root = death;
        screen.group = group;
        screen.titleImage = title;
        screen.caughtTitle = LoadSprite(DeathDir + "death-title.png");
        screen.anxietyTitle = LoadSprite(DeathDir + "anxiety-death-title-you-are-lost.png");
        screen.restartButton = restart;
        screen.mainMenuButton = mainMenu;
        screen.restartImage = restart.targetGraphic as Image;
        screen.mainMenuImage = mainMenu.targetGraphic as Image;
        screen.mainMenuSceneName = "Start";

        death.SetActive(false);
    }

    /// <summary>
    /// 死亡画面的选项。选中状态靠颜色区分（脚本控制），
    /// 所以这里把 Button 的 transition 关掉，免得和脚本抢着改颜色。
    /// </summary>
    static Button MakeDeathOption(string name, Transform parent, string spritePath, Vector2 pos, Vector2 size)
    {
        var img = NewImage(name, parent, LoadSprite(spritePath), Color.white, true);
        img.preserveAspect = true;
        Place(Rt(img.gameObject), Center, Center, pos, size);

        var btn = img.gameObject.AddComponent<Button>();
        btn.targetGraphic = img;
        btn.transition = Selectable.Transition.None;

        var nav = btn.navigation;
        nav.mode = Navigation.Mode.None;
        btn.navigation = nav;

        return btn;
    }

    // ---------------- 开局玩法介绍 ----------------

    static void BuildTutorialPopup(Transform parent, GameObject controllerHost)
    {
        var popup = NewUI("TutorialPopup", parent);
        Stretch(Rt(popup));

        var backdrop = NewImage("Backdrop", popup.transform, null, new Color(0f, 0f, 0f, 0.88f), true);
        Stretch(Rt(backdrop.gameObject));

        var page = NewImage("Page", popup.transform, null, Color.white, false);
        page.preserveAspect = true;
        Place(Rt(page.gameObject), Center, Center, Vector2.zero, new Vector2(1500f, 844f));

        var prev = MakeArrowButton("PrevButton", popup.transform, "\u25C0", new Vector2(-830f, 0f));
        var next = MakeArrowButton("NextButton", popup.transform, "\u25B6", new Vector2(830f, 0f));

        var label = NewText("PageLabel", popup.transform, "1 / 3", 24, TextAnchor.MiddleCenter,
            new Color(0.85f, 0.82f, 0.76f, 0.9f));
        Place(Rt(label.gameObject), Center, Center, new Vector2(0f, -452f), new Vector2(300f, 34f));

        var hint = NewText("Hint", popup.transform, "\u2190 \u2192 \u7FFB\u9875\u3000\u3000ESC \u8DF3\u8FC7", 20,
            TextAnchor.MiddleCenter, new Color(0.75f, 0.72f, 0.67f, 0.75f));
        Place(Rt(hint.gameObject), Center, Center, new Vector2(0f, -492f), new Vector2(600f, 30f));

        var sprites = new Sprite[]
        {
            LoadSprite(TutorialDir + "01basic-controls-ui-original-colors-transparent.png"),
            LoadSprite(TutorialDir + "02role-cooperation-ui-original-colors-transparent.png"),
            LoadSprite(TutorialDir + "03anxiety-ui-original-colors-transparent.png"),
        };
        page.sprite = sprites[0];

        var tutorial = controllerHost.AddComponent<FormalTutorialPopup>();
        tutorial.root = popup;
        tutorial.pageImage = page;
        tutorial.pageLabel = label;
        tutorial.prevButton = prev;
        tutorial.nextButton = next;
        tutorial.pages = sprites;
        tutorial.triggerLevelScene = "FormalLevel01";
        tutorial.rememberAcrossRuns = true;

        popup.SetActive(false);
    }

    static Button MakeArrowButton(string name, Transform parent, string glyph, Vector2 pos)
    {
        var img = NewImage(name, parent, null, new Color(1f, 1f, 1f, 0.06f), true);
        Place(Rt(img.gameObject), Center, Center, pos, new Vector2(100f, 140f));

        var label = NewText("Label", img.transform, glyph, 44, TextAnchor.MiddleCenter, Color.white);
        Stretch(Rt(label.gameObject));

        var btn = img.gameObject.AddComponent<Button>();
        btn.targetGraphic = img;
        btn.transition = Selectable.Transition.ColorTint;

        var colors = btn.colors;
        colors.normalColor = new Color(1f, 1f, 1f, 0.7f);
        colors.highlightedColor = Color.white;
        colors.pressedColor = new Color(0.6f, 0.6f, 0.6f, 1f);
        colors.selectedColor = new Color(1f, 1f, 1f, 0.7f);
        colors.fadeDuration = 0.1f;
        btn.colors = colors;

        var nav = btn.navigation;
        nav.mode = Navigation.Mode.None;
        btn.navigation = nav;

        return btn;
    }

    [MenuItem("Tools/默名/重置玩法介绍（下次再弹一遍）")]
    public static void ResetTutorialFlag()
    {
        PlayerPrefs.DeleteKey(FormalTutorialPopup.PrefKey);
        PlayerPrefs.DeleteKey(FormalTutorialPopup.CheckpointPrefKeyDefault);
        PlayerPrefs.Save();
        Debug.Log("[FormalUIBuilder] 玩法介绍 + 存档地毯介绍已重置，下次会再弹一遍。");
    }

    // ---------------- 设置面板 ----------------

    static GameObject BuildSettingsPanel(Transform parent)
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(SettingsPrefabPath);
        if (prefab == null)
        {
            Debug.LogWarning("[FormalUIBuilder] 找不到 " + SettingsPrefabPath + "，设置面板跳过。");
            return null;
        }

        var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
        instance.name = "SettingsPanel";
        var rt = instance.transform as RectTransform;
        if (rt != null)
            Stretch(rt);
        instance.transform.SetAsLastSibling();
        return instance;
    }

    // ---------------- 场景里缺的东西 ----------------

    static void EnsureEventSystem()
    {
        if (Object.FindObjectOfType<EventSystem>() != null)
            return;

        var go = new GameObject("EventSystem");
        go.AddComponent<EventSystem>();
        go.AddComponent<StandaloneInputModule>();
        Debug.Log("[FormalUIBuilder] 场景里没有 EventSystem，已补上（没有它 UI 点不动）。");
    }

    /// <summary>
    /// 色差（焦虑高时的“眩晕”）。脚本一直躺在项目里没人挂，
    /// 挂到 Global Volume 上即可，它会自己找 Profile 里的 Chromatic Aberration。
    /// </summary>
    static void EnsureAnxietyPostFX()
    {
        if (Object.FindObjectOfType<AnxietyPostFX>() != null)
            return;

        var volume = Object.FindObjectOfType<UnityEngine.Rendering.Volume>();
        if (volume == null)
        {
            Debug.LogWarning("[FormalUIBuilder] 场景里没有 Volume，色差效果跳过。");
            return;
        }

        volume.gameObject.AddComponent<AnxietyPostFX>();
        Debug.Log("[FormalUIBuilder] 已把 AnxietyPostFX 挂到 " + volume.gameObject.name + " 上。");
    }

    static void EnsureAnxietyState()
    {
        if (Object.FindObjectOfType<FormalAnxietyState>() != null)
            return;

        var go = new GameObject("FormalAnxiety");
        go.AddComponent<FormalAnxietyState>();
        Debug.Log("[FormalUIBuilder] 场景里没有 FormalAnxietyState，已补上。" +
                  "不想要“焦虑涨满自动重置关卡”的话，把它的 On Full 改成 None。");
    }

    // ---------------- 小工具 ----------------

    static readonly Vector2 Center = new Vector2(0.5f, 0.5f);

    static RectTransform Rt(GameObject go)
    {
        return (RectTransform)go.transform;
    }

    static GameObject NewUI(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go;
    }

    static void Stretch(RectTransform r)
    {
        r.anchorMin = Vector2.zero;
        r.anchorMax = Vector2.one;
        r.offsetMin = Vector2.zero;
        r.offsetMax = Vector2.zero;
        r.localScale = Vector3.one;
    }

    static void Place(RectTransform r, Vector2 anchor, Vector2 pivot, Vector2 pos, Vector2 size)
    {
        r.anchorMin = anchor;
        r.anchorMax = anchor;
        r.pivot = pivot;
        r.anchoredPosition = pos;
        r.sizeDelta = size;
        r.localScale = Vector3.one;
    }

    static Image NewImage(string name, Transform parent, Sprite sprite, Color color, bool raycast)
    {
        var go = NewUI(name, parent);
        var img = go.AddComponent<Image>();
        img.sprite = sprite;
        img.color = color;
        img.raycastTarget = raycast;
        return img;
    }

    static Text NewText(string name, Transform parent, string content, int size, TextAnchor anchor, Color color)
    {
        var go = NewUI(name, parent);
        var t = go.AddComponent<Text>();
        t.font = BuiltinFont();
        t.text = content;
        t.fontSize = size;
        t.alignment = anchor;
        t.color = color;
        t.raycastTarget = false;
        t.horizontalOverflow = HorizontalWrapMode.Overflow;
        t.verticalOverflow = VerticalWrapMode.Overflow;
        return t;
    }

    /// <summary>2022.2 之后内置 Arial 改名成 LegacyRuntime.ttf，两个都试一下。</summary>
    static Font BuiltinFont()
    {
        Font f = null;
        try { f = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); }
        catch { }
        if (f == null)
        {
            try { f = Resources.GetBuiltinResource<Font>("Arial.ttf"); }
            catch { }
        }
        if (f == null)
            Debug.LogWarning("[FormalUIBuilder] 没拿到内置字体，文字可能不显示。手动给 Text 指定一个字体。");
        return f;
    }

    static Sprite LoadSprite(string path)
    {
        var sp = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        if (sp == null)
            Debug.LogWarning("[FormalUIBuilder] 加载不到图：" + path + "（检查路径，或者这张图的 Texture Type 不是 Sprite）");
        return sp;
    }
}
