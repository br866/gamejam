using System;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>Audio follows actual door/gate movement, including non-keyed puzzle doors.</summary>
public sealed class FormalMechanismAudioRouter : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Install()
    {
        if (FindObjectOfType<FormalMechanismAudioRouter>() != null) return;
        var go = new GameObject("[FormalMechanismAudioRouter]");
        DontDestroyOnLoad(go);
        go.AddComponent<FormalMechanismAudioRouter>();
    }
    private void Awake()
    {
        FormalSfxEvents.Preload();
        SceneManager.sceneLoaded += Loaded;
        for (int i = 0; i < SceneManager.sceneCount; i++) Bind(SceneManager.GetSceneAt(i));
    }
    private void Loaded(Scene s, LoadSceneMode mode) => Bind(s);
    private void Bind(Scene s)
    {
        if (!s.isLoaded || !(s.name.StartsWith("FormalLevel", StringComparison.Ordinal) || s.name.StartsWith("FormalSharedArt", StringComparison.Ordinal))) return;
        foreach (GameObject root in s.GetRootGameObjects())
        {
            foreach (FormalDoor door in root.GetComponentsInChildren<FormalDoor>(true))
                Get(door.gameObject).Initialize(door, null);
            foreach (GateController gate in root.GetComponentsInChildren<GateController>(true))
                Get(gate.gameObject).Initialize(null, gate);
        }
    }
    private static FormalMechanismAudioEmitter Get(GameObject go)
    {
        var emitter = go.GetComponent<FormalMechanismAudioEmitter>();
        return emitter != null ? emitter : go.AddComponent<FormalMechanismAudioEmitter>();
    }
    private void OnDestroy() => SceneManager.sceneLoaded -= Loaded;
}
