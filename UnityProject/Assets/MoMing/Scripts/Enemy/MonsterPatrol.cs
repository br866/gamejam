using UnityEngine;

/// <summary>
/// 怪物：仅在自己房间内活动。玩家进入房间时触发检测(锥形视野+听觉)，追击也限定在房间内。
/// 玩家离开房间后怪物丢失目标，回到巡逻。
/// </summary>
public class MonsterPatrol : MonoBehaviour
{
    public enum State { Patrol, Chase, Attack }

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

    [Header("Attack")]
    public float attackRange = 2f;
    public float attackWindup = 0.5f;
    public float attackCooldown = 1f;
    [SerializeField] private string attackStateName = "";

    [Header("Safe Zones")]
    [SerializeField] private Collider[] safeZones;

    [Header("Audio")]
    [SerializeField] private AK.Wwise.Event footstepEvent;
    [SerializeField, Min(0.1f)] private float patrolFootstepDistance = 1.8f;
    [SerializeField, Min(0.1f)] private float chaseFootstepDistance = 2.2f;
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
    private MonsterAnimatorDriver animatorDriver;
    private Vector3 startPos;
    private Transform chaseTarget;
    private Transform forcedHumanTarget;
    private Transform forcedDogTarget;
    private float lastSeenTime;
    private bool forcedChase;
    private float attackTimer;
    private float nextAttackTime;
    private float forcedRepathCooldown;
    private Vector3 lastFootstepPosition;
    private bool hasWarnedMissingFootstepEvent;
    private FormalPlayerActor executionTarget;

    void Awake()
    {
        if (!HasAssignedWaypoints())
        {
            enabled = false;
            return;
        }

        audioSource = GetComponent<AudioSource>();
        navigation = GetComponent<LevelMonsterNavigation>();
        animatorDriver = GetComponent<MonsterAnimatorDriver>();
        startPos = transform.position;
        lastFootstepPosition = startPos;
        foreach (Renderer r in GetComponentsInChildren<Renderer>())
            if (r.enabled && r.gameObject.activeInHierarchy)
            {
                visualRenderer = r;
                break;
            }
        ResolveFormalSafeZones();
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
        ReleaseExecutionTarget();
        if (GameManager.Instance != null)
            GameManager.Instance.OnLevelReset -= ResetPatrol;
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

    void Update()
    {
        if (currentState == State.Attack)
        {
            UpdateAttack();
            return;
        }

        if (forcedChase)
        {
            ForcedChase();
            HandleFootsteps();
            TryCatch(chaseTarget, false);
            return;
        }

        if (currentState == State.Patrol)
        {
            Patrol();
        }
        else if (currentState == State.Chase)
        {
            Chase();
        }

        HandleFootsteps();
        CheckForPlayers();
    }

    void HandleFootsteps()
    {
        Vector3 current = transform.position;
        Vector3 previous = lastFootstepPosition;
        current.y = 0f;
        previous.y = 0f;

        float requiredDistance = forcedChase || currentState == State.Chase
            ? chaseFootstepDistance
            : patrolFootstepDistance;
        if ((current - previous).sqrMagnitude < requiredDistance * requiredDistance)
            return;

        lastFootstepPosition = transform.position;
        if (footstepEvent != null && footstepEvent.IsValid())
        {
            footstepEvent.Post(gameObject);
        }
        else if (!hasWarnedMissingFootstepEvent)
        {
            Debug.LogWarning("MonsterPatrol: Footstep Event is not assigned.", this);
            hasWarnedMissingFootstepEvent = true;
        }
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
        float xzDist = HorizontalDistance(transform.position, chaseTarget.position);

        if (xzDist <= attackRange && Time.time >= nextAttackTime && HasLineOfSight(chaseTarget))
        {
            BeginAttack(chaseTarget);
            return;
        }

        if (navigation != null)
        {
            // 寻路失败(如门关闭隔断) → 回到巡逻点保持移动，不石化
            if (navigation.LastPathFailed)
            {
                chaseTarget = null;
                currentState = State.Patrol;
                Debug.Log("[Monster] No path to target, returning to patrol waypoints.");
                return;
            }

            navigation.SetMoveSpeed(chaseSpeed);
            navigation.SetDestination(targetPos);
            if (xzDist <= chaseStopDistance)
                navigation.ClearDestination();
            return;
        }

        if (xzDist > chaseStopDistance)
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

    public void BeginForcedChase(Transform target)
    {
        if (target == null)
            return;

        forcedHumanTarget = null;
        forcedDogTarget = null;

        BeginForcedChaseInternal(target);
    }

    public void BeginForcedChase(Transform humanTarget, Transform dogTarget)
    {
        forcedHumanTarget = humanTarget;
        forcedDogTarget = dogTarget;

        Transform target = SelectForcedChaseTarget();
        if (target == null)
            return;

        BeginForcedChaseInternal(target);
    }

    void BeginForcedChaseInternal(Transform target)
    {

        if (navigation != null)
        {
            navigation.ClearDestination();
            navigation.ClearPathFailure();
            navigation.RescanGraph();
        }

        forcedChase = true;
        forcedRepathCooldown = 0f;
        chaseTarget = target;
        currentState = State.Chase;
        lastSeenTime = Time.time;
    }

    void ForcedChase()
    {
        Transform nearestTarget = SelectForcedChaseTarget();
        if (nearestTarget != null)
            chaseTarget = nearestTarget;

        if (chaseTarget == null || !chaseTarget.gameObject.activeInHierarchy)
            return;

        Vector3 targetPos = chaseTarget.position;
        float xzDist = HorizontalDistance(transform.position, targetPos);

        if (xzDist <= attackRange && Time.time >= nextAttackTime && HasLineOfSight(chaseTarget))
        {
            BeginAttack(chaseTarget);
            return;
        }

        if (xzDist <= chaseStopDistance)
            return;

        if (navigation != null)
        {
            // 寻路失败(门关闭/无连通) → 临时奔向巡逻点保持跑动，稍后重试追击
            if (navigation.LastPathFailed)
            {
                SteerForcedChaseToWaypoint();
                return;
            }

            if (forcedRepathCooldown > 0f)
            {
                forcedRepathCooldown -= Time.deltaTime;
                return;
            }

            navigation.SetMoveSpeed(chaseSpeed);
            navigation.SetDestination(targetPos);
            return;
        }

        // 无导航组件时的直线兜底
        targetPos.y = transform.position.y;
        Vector3 delta = targetPos - transform.position;
        delta.y = 0f;

        Vector3 direction = delta.normalized;
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            Quaternion.LookRotation(direction),
            8f * Time.deltaTime);
        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPos,
            chaseSpeed * Time.deltaTime);
    }

