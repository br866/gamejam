using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 批量把 UI 图的导入设置改成 Sprite，否则在 Image 组件的 Source Image
/// 下拉框里根本看不见它们。
///
/// 菜单：Tools / UI / ...
/// </summary>
public static class UISpriteImportFixer
{
    private const string UiRoot = "Assets/SuperBreadMan/ui";

    // 参考图/概念图不进游戏，跳过，省得白占图集和内存
    private static readonly string[] SkipKeywords = { "参考", "概念", "developer_panel", "开发者面板" };

    [MenuItem("Tools/UI/把 SuperBreadMan UI 图批量设为 Sprite", false, 1)]
    private static void FixAllUiSprites()
    {
        if (!AssetDatabase.IsValidFolder(UiRoot))
        {
            EditorUtility.DisplayDialog("找不到目录", "没有 " + UiRoot + " 这个目录。", "知道了");
            return;
        }

        string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { UiRoot });
        Apply(guids, "整个 ui 目录");
    }

    [MenuItem("Tools/UI/把选中的图设为 Sprite", false, 2)]
    private static void FixSelectedSprites()
    {
        var guids = new List<string>();
        foreach (Object obj in Selection.objects)
        {
            string path = AssetDatabase.GetAssetPath(obj);

            if (AssetDatabase.IsValidFolder(path))
            {
                guids.AddRange(AssetDatabase.FindAssets("t:Texture2D", new[] { path }));
            }
            else if (obj is Texture2D)
            {
                guids.Add(AssetDatabase.AssetPathToGUID(path));
            }
        }

        if (guids.Count == 0)
        {
            EditorUtility.DisplayDialog("没选中图片", "先在 Project 窗口里选中图片或文件夹。", "知道了");
            return;
        }

        Apply(guids.ToArray(), "选中的 " + guids.Count + " 项");
    }

    [MenuItem("Tools/UI/报告：哪些 UI 图还不是 Sprite", false, 20)]
    private static void ReportNonSprites()
    {
        string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { UiRoot });
        var pending = new List<string>();

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) continue;
            if (importer.textureType != TextureImporterType.Sprite)
                pending.Add(path);
        }

        if (pending.Count == 0)
        {
            Debug.Log("[UISpriteImportFixer] 全部都已经是 Sprite 了。");
            return;
        }

        Debug.Log($"[UISpriteImportFixer] 还有 {pending.Count} 张不是 Sprite：\n  " + string.Join("\n  ", pending));
    }

    private static void Apply(string[] guids, string scopeLabel)
    {
        var changed = new List<string>();
        var skipped = new List<string>();

        try
        {
            AssetDatabase.StartAssetEditing();

            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                string fileName = Path.GetFileNameWithoutExtension(path);

                EditorUtility.DisplayProgressBar("设置 Sprite 导入", fileName, (float)i / Mathf.Max(1, guids.Length));

                if (ShouldSkip(fileName))
                {
                    skipped.Add(path);
                    continue;
                }

                var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer == null) continue;

                bool dirty = false;

                if (importer.textureType != TextureImporterType.Sprite)
                {
                    importer.textureType = TextureImporterType.Sprite;
                    dirty = true;
                }

                if (importer.spriteImportMode != SpriteImportMode.Single)
                {
                    importer.spriteImportMode = SpriteImportMode.Single;
                    dirty = true;
                }

                // UI 图不需要 mipmap，开着只会让画面在缩放时发糊
                if (importer.mipmapEnabled)
                {
                    importer.mipmapEnabled = false;
                    dirty = true;
                }

                if (!importer.alphaIsTransparency)
                {
                    importer.alphaIsTransparency = true;
                    dirty = true;
                }

                // Clamp 避免边缘出现对侧像素的渗色
                if (importer.wrapMode != TextureWrapMode.Clamp)
                {
                    importer.wrapMode = TextureWrapMode.Clamp;
                    dirty = true;
                }

                if (importer.filterMode != FilterMode.Bilinear)
                {
                    importer.filterMode = FilterMode.Bilinear;
                    dirty = true;
                }

                // UI 用高质量压缩，DXT 会把渐变压出色带
                if (importer.textureCompression != TextureImporterCompression.CompressedHQ)
                {
                    importer.textureCompression = TextureImporterCompression.CompressedHQ;
                    dirty = true;
                }

                if (dirty)
                {
                    importer.SaveAndReimport();
                    changed.Add(path);
                }
            }
        }
        finally
        {
            AssetDatabase.StopAssetEditing();
            EditorUtility.ClearProgressBar();
            AssetDatabase.Refresh();
        }

        string report = $"范围：{scopeLabel}\n改好了 {changed.Count} 张，跳过参考图 {skipped.Count} 张。";
        Debug.Log("[UISpriteImportFixer] " + report +
                  (changed.Count > 0 ? "\n  " + string.Join("\n  ", changed) : ""));
        EditorUtility.DisplayDialog("批量设置完成", report, "好");
    }

    private static bool ShouldSkip(string fileName)
    {
        foreach (string keyword in SkipKeywords)
        {
            if (fileName.Contains(keyword))
                return true;
        }
        return false;
    }
}
