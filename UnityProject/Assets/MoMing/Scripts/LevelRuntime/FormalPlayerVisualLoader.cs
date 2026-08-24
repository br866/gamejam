using UnityEngine;

public class FormalPlayerVisualLoader : MonoBehaviour
{
    [SerializeField] private FormalPlayerActor human;
    [SerializeField] private FormalPlayerActor dog;
    [SerializeField] private GameObject humanVisualPrefab;
    [SerializeField] private GameObject dogVisualPrefab;
    [SerializeField] private Vector3 humanVisualOffset;
    [SerializeField] private Vector3 dogVisualOffset;

    void Awake()
    {
        LoadVisual(human, humanVisualPrefab, "FormalHumanVisual", humanVisualOffset);
        LoadVisual(dog, dogVisualPrefab, "FormalDogVisual", dogVisualOffset);
    }

    static void LoadVisual(FormalPlayerActor player, GameObject prefab, string instanceName, Vector3 offset)
    {
        if (player == null || prefab == null)
            return;

        // 视觉挂到 Body 下，与胶囊共享同一缩放旋钮；无 Body 时回退到根节点。
        Transform parent = player.transform.Find("Body");
        if (parent == null)
            parent = player.transform;
        if (parent.Find(instanceName) != null)
            return;

        GameObject visual = Instantiate(prefab, parent);
        visual.name = instanceName;
        visual.transform.localPosition += offset;
    }
}
