using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 往场景里贴贴纸的工具。第一关、第三关、随便哪一关都能用 —— 它只认「当前打开的场景」。
///
/// 常规用法：
///   1. 打开要摆的那一关，在 Scene 视图里把镜头对准想贴的那面墙 / 那块地；
///   2. 在 Project 窗口里点中一张图；
///   3. 菜单 Tools / SuperBreadMan / 贴纸 / 把选中的图贴到当前视角中心。
///
/// 图会生在视图正中间、自动转向面对你的镜头，然后自己微调位置就行。
/// 图的导入设置不是 Sprite 的话工具会顺手改掉，不用手动去 Inspector 点。
///
/// 生成的结构：
///   Sticker_xxx   (FormalWallSticker，靠近弹字，Hint Message 自己填)
///     └ Face      (SpriteRenderer，图本身)
///
/// 只标脏不自动存盘，摆好之后自己 Ctrl+S。
/// </summary>
public static class FormalStickerBuilder
{
    private const string HintArtFolder = "Assets/SuperBreadMan/关卡提示透明底";

    /// <summary>贴纸默认在世界里的高度（米）。太大太小直接改 Face 的 Scale。</summary>
    private const float StickerWorldHeight = 1.2f;

    /// <summary>贴纸离墙留一点缝，免得和墙面 z-fighting 闪烁。</summary>
    private const float WallClearance = 0.02f;

    [MenuItem("Tools/SuperBreadMan/贴纸/把选中的图贴到当前视角中心 %#j")]
    public static void PlaceSelectedSticker()
    {
        Sprite sprite = ResolveSelectedSprite();
        if (sprite == null)
            return;

        Scene scene = SceneManager.GetActiveScene();
        if (!scene.IsValid() || !scene.isLoaded)
        {
            Debug.LogError("[Sticker] 当前没有打开的场景。");
            return;
        }

        GameObject sticker = new GameObject(UniqueName(scene, "Sticker_" + sprite.name));
        SceneManager.MoveGameObjectToScene(sticker, scene);
        Undo.RegisterCreatedObjectUndo(sticker, "Place Sticker");
        PlaceAtSceneViewCenter(sticker.transform);

        Undo.AddComponent<FormalWallSticker>(sticker);

        GameObject face = new GameObject("Face");
        face.transform.SetParent(sticker.transform, false);
        Undo.RegisterCreatedObjectUndo(face, "Create Sticker Face");

        SpriteRenderer renderer = Undo.AddComponent<SpriteRenderer>(face);
        renderer.sprite = sprite;
        FitFace(face.transform, sprite);

        EditorSceneManager.MarkSceneDirty(scene);
        Selection.activeGameObject = sticker;
        EditorGUIUtility.PingObject(sticker);

        Debug.Log("[Sticker] 已贴上 " + sticker.name + "（场景 " + scene.name + "，位置 " +
                  sticker.transform.position.ToString("F2") + "）。\n" +
                  "在 Inspector 的 Formal Wall Sticker / Hint Message 里填靠近时要弹的文字，" +
                  "留空就是纯装饰。摆好之后 Ctrl+S。", sticker);
    }

    [MenuItem("Tools/SuperBreadMan/贴纸/让选中的贴纸重新对齐当前视角")]
    public static void RealignSelectedSticker()
    {
        GameObject selected = Selection.activeGameObject;
        FormalWallSticker sticker = selected != null ? selected.GetComponentInParent<FormalWallSticker>() : null;
        if (sticker == null)
        {
            Debug.LogWarning("[Sticker] 先在 Hierarchy 里选中一个贴纸（带 Formal Wall Sticker 的那个物体）。");
            return;
        }

        Undo.RecordObject(sticker.transform, "Realign Sticker");
        PlaceAtSceneViewCenter(sticker.transform);
        EditorSceneManager.MarkSceneDirty(sticker.gameObject.scene);

        Debug.Log("[Sticker] " + sticker.name + " 已移动到 " +
                  sticker.transform.position.ToString("F2") + "。", sticker);
    }

