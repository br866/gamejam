using System;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Installs spatial fluorescent-light audio on every pendant lamp in loaded formal levels.
/// </summary>
public sealed class FormalFluorescentLightAudioRouter : MonoBehaviour
{
    private const string RouterObjectName = "[FormalFluorescentLightAudioRouter]";
    private const string FormalScenePrefix = "FormalLevel";
    private const string PendantLampName = "pendant_lamp";

    private WwiseUIFeedbackSettings settings;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void InstallRouter()
    {
        if (FindObjectOfType<FormalFluorescentLightAudioRouter>() != null)
            return;

        GameObject routerObject = new GameObject(RouterObjectName);
        DontDestroyOnLoad(routerObject);
        routerObject.AddComponent<FormalFluorescentLightAudioRouter>();
    }

    private void Awake()
    {
        settings = Resources.Load<WwiseUIFeedbackSettings>(WwiseUIFeedbackSettings.ResourcesPath);
        SceneManager.sceneLoaded += OnSceneLoaded;
        InstallOnAllLoadedScenes();
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        InstallOnScene(scene);
    }

    private void InstallOnAllLoadedScenes()
    {
        int installedCount = 0;
        for (int i = 0; i < SceneManager.sceneCount; i++)
            installedCount += InstallOnScene(SceneManager.GetSceneAt(i));

        if (installedCount > 0)
        {
            Debug.Log(
                $"[FormalFluorescentLightAudio] Installed audio on {installedCount} pendant lamp(s).");
        }
    }

    private int InstallOnScene(Scene scene)
    {
        if (!scene.IsValid()
            || !scene.isLoaded
            || !scene.name.StartsWith(FormalScenePrefix, StringComparison.OrdinalIgnoreCase))
        {
            return 0;
        }

        if (settings == null || !settings.HasValidFluorescentLightEvent)
        {
            Debug.LogWarning(
                "[FormalFluorescentLightAudio] Play_Fluorescent_Light is not configured; lamps remain silent.",
                this);
            return 0;
        }

        int installedCount = 0;
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            Transform[] transforms = root.GetComponentsInChildren<Transform>(true);
            foreach (Transform candidate in transforms)
            {
                if (!IsCeilingLampName(candidate.name))
                    continue;

                FormalFluorescentLightAudioEmitter emitter =
                    candidate.GetComponent<FormalFluorescentLightAudioEmitter>();
                if (emitter == null)
                {
                    emitter = candidate.gameObject.AddComponent<FormalFluorescentLightAudioEmitter>();
                    installedCount++;
                }

                emitter.Initialize(settings.FluorescentLightEvent);
            }
        }

        return installedCount;
    }

    public static bool IsCeilingLampName(string objectName)
    {
        return !string.IsNullOrEmpty(objectName)
            && objectName.IndexOf(PendantLampName, StringComparison.OrdinalIgnoreCase) >= 0;
    }
}
