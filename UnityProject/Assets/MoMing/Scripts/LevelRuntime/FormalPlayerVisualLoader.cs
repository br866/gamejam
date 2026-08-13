using UnityEngine;

public class FormalPlayerVisualLoader : MonoBehaviour
{
    [SerializeField] private PlayerController human;
    [SerializeField] private PlayerController dog;
    [SerializeField] private GameObject humanVisualPrefab;
    [SerializeField] private GameObject dogVisualPrefab;

    void Awake()
    {
        LoadVisual(human, humanVisualPrefab, "FormalHumanVisual");
        LoadVisual(dog, dogVisualPrefab, "FormalDogVisual");
    }

    static void LoadVisual(PlayerController player, GameObject prefab, string instanceName)
    {
        if (player == null || prefab == null || player.transform.Find(instanceName) != null)
            return;

        foreach (MeshRenderer renderer in player.GetComponentsInChildren<MeshRenderer>(true))
            renderer.enabled = false;

        GameObject visual = Instantiate(prefab, player.transform);
        visual.name = instanceName;
        visual.transform.localPosition = Vector3.zero;
    }
}
