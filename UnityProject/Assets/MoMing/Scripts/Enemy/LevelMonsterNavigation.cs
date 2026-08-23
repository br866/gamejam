using Pathfinding;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelMonsterNavigation : MonoBehaviour
{
    [Header("Bounded navigation")]
    public Vector3 areaCenter = new Vector3(-1f, 12.1f, 0f);
    public Vector3 areaSize = new Vector3(27f, 0f, 20f);
    public float nodeSize = 0.5f;
    public float actorDiameter = 1f;
    public float actorHeight = 2f;
    public float repathInterval = 0.25f;
    public float moveSpeed = 4.5f;
    public float arrivalDistance = 1f;

    [Header("Dynamic doors")]
    [SerializeField] private bool dynamicDoorNavigation = false;
    [SerializeField] private bool inheritRoomBoundsFromPatrol = true;
    [SerializeField] private float pushOutDuration = 0.2f;

    [Header("Area Gizmos")]
    public bool showAreaBounds = true;
    public Color areaBoundsGizmoColor = new Color(1f, 0.5f, 0f, 0.9f);

    void OnDrawGizmos()
    {
        if (!showAreaBounds)
            return;

        Vector3 center = new Vector3(areaCenter.x, areaCenter.y, areaCenter.z);
        Vector3 size = new Vector3(areaSize.x, 0.05f, areaSize.z);
        Gizmos.color = areaBoundsGizmoColor;
        Gizmos.DrawWireCube(center, size);
    }

    private Seeker seeker;
    private GridGraph graph;
    private Path currentPath;
    private Vector3 destination;
    private int waypointIndex;
    private float nextRepathTime;
    private bool hasDestination;
    private bool pathPending;
    private bool pushingOut;
    private readonly List<FormalDoor> trackedDoors = new List<FormalDoor>();
    private readonly Dictionary<FormalDoor, Bounds> trackedDoorBounds = new Dictionary<FormalDoor, Bounds>();
    private Coroutine pushOutRoutine;

    public bool HasArrived { get; private set; }
    public bool HasPath { get { return currentPath != null && !currentPath.error; } }
    public bool DynamicDoorNavigation { get { return dynamicDoorNavigation; } }
    public bool LastPathFailed { get; private set; }

    void Awake()
    {
        MonsterPatrol patrol = GetComponent<MonsterPatrol>();
        if (patrol != null && inheritRoomBoundsFromPatrol)
        {
            areaCenter = patrol.roomCenter;
            areaSize = patrol.roomSize;
        }

        seeker = GetComponent<Seeker>();
        if (seeker == null)
            seeker = gameObject.AddComponent<Seeker>();
    }

    void Start()
    {
        StartCoroutine(InitializeNavigation());
    }

    IEnumerator InitializeNavigation()
    {
        // A* 5.x initializes AstarPath asynchronously; AddGraph issued the same
        // frame the component is created gets dropped, so retry until it sticks.
        while (!EnsureGraph())
            yield return null;

        if (dynamicDoorNavigation)
            TrackDoors();

        ForceRepath();
    }

    void OnDestroy()
    {
        foreach (FormalDoor door in trackedDoors)
            if (door != null)
                door.StateChanged -= OnTrackedDoorStateChanged;
    }

    void Update()
    {
        if (!hasDestination || pathPending || pushingOut)
            return;

        if (currentPath == null || currentPath.error)
        {
            // Failed paths are retried on the normal repath timer instead of stalling forever.
            if (Time.time >= nextRepathTime)
                RequestPath();
            return;
        }

        if (Time.time >= nextRepathTime)
            RequestPath();

        FollowPath();
    }

    public void SetDestination(Vector3 target)
    {
        target.y = transform.position.y;
        target = ClampToArea(target);
        if (hasDestination && (destination - target).sqrMagnitude < 0.01f)
            return;

        destination = target;
        hasDestination = true;
        HasArrived = false;
        RequestPath();
    }

    public void ClearDestination()
    {
        hasDestination = false;
        pathPending = false;
        currentPath = null;
        HasArrived = false;
    }

    public void SetMoveSpeed(float speed)
    {
        moveSpeed = Mathf.Max(0f, speed);
    }

    public void ClearPathFailure()
    {
        LastPathFailed = false;
    }

    public void RescanGraph()
    {
        if (AstarPath.active != null && graph != null)
            AstarPath.active.Scan(graph);
    }

    bool EnsureGraph()
    {
        if (AstarPath.active == null)
        {
            var graphObject = new GameObject("Level2MonsterAstar");
            AstarPath.active = graphObject.AddComponent<AstarPath>();
            return false;
        }

        if (AstarPath.active.data.graphs != null)
        {
            foreach (var existing in AstarPath.active.data.graphs)
            {
                var existingGrid = existing as GridGraph;
                if (existingGrid != null && existingGrid.name == "Level2MonsterGraph")
                {
                    graph = existingGrid;
                    return true;
                }
            }
        }

        GridGraph added = AstarPath.active.data.AddGraph<GridGraph>();
        if (added == null)
            return false;

        graph = added;
        graph.name = "Level2MonsterGraph";
        graph.center = areaCenter;
        graph.SetDimensions(
            Mathf.Max(1, Mathf.RoundToInt(areaSize.x / nodeSize)),
            Mathf.Max(1, Mathf.RoundToInt(areaSize.z / nodeSize)),
            nodeSize);
        graph.collision.collisionCheck = true;
        graph.collision.type = Pathfinding.Graphs.Grid.ColliderType.Capsule;
        graph.collision.diameter = actorDiameter / nodeSize;
        graph.collision.height = actorHeight;
        graph.collision.mask = LayerMask.GetMask("NavStatic", "NavDynamic");
        graph.collision.heightCheck = false;
        AstarPath.active.Scan(graph);
        return true;
    }

    void TrackDoors()
    {
        FormalDoor[] allDoors = FindObjectsOfType<FormalDoor>(true);
        Bounds areaBounds = GetAreaBoundsXZ();

        foreach (FormalDoor door in allDoors)
        {
            if (door == null || trackedDoors.Contains(door))
                continue;

            Collider collider = door.BlockingCollider;
            if (collider == null)
                continue;

            // Measure while the collider is enabled (disabled colliders report
            // zero-size bounds). Doors start closed, so this holds at subscribe time.
            Bounds doorBounds = collider.bounds;
            if (doorBounds.size.sqrMagnitude < 0.001f)
                continue;

            bool overlapsArea =
                doorBounds.min.x <= areaBounds.max.x && doorBounds.max.x >= areaBounds.min.x &&
                doorBounds.min.z <= areaBounds.max.z && doorBounds.max.z >= areaBounds.min.z;
            if (!overlapsArea)
                continue;

            trackedDoors.Add(door);
            trackedDoorBounds[door] = doorBounds;
            door.StateChanged += OnTrackedDoorStateChanged;
        }
    }

    Bounds GetAreaBoundsXZ()
    {
        return new Bounds(
            new Vector3(areaCenter.x, 0f, areaCenter.z),
            new Vector3(Mathf.Max(0f, areaSize.x), 1000f, Mathf.Max(0f, areaSize.z)));
    }

    void OnTrackedDoorStateChanged(FormalDoor door)
    {
        if (AstarPath.active == null || graph == null || door == null)
            return;

        // Use the bounds captured at subscribe time: the live collider may be
        // disabled (open door) and disabled colliders report zero-size bounds.
        if (!trackedDoorBounds.TryGetValue(door, out Bounds sourceBounds))
            return;

        // Expand by the actor radius so reopening also clears the erosion halo
        // the grid scan baked around the closed door leaf; otherwise the gap
        // stays choked by unwalkable border nodes even with the door gone.
        float margin = actorDiameter * 0.5f + nodeSize * 0.5f;
        Bounds region = sourceBounds;
        region.Expand(new Vector3(margin, 0f, margin));
        // Make sure the region reaches the graph plane vertically regardless of door height.
        region.Encapsulate(new Vector3(region.center.x, areaCenter.y, region.center.z));

        // Instant-close policy: raise the collider right away so the physics
        // recalculation below can see the closed door without waiting for the
        // swing animation to finish. The visual keeps animating independently.
        Collider collider = door.BlockingCollider;
        if (!door.IsOpen && collider != null && !collider.enabled)
            collider.enabled = true;

        var guo = new GraphUpdateObject(region)
        {
            updatePhysics = true
        };
        AstarPath.active.UpdateGraphs(guo);
        AstarPath.active.FlushGraphUpdates();

        ForceRepath();

        if (!door.IsOpen)
            StartPushOutIfNeeded();
    }

    void ForceRepath()
    {
        if (!hasDestination)
            return;

        currentPath = null;
        pathPending = false;
        RequestPath();
    }

    void StartPushOutIfNeeded()
    {
        if (pushOutRoutine != null)
            return;

        NNInfo nearest = AstarPath.active.GetNearest(transform.position);
        if (nearest.node == null || nearest.node.Walkable)
            return;

        pushOutRoutine = StartCoroutine(PushOutToNearestWalkable());
    }

    IEnumerator PushOutToNearestWalkable()
    {
        pushingOut = true;

        NNInfo nearest = AstarPath.active.GetNearest(transform.position, NearestNodeConstraint.Walkable);

        Vector3 start = transform.position;
        Vector3 target = nearest.node != null ? (Vector3)nearest.node.position : start;
        target.y = start.y;

        float elapsed = 0f;
        while (elapsed < pushOutDuration)
        {
            elapsed += Time.deltaTime;
            transform.position = Vector3.Lerp(start, target, Mathf.Clamp01(elapsed / pushOutDuration));
            yield return null;
        }
        transform.position = target;

        pushingOut = false;
        pushOutRoutine = null;
        ForceRepath();
    }

    public void CancelPushOut()
    {
        if (pushOutRoutine != null)
        {
            StopCoroutine(pushOutRoutine);
            pushOutRoutine = null;
        }
        pushingOut = false;
    }

    void RequestPath()
    {
        if (seeker == null || !hasDestination || graph == null)
            return;

        pathPending = true;
        nextRepathTime = Time.time + repathInterval;
        seeker.StartPath(transform.position, destination, OnPathComplete);
    }

    void OnPathComplete(Path path)
    {
        pathPending = false;
        currentPath = path;
        waypointIndex = 0;

        bool failed = path == null || path.error;
        var abPath = path as ABPath;
        if (!failed && abPath != null && abPath.CompleteState == Pathfinding.PathCompleteState.Partial
            && abPath.vectorPath != null && abPath.vectorPath.Count > 0)
        {
            // A partial path that goes nowhere (endpoint barely past the start)
            // means the destination is effectively unreachable.
            float progress = Vector3.Distance(
                abPath.vectorPath[abPath.vectorPath.Count - 1],
                abPath.vectorPath[0]);
            if (progress < arrivalDistance * 2f)
                failed = true;
        }

        LastPathFailed = failed;
        HasArrived = failed;
    }

    void FollowPath()
    {
        if (currentPath == null || currentPath.error || currentPath.vectorPath == null)
            return;

        while (waypointIndex < currentPath.vectorPath.Count - 1 &&
               FlatDistance(transform.position, currentPath.vectorPath[waypointIndex]) < 0.35f)
            waypointIndex++;

        Vector3 target = currentPath.vectorPath[waypointIndex];
        target.y = transform.position.y;
        Vector3 delta = target - transform.position;
        delta.y = 0f;

        if (Vector3.Distance(transform.position, destination) <= arrivalDistance)
        {
            HasArrived = true;
            return;
        }

        if (delta.sqrMagnitude > 0.001f)
        {
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                Quaternion.LookRotation(delta.normalized),
                8f * Time.deltaTime);
            transform.position = Vector3.MoveTowards(
                transform.position,
                target,
                moveSpeed * Time.deltaTime);
        }
    }

    Vector3 ClampToArea(Vector3 value)
    {
        Vector3 min = areaCenter - areaSize * 0.5f;
        Vector3 max = areaCenter + areaSize * 0.5f;
        value.x = Mathf.Clamp(value.x, min.x, max.x);
        value.z = Mathf.Clamp(value.z, min.z, max.z);
        return value;
    }

    static float FlatDistance(Vector3 a, Vector3 b)
    {
        a.y = 0f;
        b.y = 0f;
        return Vector3.Distance(a, b);
    }
}
