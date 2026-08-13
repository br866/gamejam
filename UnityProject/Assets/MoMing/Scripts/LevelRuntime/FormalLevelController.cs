using System.Collections.Generic;
using UnityEngine;

public class FormalLevelController : MonoBehaviour
{
    [SerializeField] private string levelId;
    [SerializeField] private Transform humanSpawn;
    [SerializeField] private Transform dogSpawn;

    private readonly List<IFormalLevelTemporaryState> temporaryStates = new List<IFormalLevelTemporaryState>();
    private Vector3 checkpointHuman;
    private Vector3 checkpointDog;
    private bool hasCheckpoint;

    public string LevelId => levelId;
    public bool HasCheckpoint => hasCheckpoint;

    void Awake()
    {
        RegisterChildren();
    }

    void Start()
    {
        PlacePlayersAtSpawn();
    }

    public void PlacePlayersAtSpawn()
    {
        MovePlayer(FormalPlayerActors.Instance != null ? FormalPlayerActors.Instance.Human : null, humanSpawn != null ? humanSpawn.position : Vector3.zero);
        MovePlayer(FormalPlayerActors.Instance != null ? FormalPlayerActors.Instance.Dog : null, dogSpawn != null ? dogSpawn.position : Vector3.zero);
    }

    public void RegisterTemporaryState(IFormalLevelTemporaryState state)
    {
        if (state != null && !temporaryStates.Contains(state))
            temporaryStates.Add(state);
    }

    public void SetCheckpoint(Transform activatingPlayer)
    {
        Vector3 spawn = activatingPlayer.position;
        checkpointHuman = humanSpawn != null ? humanSpawn.position : spawn;
        checkpointDog = dogSpawn != null ? dogSpawn.position : spawn;
        hasCheckpoint = true;
    }

    public void ResetLevel()
    {
        foreach (IFormalLevelTemporaryState state in temporaryStates)
            state.ResetTemporaryState();

        if (!hasCheckpoint)
            return;

        MovePlayer(FormalPlayerActors.Instance != null ? FormalPlayerActors.Instance.Human : null, checkpointHuman);
        MovePlayer(FormalPlayerActors.Instance != null ? FormalPlayerActors.Instance.Dog : null, checkpointDog);
    }

    void RegisterChildren()
    {
        MonoBehaviour[] behaviours = GetComponentsInChildren<MonoBehaviour>(true);
        foreach (MonoBehaviour behaviour in behaviours)
        {
            IFormalLevelTemporaryState state = behaviour as IFormalLevelTemporaryState;
            if (state != null)
                RegisterTemporaryState(state);
        }
    }

    static void MovePlayer(FormalPlayerActor player, Vector3 position)
    {
        if (player == null)
            return;

        player.SetPosition(position);
    }
}
