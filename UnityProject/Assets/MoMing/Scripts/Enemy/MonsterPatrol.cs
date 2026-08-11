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

    [Header("Audio")]
    [SerializeField] private AudioClip footstepClip;
    [SerializeField] private AudioClip catchClip;
    [SerializeField] private AudioClip detectClip;

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

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        navigation = GetComponent<LevelMonsterNavigation>();
        startPos = transform.position;
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
        if (currentState == State.Patrol && waypoints != null && waypoints.Length > 0)
        {
            Patrol();
        }
        else if (currentState == State.Chase)
        {
            Chase();
        }

        CheckForPlayers();
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

    void PlayAudio(AudioClip clip)
    {
        if (clip != null && audioSource != null)
            audioSource.PlayOneShot(clip);
    }

    bool TryCatch(Transform player)
    {
        if (player == null || !player.gameObject.activeInHierarchy) return false;
        float xzDist = Mathf.Sqrt(
            (player.position.x - transform.position.x) * (player.position.x - transform.position.x) +
            (player.position.z - transform.position.z) * (player.position.z - transform.position.z));
        if (xzDist <= catchRadius)
        {
            if (GameManager.Instance != null)
                GameManager.Instance.OnPlayerCaught();
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

    void OnDrawGizmos()
    {
        if (!showDetectionGizmos)
            return;

        // 房间范围
        Gizmos.color = new Color(0f, 0.5f, 1f, 0.15f);
        Gizmos.DrawCube(roomCenter, roomSize);
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(roomCenter, roomSize);

        // 抓捕范围
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, catchRadius);

        // 视野锥和最大检测范围
        Gizmos.color = fieldOfViewGizmoColor;
        Vector3 origin = transform.position + Vector3.up * 0.1f;
        Vector3 flatForward = transform.forward;
        flatForward.y = 0f;
        flatForward.Normalize();
        Vector3 leftDir = Quaternion.Euler(0f, -fieldOfView * 0.5f, 0f) * flatForward;
        Vector3 rightDir = Quaternion.Euler(0f, fieldOfView * 0.5f, 0f) * flatForward;
        Gizmos.DrawRay(origin, leftDir * detectionRange);
        Gizmos.DrawRay(origin, rightDir * detectionRange);

        const int arcSegments = 16;
        Vector3 previous = origin + leftDir * detectionRange;
        for (int i = 1; i <= arcSegments; i++)
        {
            float angle = Mathf.Lerp(-fieldOfView * 0.5f, fieldOfView * 0.5f, i / (float)arcSegments);
            Vector3 point = origin + Quaternion.Euler(0f, angle, 0f) * flatForward * detectionRange;
            Gizmos.DrawLine(previous, point);
            previous = point;
        }

        Gizmos.color = new Color(fieldOfViewGizmoColor.r, fieldOfViewGizmoColor.g, fieldOfViewGizmoColor.b, 0.25f);
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}
