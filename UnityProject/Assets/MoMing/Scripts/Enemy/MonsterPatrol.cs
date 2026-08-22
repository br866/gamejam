using UnityEngine;

/// <summary>
/// 怪物：仅在自己房间内活动。玩家进入房间时触发检测(锥形视野+听觉)，追击也限定在房间内。
/// 玩家离开房间后怪物丢失目标，回到巡逻。
/// </summary>
public class MonsterPatrol : MonoBehaviour
{
    public enum State { Patrol, Chase }

    [Header("Room Bounds (怪物活动范围)")]
    public Vector3 roomCenter = new Vector3(14f, 0f, 0f);
    public Vector3 roomSize = new Vector3(16f, 10f, 10f);

    [Header("Patrol")]
    public Transform[] waypoints;
    public float patrolSpeed = 2.5f;
    public float arrivalThreshold = 0.3f;

    [Header("Detection")]
    public float detectionRange = 8f;
    public float fieldOfView = 60f;
    public float catchRadius = 1.2f;
    public float loseTargetTime = 3f;

    [Header("Chase")]
    public float chaseSpeed = 4.5f;
    public float chaseStopDistance = 1f;

    [Header("Safe Zones")]
    [SerializeField] private Collider[] safeZones;

    [Header("Audio")]
    [SerializeField] private AudioClip footstepClip;
    [SerializeField] private AudioClip catchClip;
    [SerializeField] private AudioClip detectClip;

    [Header("Line of Sight")]
    [SerializeField] private LayerMask sightBlockMask;
    [SerializeField] private float eyeHeight = 1.5f;

    [Header("Detection Gizmos")]
    public bool showDetectionGizmos = true;
    public Color fieldOfViewGizmoColor = new Color(1f, 0.85f, 0f, 0.9f);

    [Header("Room Bounds Gizmos")]
    public bool showRoomBounds = true;
    public Color roomBoundsGizmoColor = new Color(0f, 1f, 0.55f, 0.9f);

    bool HasLineOfSight(Transform target)
    {
        Vector3 from = transform.position;
        if (visualRenderer != null)
            from.y = visualRenderer.bounds.min.y + eyeHeight;

        Vector3 to = target.position + Vector3.up * (eyeHeight * 0.5f);
        int mask = sightBlockMask.value != 0 ? sightBlockMask.value : ~0;
        mask &= ~(1 << gameObject.layer);
        mask &= ~(1 << target.gameObject.layer);

        if (Physics.Linecast(from, to, out RaycastHit hitInfo, mask, QueryTriggerInteraction.Ignore))
        {
            if (hitInfo.collider.transform == target || hitInfo.collider.transform.IsChildOf(transform))
                return true;
            return false;
        }

        return true;
    }

    void OnDrawGizmos()
    {
        if (showDetectionGizmos)
        {
            Gizmos.color = fieldOfViewGizmoColor;
            Vector3 origin = transform.position;
            Vector3 flatForward = transform.forward;
            flatForward.y = 0f;
            flatForward.Normalize();
            if (flatForward.sqrMagnitude < 0.001f)
                flatForward = Vector3.forward;

            float half = fieldOfView * 0.5f;
            Vector3 dirLeft = Quaternion.Euler(0f, -half, 0f) * flatForward;
            Vector3 dirRight = Quaternion.Euler(0f, half, 0f) * flatForward;
            Gizmos.DrawRay(origin, dirLeft * detectionRange);
            Gizmos.DrawRay(origin, dirRight * detectionRange);

            const int segments = 20;
            Vector3 previous = origin + dirLeft * detectionRange;
            for (int i = 1; i <= segments; i++)
            {
                float angle = Mathf.Lerp(-half, half, i / (float)segments);
                Vector3 next = origin + Quaternion.Euler(0f, angle, 0f) * flatForward * detectionRange;
                Gizmos.DrawLine(previous, next);
                previous = next;
            }
        }

        if (!showRoomBounds)
            return;

        float y = Application.isPlaying ? startPos.y : transform.position.y;
        Vector3 center = new Vector3(roomCenter.x, y, roomCenter.z);
        Vector3 size = new Vector3(roomSize.x, 0.05f, roomSize.z);
        Gizmos.color = roomBoundsGizmoColor;
        Gizmos.DrawWireCube(center, size);

        if (waypoints != null)
        {
            Gizmos.color = Color.cyan;
            foreach (Transform waypoint in waypoints)
                if (waypoint != null)
                    Gizmos.DrawWireSphere(waypoint.position, 0.35f);
        }
    }

    private State currentState = State.Patrol;
    private Renderer visualRenderer;

