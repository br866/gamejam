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
    [Tooltip("抓捕缓冲：在人物与怪物碰撞体刚接触的基础上再外扩一点。最终阈值=怪物半径+玩家半径+此值")]
    public float catchBuffer = 0.35f;
    public float loseTargetTime = 3f;

    [Header("Chase")]
    public float chaseSpeed = 4.5f;
    public float chaseStopDistance = 1f;

    [Header("Audio")]
    [SerializeField] private AudioClip footstepClip;
    [SerializeField] private AudioClip catchClip;
    [SerializeField] private AudioClip detectClip;

    [Tooltip("怪物走多远算一步")]
    public float footstepDistance = 1.6f;

    [Header("Detection Gizmos")]
    public bool showDetectionGizmos = true;
    public Color fieldOfViewGizmoColor = new Color(1f, 0.85f, 0f, 0.9f);

    private State currentState = State.Patrol;
    private int currentWaypoint = 0;
    private AudioSource audioSource;
    private LevelMonsterNavigation navigation;
    private Vector3 startPos;
    private Transform chaseTarget;
    private float lastSeenTime;
    private Vector3 lastStepPos;
    private CapsuleCollider selfCol;

    void Awake()
    {
        if (!HasAssignedWaypoints())
        {
            enabled = false;
            return;
        }

        audioSource = GetComponent<AudioSource>();
        navigation = GetComponent<LevelMonsterNavigation>();
        selfCol = GetComponent<CapsuleCollider>();
        startPos = transform.position;
        lastStepPos = transform.position;
    }

    void Start()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnLevelReset += ResetPatrol;
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
        HandleFootsteps();
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
        if (!IsInRoom(chaseTarget.position))
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

        // 2. 检测玩家：必须同时满足(在房间内) AND (在锥形视野内)
        Collider[] detectionHits = Physics.OverlapSphere(transform.position, detectionRange);
        foreach (var hit in detectionHits)
        {
            if (hit.CompareTag("Player"))
            {
                // 玩家不在怪物房间内 → 不检测
                if (!IsInRoom(hit.transform.position))
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

                if (inSight)
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

    /// <summary>怪物移动时的脚步声，按移动距离触发。</summary>
    void HandleFootsteps()
    {
        Vector3 a = transform.position; a.y = 0f;
        Vector3 b = lastStepPos;        b.y = 0f;
        if ((a - b).sqrMagnitude < footstepDistance * footstepDistance) return;
        lastStepPos = transform.position;

        if (footstepClip != null)
        {
            PlayAudio(footstepClip);
            return;
        }
        SfxManager.PlayRandom(Sfx.FootstepFolder, Sfx.MonsterStep, transform.position);
    }

    void PlayAudio(AudioClip clip)
    {
        if (clip != null && audioSource != null)
            audioSource.PlayOneShot(clip);
    }

    bool TryCatch(Transform player)
    {
        if (player == null || !player.gameObject.activeInHierarchy) return false;

        // XZ 平面中心距（忽略 Y 高度差）
        float dx = player.position.x - transform.position.x;
        float dz = player.position.z - transform.position.z;
        float xzDist = Mathf.Sqrt(dx * dx + dz * dz);

        // 抓捕阈值随实际碰撞体大小自适应：怪物世界半径 + 玩家世界半径 + 缓冲。
        // 怪物被放大后(scale 2.2)实心碰撞体会把玩家顶开，中心永远进不到旧的固定 catchRadius(1.2) 内，
        // 所以按真实半径算，保证“贴上就抓到”。找不到碰撞体时回退到 catchRadius。
        float threshold = WorldRadius(selfCol, transform)
                        + WorldRadius(player.GetComponent<CapsuleCollider>(), player)
                        + catchBuffer;
        if (threshold < catchRadius) threshold = catchRadius;

        if (xzDist <= threshold)
        {
            if (GameManager.Instance != null)
                GameManager.Instance.OnPlayerCaught();
            PlayAudio(catchClip);
            return true;
        }
        return false;
    }

    /// <summary>胶囊碰撞体在世界空间下的半径（Y 轴胶囊按 X/Z 里较大的缩放算）。</summary>
    static float WorldRadius(CapsuleCollider c, Transform t)
    {
        if (c == null || t == null) return 0f;
        Vector3 sc = t.lossyScale;
        float radialScale = Mathf.Max(Mathf.Abs(sc.x), Mathf.Abs(sc.z));
        return c.radius * radialScale;
    }

    public void ResetPatrol()
    {
        currentState = State.Patrol;
        chaseTarget = null;
        currentWaypoint = 0;
        transform.position = startPos;
    }

}