    Transform SelectForcedChaseTarget()
    {
        bool humanValid = forcedHumanTarget != null && forcedHumanTarget.gameObject.activeInHierarchy;
        bool dogValid = forcedDogTarget != null && forcedDogTarget.gameObject.activeInHierarchy;
        if (!humanValid && !dogValid)
            return chaseTarget;
        if (!humanValid)
            return forcedDogTarget;
        if (!dogValid)
            return forcedHumanTarget;

        float humanDistance = HorizontalDistance(transform.position, forcedHumanTarget.position);
        float dogDistance = HorizontalDistance(transform.position, forcedDogTarget.position);
        const float tieTolerance = 0.1f;
        if ((chaseTarget == forcedHumanTarget || chaseTarget == forcedDogTarget) &&
            Mathf.Abs(humanDistance - dogDistance) <= tieTolerance)
            return chaseTarget;

        return humanDistance <= dogDistance ? forcedHumanTarget : forcedDogTarget;
    }

    void SteerForcedChaseToWaypoint()
    {
        if (!HasAssignedWaypoints())
        {
            navigation.ClearDestination();
            return;
        }

        Transform wp = waypoints[currentWaypoint];
        Vector3 waypointPos = new Vector3(wp.position.x, startPos.y, wp.position.z);

        if (navigation.HasArrived || !navigation.HasPath)
        {
            currentWaypoint = (currentWaypoint + 1) % waypoints.Length;
            wp = waypoints[currentWaypoint];
            waypointPos = new Vector3(wp.position.x, startPos.y, wp.position.z);
        }

        navigation.SetMoveSpeed(chaseSpeed);
        navigation.SetDestination(waypointPos);
        forcedRepathCooldown = 1f;
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
        return TryCatch(player, true);
    }

    bool TryCatch(Transform player, bool requireLineOfSight)
    {
        if (player == null || !player.gameObject.activeInHierarchy) return false;
        if (requireLineOfSight && !HasLineOfSight(player)) return false;
        float xzDist = Mathf.Sqrt(
            (player.position.x - transform.position.x) * (player.position.x - transform.position.x) +
            (player.position.z - transform.position.z) * (player.position.z - transform.position.z));
        if (xzDist <= catchRadius)
        {
            BeginAttack(player);
            return true;
        }
        return false;
    }