    public bool IsChasing
    {
        get { return currentState == State.Chase; }
    }
    private int currentWaypoint = 0;
    private AudioSource audioSource;
    private LevelMonsterNavigation navigation;
    private Vector3 startPos;
    private Transform chaseTarget;
    private float lastSeenTime;

    void Awake()
    {
        if (!HasAssignedWaypoints())
        {
            enabled = false;
            return;
        }

        audioSource = GetComponent<AudioSource>();
        navigation = GetComponent<LevelMonsterNavigation>();
        startPos = transform.position;
        foreach (Renderer r in GetComponentsInChildren<Renderer>())
            if (r.enabled && r.gameObject.activeInHierarchy)
            {
                visualRenderer = r;
                break;
            }
        ResolveFormalSafeZones();
    }

    void ResolveFormalSafeZones()
    {
        if (safeZones != null && safeZones.Length > 0)
            return;

        FormalActuatorTrigger[] triggers = FindObjectsOfType<FormalActuatorTrigger>(true);
        var resolved = new System.Collections.Generic.List<Collider>();
        foreach (FormalActuatorTrigger trigger in triggers)
        {
            if (trigger.gameObject.scene != gameObject.scene ||
                trigger.gameObject.name.IndexOf("SafeZone", System.StringComparison.OrdinalIgnoreCase) < 0)
                continue;

            Collider zone = trigger.GetComponent<Collider>();
            if (zone != null)
                resolved.Add(zone);
        }

        safeZones = resolved.ToArray();
    }

    void Start()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnLevelReset += ResetPatrol;

