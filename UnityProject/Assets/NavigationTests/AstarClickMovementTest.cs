using Pathfinding;
using UnityEngine;

public class AstarClickMovementTest : MonoBehaviour
{
    [SerializeField] private AIPath agent;
    [SerializeField] private Transform targetMarker;
    [SerializeField] private Camera testCamera;
    [SerializeField] private LayerMask clickMask = 1 << 2;

    private GridGraph graph;
    private Transform obstacle;
    private Collider obstacleCollider;
    private Vector3 initialObstaclePosition;
    private bool obstacleMoved;

    private void Start()
    {
        if (agent == null)
            agent = GameObject.Find("AstarTestAgent").GetComponent<AIPath>();
        if (targetMarker == null)
            targetMarker = GameObject.Find("AstarTestTarget").transform;
        if (testCamera == null)
            testCamera = Camera.main;

        agent.enabled = false;
        agent.transform.position = new Vector3(-10f, 0f, 0f);
        var controller = agent.GetComponent<CharacterController>();
        controller.radius = 0.5f;
        controller.height = 2f;
        controller.center = new Vector3(0f, 1f, 0f);

        obstacle = GameObject.Find("AstarTestObstacle").transform;
        obstacleCollider = obstacle.GetComponent<Collider>();
        initialObstaclePosition = obstacle.position;

        graph = AstarPath.active.data.AddGraph<GridGraph>();
        // Keep the graph above the ground collider so only raised obstacles block nodes.
        graph.center = new Vector3(0f, 1f, 0f);
        graph.SetDimensions(60, 40, 0.5f);
        graph.collision.collisionCheck = false;
        graph.collision.heightCheck = false;
        AstarPath.active.Scan();

        UpdateObstacleWalkability(obstacleCollider.bounds, false);

        agent.enabled = true;
        SetDestination(targetMarker.position);
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0) && Physics.Raycast(testCamera.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, 100f, clickMask))
            SetDestination(hit.point);

        if (Input.GetKeyDown(KeyCode.O))
            ToggleObstacle();
    }

    private void SetDestination(Vector3 position)
    {
        position.y = targetMarker.position.y;
        targetMarker.position = position;
        agent.destination = position;
        agent.SearchPath();
    }

    private void ToggleObstacle()
    {
        var oldBounds = obstacleCollider.bounds;
        UpdateObstacleWalkability(oldBounds, true);

        obstacleMoved = !obstacleMoved;
        obstacle.position = obstacleMoved
            ? initialObstaclePosition + new Vector3(0f, 0f, 5f)
            : initialObstaclePosition;

        UpdateObstacleWalkability(obstacleCollider.bounds, false);
        agent.SearchPath();
    }

    private static void UpdateObstacleWalkability(Bounds bounds, bool walkable)
    {
        var update = new GraphUpdateObject(bounds)
        {
            modifyWalkability = true,
            setWalkability = walkable
        };
        AstarPath.active.UpdateGraphs(update);
        AstarPath.active.FlushGraphUpdates();
    }
}
