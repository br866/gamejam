using UnityEngine;

public class FormalPlayerVisualLoader : MonoBehaviour
{
    [SerializeField] private FormalPlayerActor human;
    [SerializeField] private FormalPlayerActor dog;
    [SerializeField] private GameObject humanVisualPrefab;
    [SerializeField] private GameObject dogVisualPrefab;

    void Awake()
    {
        LoadVisual(human, humanVisualPrefab, "FormalHumanVisual");
        LoadVisual(dog, dogVisualPrefab, "FormalDogVisual");
    }

    static void LoadVisual(FormalPlayerActor player, GameObject prefab, string instanceName)
    {
        if (player == null || prefab == null || player.transform.Find(instanceName) != null)
            return;

        GameObject visual = Instantiate(prefab, player.transform);
        visual.name = instanceName;
        visual.transform.localPosition = Vector3.zero;
    }
}