    void BeginAttack(Transform target)
    {
        if (target == null || currentState == State.Attack || Time.time < nextAttackTime)
            return;

        if (IsInSafeZone(target.position))
            return;

        executionTarget = target.GetComponentInParent<FormalPlayerActor>();
        if (executionTarget != null)
            executionTarget.AcquireExecutionLock();

        chaseTarget = target;
        currentState = State.Attack;
        attackTimer = Mathf.Max(0f, attackWindup);

        Debug.Log("[Monster][ATK] begin windup=" + attackWindup + "s target=" + target.name);

        if (navigation != null)
            navigation.ClearDestination();
        FaceTarget(target);

        if (animatorDriver != null && !string.IsNullOrEmpty(attackStateName))
            animatorDriver.PlayLockedState(attackStateName, attackWindup + 0.25f);
    }

    void UpdateAttack()
    {
        if (!IsAttackTargetValid(chaseTarget))
        {
            Debug.Log("[Monster][ATK] cancel reason=target invalid pos=" +
                      (chaseTarget != null ? chaseTarget.position.ToString("F1") : "null"));
            CancelAttack();
            return;
        }

        FaceTarget(chaseTarget);

        attackTimer -= Time.deltaTime;
        if (attackTimer > 0f)
            return;

        Transform target = chaseTarget;
        EndAttack();
        chaseTarget = null;
        currentState = State.Patrol;

        ExecuteCatch();
        Debug.Log("[Monster] Attack hit " + (target != null ? target.name : "target") + ".");
    }

    void CancelAttack()
    {
        EndAttack();
        if (forcedChase)
            return;

        chaseTarget = null;
        currentState = State.Patrol;
        Debug.Log("[Monster] Attack cancelled, returning to patrol.");
    }

    void EndAttack()
    {
        if (animatorDriver != null)
            animatorDriver.ClearAnimationLock();
        ReleaseExecutionTarget();
        nextAttackTime = Time.time + Mathf.Max(0f, attackCooldown);
    }

    bool IsAttackTargetValid(Transform target)
    {
        if (target == null || !target.gameObject.activeInHierarchy) return false;
        return true;
    }

    void ReleaseExecutionTarget()
    {
        if (executionTarget == null)
            return;

        executionTarget.ReleaseExecutionLock();
        executionTarget = null;
    }

    static float HorizontalDistance(Vector3 from, Vector3 to)
    {
        float x = to.x - from.x;
        float z = to.z - from.z;
        return Mathf.Sqrt(x * x + z * z);
    }

    void FaceTarget(Transform target)
    {
        Vector3 dir = target.position - transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.0001f)
            return;
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            Quaternion.LookRotation(dir),
            10f * Time.deltaTime);
    }

    void ExecuteCatch()
    {
        Debug.Log("[Monster][ATK] windup done, executing catch");

        // 正式关卡：先弹死亡画面。Trigger 内部会挡住重复触发，
        // 所以不用担心怪物每帧都调一次。旧场景没有这个组件，会自动落到下面的老逻辑。
        if (FormalDeathScreen.Trigger(FormalDeathScreen.DeathCause.Caught))
        {
            PlayAudio(catchClip);
            return;
        }

        FormalGameFlowController flow = UnityEngine.Object.FindObjectOfType<FormalGameFlowController>();
        if (flow != null)
            flow.ResetCurrentLevel();
        else if (GameManager.Instance != null)
            GameManager.Instance.OnPlayerCaught();
        else
            Debug.LogError("[MonsterPatrol] Formal monster capture requires FormalGameFlowController for shared recovery.", this);
        PlayAudio(catchClip);
    }

    public void ResetPatrol()
    {
        forcedChase = false;
        forcedHumanTarget = null;
        forcedDogTarget = null;
        forcedRepathCooldown = 0f;
        if (navigation != null)
        {
            navigation.CancelPushOut();
            navigation.ClearDestination();
            navigation.ClearPathFailure();
        }
        currentState = State.Patrol;
        chaseTarget = null;
        currentWaypoint = 0;
        transform.position = startPos;
        lastFootstepPosition = startPos;
    }

    bool IsInSafeZone(Vector3 position)
    {
        if (safeZones == null)
            return false;

        foreach (Collider safeZone in safeZones)
            if (safeZone != null && safeZone.bounds.Contains(position))
                return true;

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
