using System;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Creates the shared settings asset and resolves its Wwise Types through the
/// integration's supported reference API. Safe to run repeatedly.
/// </summary>
[InitializeOnLoad]
public static class WwiseUIFeedbackSettingsBootstrap
{
    private const string ResourcesFolder = "Assets/Resources";
    private const string MoMingFolder = ResourcesFolder + "/MoMing";
    private const string SettingsPath = MoMingFolder + "/WwiseUIFeedbackSettings.asset";

    private const string HoverName = "Play_UI_Hover";
    private const string HoverGuid = "39A8FF1E-71FF-4F9A-8DF2-CEDFEF889CFB";
    private const string ClickName = "Play_UI_Click";
    private const string ClickGuid = "37854238-6358-4DCC-9A9F-C4FB8C36EC72";
    private const string MusicVolumeName = "MusicVolume";
    private const string MusicVolumeGuid = "19F97BBC-96A2-467C-98C6-C4CC546EB40A";
    private const string SfxVolumeName = "SFXVolume";
    private const string SfxVolumeGuid = "8C118964-3C92-454F-B702-367221925079";

    private const int MaxAutomaticAttempts = 20;
    private static int automaticAttempts;
    private static bool queued;

    static WwiseUIFeedbackSettingsBootstrap()
    {
        QueueEnsure();
    }

    [MenuItem("MoMing/Audio/Refresh Wwise UI Feedback Settings")]
    public static void EnsureSettingsFromMenu()
    {
        automaticAttempts = 0;
        EnsureSettings(logSuccess: true);
    }

    private static void QueueEnsure()
    {
        if (queued)
            return;

        queued = true;
        EditorApplication.delayCall += EnsureSettingsAutomatically;
    }

    private static void EnsureSettingsAutomatically()
    {
        queued = false;
        if (EditorApplication.isCompiling || EditorApplication.isUpdating)
        {
            QueueEnsure();
            return;
        }

        if (!EnsureSettings(logSuccess: false) && automaticAttempts++ < MaxAutomaticAttempts)
            QueueEnsure();
    }

    private static bool EnsureSettings(bool logSuccess)
    {
        EnsureFolder(ResourcesFolder, "Resources");
        EnsureFolder(MoMingFolder, "MoMing");

        WwiseUIFeedbackSettings settings =
            AssetDatabase.LoadAssetAtPath<WwiseUIFeedbackSettings>(SettingsPath);
        bool created = false;
        if (settings == null)
        {
            settings = ScriptableObject.CreateInstance<WwiseUIFeedbackSettings>();
            AssetDatabase.CreateAsset(settings, SettingsPath);
            created = true;
        }

        settings.ConfigureEditorReferences(
            HoverName,
            new Guid(HoverGuid),
            ClickName,
            new Guid(ClickGuid),
            MusicVolumeName,
            new Guid(MusicVolumeGuid),
            SfxVolumeName,
            new Guid(SfxVolumeGuid));

        EditorUtility.SetDirty(settings);
        AssetDatabase.SaveAssets();

        if (settings.IsConfigured
            && settings.HasValidMusicVolumeRtpc
            && settings.HasValidSfxVolumeRtpc)
        {
            if (created || logSuccess)
                Debug.Log(
                    "[WwiseUIFeedback] UI Events and Music/SFX volume RTPCs are ready.",
                    settings);
            return true;
        }

        if (logSuccess)
        {
            Debug.LogWarning(
                "[WwiseUIFeedback] Wwise project data is not ready yet. " +
                "Refresh the Wwise Picker, then run this menu command again.",
                settings);
        }
        return false;
    }

    private static void EnsureFolder(string path, string leafName)
    {
        if (AssetDatabase.IsValidFolder(path))
            return;

        string parent = path.Substring(0, path.LastIndexOf('/'));
        AssetDatabase.CreateFolder(parent, leafName);
    }
}
