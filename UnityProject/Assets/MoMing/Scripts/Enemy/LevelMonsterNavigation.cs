using Pathfinding;
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

    private Seeker seeker;
    private GridGraph graph;
    private Path currentPath;
    private Vector3 destination;
    private int waypointIndex;
    private float nextRepathTime;
    private bool hasDestination;
    private bool pathPending;

    public bool HasArrived { get; private set; }
    public bool HasPath { get { return currentPath != null && !currentPath.error; } }

    void Awake()
    {
        seeker = GetComponent<Seeker>();
        if (seeker == null)
            seeker = gameObject.AddComponent<Seeker>();
    }

    void Start()
    {
        EnsureGraph();
    }

    void Update()
    {
        if (!hasDestination || pathPending || currentPath == null || currentPath.error)
            return;

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

    void EnsureGraph()
    {
        if (AstarPath.active == null)
        {
            var graphObject = new GameObject("Level2MonsterAstar");
            AstarPath.active = graphObject.AddComponent<AstarPath>();
        }

        if (AstarPath.active.data.graphs != null)
        {
            foreach (var existing in AstarPath.active.data.graphs)
            {
                var existingGrid = existing as GridGraph;
                if (existingGrid != null && existingGrid.name == "Level2MonsterGraph")
                {
                    graph = existingGrid;
                    return;
                }
            }
        }

        graph = AstarPath.active.data.AddGraph<GridGraph>();
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
    }

    void RequestPath()
    {
        if (seeker == null || !hasDestination)
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
        HasArrived = path == null || path.error;
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
