using UnityEngine;

public class FormalPlayerSpawner : MonoBehaviour
{
    [SerializeField] private FormalPlayerActors playerActorsPrefab;

    void Awake()
    {
        if (FormalPlayerActors.Instance == null && playerActorsPrefab != null)
            Instantiate(playerActorsPrefab);
    }
}