        FormalLevelController level = FormalLevelActors.FindLevelController(gameObject.scene);
        if (level != null)
            level.RegisterTemporaryState(new FormalMonsterResetState(this));
    }

    void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnLevelReset -= ResetPatrol;
    }

    void Update()
    {
        if (currentState == State.Patrol)
        {
            Patrol();
        }
        else if (currentState == State.Chase)
        {
            Chase();
        }

        CheckForPlayers();
    }

    bool HasAssignedWaypoints()
    {
        if (waypoints == null || waypoints.Length == 0)
            return false;

        foreach (Transform waypoint in waypoints)
        {
            if (waypoint == null)
                return false;
        }

        return true;
    }

    // 判断某点是否在房间范围内
    bool IsInRoom(Vector3 pos)
    {
        Vector3 min = roomCenter - roomSize * 0.5f;
        Vector3 max = roomCenter + roomSize * 0.5f;
        return pos.x >= min.x && pos.x <= max.x &&
               pos.z >= min.z && pos.z <= max.z;
    }

    // 将位置限制在房间范围内
    Vector3 ClampToRoom(Vector3 pos)
    {
        Vector3 min = roomCenter - roomSize * 0.5f;
        Vector3 max = roomCenter + roomSize * 0.5f;
        pos.x = Mathf.Clamp(pos.x, min.x, max.x);
        pos.z = Mathf.Clamp(pos.z, min.z, max.z);
        return pos;
    }

    void Patrol()
    {
        Transform wp = waypoints[currentWaypoint];
        Vector3 targetPos = new Vector3(wp.position.x, startPos.y, wp.position.z);

        if (navigation != null)
        {
            navigation.SetMoveSpeed(patrolSpeed);
            navigation.SetDestination(targetPos);
            if (navigation.HasArrived)
                currentWaypoint = (currentWaypoint + 1) % waypoints.Length;
            return;
        }

        Vector3 dir = (targetPos - transform.position);
        dir.y = 0f;
        dir.Normalize();

        if (dir != Vector3.zero)
        {
            Quaternion targetRot = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, 5f * Time.deltaTime);
        }

        transform.position = Vector3.MoveTowards(transform.position, targetPos, patrolSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, targetPos) < arrivalThreshold)
        {
            currentWaypoint = (currentWaypoint + 1) % waypoints.Length;
        }
    }

    void Chase()
    {
        if (chaseTarget == null)
        {
            currentState = State.Patrol;
            return;
        }

        // 目标离开房间 → 丢失目标
        if (!IsInRoom(chaseTarget.position) || IsInSafeZone(chaseTarget.position))
        {
            chaseTarget = null;
            currentState = State.Patrol;
            Debug.Log("[Monster] Target left room, returning to patrol.");
            return;
        }

        Vector3 targetPos = new Vector3(chaseTarget.position.x, startPos.y, chaseTarget.position.z);
        float dist = Vector3.Distance(transform.position, targetPos);

        if (navigation != null)
        {
            navigation.SetMoveSpeed(chaseSpeed);
            navigation.SetDestination(targetPos);
            if (dist <= chaseStopDistance)
                navigation.ClearDestination();
            return;
        }

        if (dist > chaseStopDistance)
        {
            Vector3 dir = (targetPos - transform.position).normalized;
            if (dir != Vector3.zero)
            {
                Quaternion targetRot = Quaternion.LookRotation(dir);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, 8f * Time.deltaTime);
            }
            Vector3 newPos = Vector3.MoveTowards(transform.position, targetPos, chaseSpeed * Time.deltaTime);
            // 限制在房间内
            transform.position = ClampToRoom(newPos);
        }

        // 超时丢失目标
        if (Time.time - lastSeenTime > loseTargetTime)
        {
            chaseTarget = null;
            currentState = State.Patrol;
            Debug.Log("[Monster] Lost target, returning to patrol.");
        }
    }

    void CheckForPlayers()
    {
        // 1. 接触抓捕 (XZ平面距离判定，避免Y高度差导致无法抓捕)
        if (GameManager.Instance != null)
        {
            if (TryCatch(GameManager.Instance.humanPlayer)) return;
            if (TryCatch(GameManager.Instance.dogPlayer)) return;
        }

        if (FormalPlayerActors.Instance != null)
        {
            if (TryCatch(FormalPlayerActors.Instance.Human != null ? FormalPlayerActors.Instance.Human.transform : null)) return;
            if (TryCatch(FormalPlayerActors.Instance.Dog != null ? FormalPlayerActors.Instance.Dog.transform : null)) return;
        }

        // 2. 检测玩家：必须同时满足(在房间内) AND (在锥形视野内)
        Collider[] detectionHits = Physics.OverlapSphere(transform.position, detectionRange);
        foreach (var hit in detectionHits)
        {
            FormalPlayerActor formalPlayer = FormalLevelActors.ResolvePlayer(hit);
            if (hit.CompareTag("Player") || formalPlayer != null)
            {
                // 玩家不在怪物房间内 → 不检测
                if (!IsInRoom(hit.transform.position) || IsInSafeZone(hit.transform.position))
                    continue;

                // XZ平面计算，忽略Y高度差
                Vector3 toPlayer = hit.transform.position - transform.position;
                toPlayer.y = 0f;
                float distToPlayer = toPlayer.magnitude;
                Vector3 dirToPlayer = toPlayer.normalized;
                Vector3 flatForward = transform.forward;
                flatForward.y = 0f;
                flatForward.Normalize();
                float angle = Vector3.Angle(flatForward, dirToPlayer);

                bool inSight = angle <= fieldOfView * 0.5f;

                if (inSight && HasLineOfSight(hit.transform))
                {
                    bool wasChasing = currentState == State.Chase;
                    chaseTarget = hit.transform;
                    lastSeenTime = Time.time;
                    currentState = State.Chase;

                    if (!wasChasing)
                    {
                        PlayAudio(detectClip);
                        Debug.Log("[Monster] saw " + hit.gameObject.name + " in room! Chasing!");
                    }
                    return;
                }
            }
        }
    }

    void PlayAudio(AudioClip clip)
    {
        if (clip != null && audioSource != null)
            audioSource.PlayOneShot(clip);
    }

    bool TryCatch(Transform player)
    {
        if (player == null || !player.gameObject.activeInHierarchy) return false;
        if (IsInSafeZone(player.position)) return false;
        if (!HasLineOfSight(player)) return false;
        float xzDist = Mathf.Sqrt(
            (player.position.x - transform.position.x) * (player.position.x - transform.position.x) +
            (player.position.z - transform.position.z) * (player.position.z - transform.position.z));
        if (xzDist <= catchRadius)
        {
            if (GameManager.Instance != null)
                GameManager.Instance.OnPlayerCaught();
            else
            {
                FormalLevelController level = FormalLevelActors.FindLevelController(gameObject.scene);
                if (level != null)
                    level.ResetLevel();
            }
            PlayAudio(catchClip);
            return true;
        }
        return false;
    }

    public void ResetPatrol()
    {
        currentState = State.Patrol;
        chaseTarget = null;
        currentWaypoint = 0;
        transform.position = startPos;
    }

    bool IsInSafeZone(Vector3 position)
    {
        if (safeZones == null)
            return false;

        foreach (Collider safeZone in safeZones)
        {
            if (safeZone != null && safeZone.bounds.Contains(position))
                return true;
        }

        return false;
    }

    class FormalMonsterResetState : IFormalLevelTemporaryState
    {
        readonly MonsterPatrol patrol;

        public FormalMonsterResetState(MonsterPatrol patrol)
        {
            this.patrol = patrol;
        }

        public void ResetTemporaryState()
        {
            patrol.ResetPatrol();
        }
    }

}