    [MenuItem("Tools/SuperBreadMan/贴纸/把「关卡提示透明底」整个文件夹转成 Sprite")]
    public static void ConvertHintFolderToSprites()
    {
        string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { HintArtFolder });
        if (guids == null || guids.Length == 0)
        {
            Debug.LogError("[Sticker] " + HintArtFolder + " 里没找到图。");
            return;
        }

        int changed = 0;
        try
        {
            AssetDatabase.StartAssetEditing();
            foreach (string guid in guids)
            {
                if (MakeSpriteImporter(AssetDatabase.GUIDToAssetPath(guid)))
                    changed++;
            }
        }
        finally
        {
            AssetDatabase.StopAssetEditing();
            AssetDatabase.Refresh();
        }

        Debug.Log("[Sticker] " + HintArtFolder + "：共 " + guids.Length + " 张，改了 " + changed +
                  " 张的导入设置（Texture Type = Sprite，Alpha Is Transparency 打开）。");
    }

    /// <summary>Project 窗口里选中的图 -> Sprite。导入设置不对就顺手改掉再返回。</summary>
    static Sprite ResolveSelectedSprite()
    {
        Object selected = Selection.activeObject;
        if (selected == null)
        {
            Debug.LogError("[Sticker] 先在 Project 窗口里点中一张图，再点这个菜单。");
            return null;
        }

        string path = AssetDatabase.GetAssetPath(selected);
        if (string.IsNullOrEmpty(path) || AssetImporter.GetAtPath(path) as TextureImporter == null)
        {
            Debug.LogError("[Sticker] 选中的不是一张图片资源：" + (selected != null ? selected.name : "null"));
            return null;
        }

        if (Selection.objects != null && Selection.objects.Length > 1)
            Debug.LogWarning("[Sticker] 选了多张图，这次只贴第一张：" + selected.name);

        MakeSpriteImporter(path);

        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        if (sprite == null)
            Debug.LogError("[Sticker] 转成 Sprite 之后还是读不出来：" + path);

        return sprite;
    }

    /// <summary>把一张图的导入设置改成单张 Sprite。真的改了才返回 true。</summary>
    static bool MakeSpriteImporter(string path)
    {
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer == null)
            return false;

        bool changed = false;
        if (importer.textureType != TextureImporterType.Sprite)
        {
            importer.textureType = TextureImporterType.Sprite;
            changed = true;
        }

        if (importer.spriteImportMode != SpriteImportMode.Single)
        {
            importer.spriteImportMode = SpriteImportMode.Single;
            changed = true;
        }

        if (!importer.alphaIsTransparency)
        {
            importer.alphaIsTransparency = true;
            changed = true;
        }

        if (changed)
            importer.SaveAndReimport();

        return changed;
    }

    /// <summary>放到 Scene 视图正中间，绕 Y 轴转到正对镜头 —— 也就是正对你现在看的那面墙。</summary>
    static void PlaceAtSceneViewCenter(Transform target)
    {
        SceneView view = SceneView.lastActiveSceneView;
        if (view == null)
        {
            target.position = Vector3.zero;
            target.rotation = Quaternion.identity;
            Debug.LogWarning("[Sticker] 没有活动的 Scene 视图，先摆在原点，自己拖一下。");
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

    static void FitFace(Transform face, Sprite sprite)
    {
        float spriteHeight = sprite.bounds.size.y;
        float scale = spriteHeight > 0.0001f ? StickerWorldHeight / spriteHeight : 1f;

        face.localScale = new Vector3(scale, scale, scale);
        face.localPosition = new Vector3(0f, 0f, WallClearance);
        face.localRotation = Quaternion.identity;
    }

    /// <summary>同一张图贴好几处的时候自动加后缀，免得 Hierarchy 里一堆重名。</summary>
    static string UniqueName(Scene scene, string baseName)
    {
        HashSet<string> used = new HashSet<string>();
        foreach (GameObject root in scene.GetRootGameObjects())
            Collect(root.transform, used);

        if (!used.Contains(baseName))
            return baseName;

        for (int i = 2; i < 1000; i++)
        {
            string candidate = baseName + "_" + i;
            if (!used.Contains(candidate))
                return candidate;
        }

        return baseName;
    }

    static void Collect(Transform node, HashSet<string> used)
    {
        used.Add(node.name);
        foreach (Transform child in node)
            Collect(child, used);
    }
}
