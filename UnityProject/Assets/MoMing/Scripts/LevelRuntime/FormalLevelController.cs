using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FormalLevelController : MonoBehaviour
{
    const float GroundProbeOriginY = 500f;
    const float GroundProbeDistance = 1000f;
    static readonly Quaternion RespawnRotation = Quaternion.LookRotation(Vector3.left, Vector3.up);

    [SerializeField] private string levelId;
    [SerializeField] private Transform humanRespawnAnchor;
    [SerializeField] private Transform dogRespawnAnchor;

    private readonly List<IFormalLevelTemporaryState> temporaryStates = new List<IFormalLevelTemporaryState>();
    private Transform checkpointHumanAnchor;
    private Transform checkpointDogAnchor;
    private bool hasCheckpoint;
    private bool recoveryInProgress;

    public string LevelId => levelId;
    public bool HasCheckpoint => hasCheckpoint;
    public Transform HumanRespawnAnchor => humanRespawnAnchor;
    public Transform DogRespawnAnchor => dogRespawnAnchor;

    void Awake()
    {
        RegisterChildren();
    }

    void Start()
    {
        if (FindObjectsOfType<FormalLevelController>().Length == 1)
            PlacePlayersAtRespawnAnchors();
    }

    public bool PlacePlayersAtRespawnAnchors()
    {
        return PlacePlayers(humanRespawnAnchor, dogRespawnAnchor);
    }

    public bool PlacePlayerAtRespawnAnchor(FormalPlayerActor actor, Transform anchor)
    {
        if (!TryResolveGroundPosition(anchor, actor, out Vector3 position))
            return false;

        actor.SetPositionAndRotation(position, RespawnRotation);
        return true;
    }

    public void RegisterTemporaryState(IFormalLevelTemporaryState state)
    {
        if (state != null && !temporaryStates.Contains(state))
            temporaryStates.Add(state);
    }

    public void SetCheckpoint(Transform humanAnchor, Transform dogAnchor)
    {
        checkpointHumanAnchor = humanAnchor != null ? humanAnchor : humanRespawnAnchor;
        checkpointDogAnchor = dogAnchor != null ? dogAnchor : dogRespawnAnchor;
        hasCheckpoint = checkpointHumanAnchor != null && checkpointDogAnchor != null;

        if (!hasCheckpoint)
            Debug.LogError($"[FormalLevelController] {levelId} checkpoint has no complete human/dog respawn-anchor pair.", this);
    }

    public bool RequestRecovery()
    {
        if (recoveryInProgress)
            return false;

        recoveryInProgress = true;
        try
        {
            // 重置前先全场扫一遍。只靠 Awake 里的注册不保险：
            // 关卡是 additive 加载的，各组件 Awake 的时候这个场景还没加载完，
            // 那一刻 scene.GetRootGameObjects() 拿到的根物体列表可能是不全的，
            // 漏注册的机关重开之后就不会复位（门一直开着，谜题白送）。
            // 走到这里场景肯定已经加载完了，扫一遍补齐最稳。
            CollectSceneTemporaryStates();

            foreach (IFormalLevelTemporaryState state in temporaryStates)
                state.ResetTemporaryState();

            if (FormalAnxietyState.Instance != null)
                FormalAnxietyState.Instance.ResetAnxiety();

            Transform humanAnchor = hasCheckpoint ? checkpointHumanAnchor : humanRespawnAnchor;
            Transform dogAnchor = hasCheckpoint ? checkpointDogAnchor : dogRespawnAnchor;
            return PlacePlayers(humanAnchor, dogAnchor);
        }
        finally
        {
            recoveryInProgress = false;
        }
    }

    public void ResetLevel()
    {
        RequestRecovery();
    }

    /// <summary>
    /// 把本关场景里所有可复位机关都收进来（只管自己这个场景，
    /// 过关门那种摆在 SharedArt 场景里的不碰，它们本来就该保持打开）。
    /// </summary>
    void CollectSceneTemporaryStates()
    {
        Scene scene = gameObject.scene;
        if (!scene.IsValid() || !scene.isLoaded)
            return;

        int added = 0;
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            foreach (MonoBehaviour behaviour in root.GetComponentsInChildren<MonoBehaviour>(true))
            {
                IFormalLevelTemporaryState state = behaviour as IFormalLevelTemporaryState;
                if (state == null || temporaryStates.Contains(state))
                    continue;

                temporaryStates.Add(state);
                added++;
            }
        }

        if (added > 0)
            Debug.Log($"[FormalLevelController] {levelId} 复位时补登记了 {added} 个漏注册的机关。", this);
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

    bool PlacePlayers(Transform humanAnchor, Transform dogAnchor)
    {
        FormalPlayerActors actors = FormalPlayerActors.Instance;
        if (actors == null || actors.Human == null || actors.Dog == null)
        {
            Debug.LogError($"[FormalLevelController] {levelId} cannot place players because the shared actor pair is unavailable.", this);
            return false;
        }

        if (!TryResolveGroundPosition(humanAnchor, actors.Human, out Vector3 humanPosition) ||
            !TryResolveGroundPosition(dogAnchor, actors.Dog, out Vector3 dogPosition))
            return false;

        actors.Human.SetPositionAndRotation(humanPosition, RespawnRotation);
        actors.Dog.SetPositionAndRotation(dogPosition, RespawnRotation);
        return true;
    }

    bool TryResolveGroundPosition(Transform anchor, FormalPlayerActor actor, out Vector3 position)
    {
        position = default;
        if (anchor == null || actor == null)
        {
            Debug.LogError($"[FormalLevelController] {levelId} has a missing respawn anchor or actor.", this);
            return false;
        }

        Vector3 origin = new Vector3(anchor.position.x, GroundProbeOriginY, anchor.position.z);
        RaycastHit[] hits = Physics.RaycastAll(origin, Vector3.down, GroundProbeDistance,
            Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
        System.Array.Sort(hits, (left, right) => left.distance.CompareTo(right.distance));

        foreach (RaycastHit hit in hits)
        {
            Collider collider = hit.collider;
            if (collider == null || collider.isTrigger || !collider.enabled ||
                collider.GetComponentInParent<FormalPlayerActor>() != null)
                continue;

            position = new Vector3(anchor.position.x, hit.point.y, anchor.position.z) - actor.MoverAttachOffset;
            return true;
        }

        Debug.LogError($"[FormalLevelController] {levelId}/{anchor.name} has no valid ground below its XZ position; player placement was refused.", anchor);
        return false;
    }
}
