using UnityEngine;

public class FormalPlayerVisualLoader : MonoBehaviour
{
    [SerializeField] private FormalPlayerActor human;
    [SerializeField] private FormalPlayerActor dog;
    [SerializeField] private GameObject humanVisualPrefab;
    [SerializeField] private GameObject dogVisualPrefab;
    [SerializeField] private float humanVisualScale = 1f;
    [SerializeField] private float dogVisualScale = 1f;
    [SerializeField] private Vector3 humanVisualOffset;
    [SerializeField] private Vector3 dogVisualOffset;

    void Awake()
    {
        LoadVisual(human, humanVisualPrefab, "FormalHumanVisual", humanVisualScale, humanVisualOffset);
        LoadVisual(dog, dogVisualPrefab, "FormalDogVisual", dogVisualScale, dogVisualOffset);
    }

    static void LoadVisual(FormalPlayerActor player, GameObject prefab, string instanceName, float scale, Vector3 offset)
    {
        if (player == null || prefab == null || player.transform.Find(instanceName) != null)
            return;

        GameObject visual = Instantiate(prefab, player.transform);
        visual.name = instanceName;
        visual.transform.localPosition += offset;
        visual.transform.localScale *= scale;
    }
}
