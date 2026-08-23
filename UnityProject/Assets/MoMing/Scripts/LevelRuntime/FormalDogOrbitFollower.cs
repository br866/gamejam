using Pathfinding;
using UnityEngine;

public class FormalDogOrbitFollower : MonoBehaviour
{
    [SerializeField] private float orbitRadius = 2f;
    [SerializeField] private float orbitSpeed = 1.6f;
    [SerializeField] private float moveSpeed = 4.5f;

    private FormalPlayerActor human;
    private FormalPlayerActor dog;
    private Seeker seeker;
    private Path currentPath;
    private int waypointIndex;
    private float nextRepathTime;
    private float orbitAngle;
    private bool active;

    public bool IsFollowing => active && human != null && dog != null;

    public void BeginOrbit(FormalPlayerActor humanActor, FormalPlayerActor dogActor)
    {
        human = humanActor;
        dog = dogActor;
        seeker = dog != null ? dog.GetComponent<Seeker>() : null;
        if (seeker == null && dog != null)
            seeker = dog.gameObject.AddComponent<Seeker>();
        orbitAngle = 0f;
        active = human != null && dog != null;
        currentPath = null;
        waypointIndex = 0;
        nextRepathTime = 0f;
    }

    public void StopOrbit()
    {
        active = false;
        human = null;
        dog = null;
        currentPath = null;
    }

    void Update()
    {
        if (!active || human == null || dog == null)
            return;

        orbitAngle += orbitSpeed * Time.deltaTime;
        Vector3 offset = new Vector3(Mathf.Cos(orbitAngle), 0f, Mathf.Sin(orbitAngle)) * orbitRadius;
        Vector3 target = human.transform.position + offset;
        target.y = dog.transform.position.y;

        if (seeker != null && HasUsableGraph() && Time.time >= nextRepathTime)
        {
            nextRepathTime = Time.time + 0.25f;
            seeker.StartPath(dog.transform.position, target, OnPathComplete);
        }

        Vector3 moveTarget = target;
        if (currentPath != null && !currentPath.error && currentPath.vectorPath != null && currentPath.vectorPath.Count > 0)
        {
            while (waypointIndex < currentPath.vectorPath.Count - 1 &&
                   FlatDistance(dog.transform.position, currentPath.vectorPath[waypointIndex]) < 0.35f)
                waypointIndex++;

            moveTarget = currentPath.vectorPath[waypointIndex];
            moveTarget.y = dog.transform.position.y;
        }

        dog.transform.position = Vector3.MoveTowards(dog.transform.position, moveTarget, moveSpeed * Time.deltaTime);

        Vector3 look = human.transform.position - dog.transform.position;
        look.y = 0f;
        if (look.sqrMagnitude > 0.01f)
            dog.transform.rotation = Quaternion.Slerp(dog.transform.rotation, Quaternion.LookRotation(look), 8f * Time.deltaTime);
    }

    void OnPathComplete(Path path)
    {
        currentPath = path;
        waypointIndex = 0;
    }

    static float FlatDistance(Vector3 first, Vector3 second)
    {
        first.y = 0f;
        second.y = 0f;
        return Vector3.Distance(first, second);
    }

    static bool HasUsableGraph()
    {
        return AstarPath.active != null &&
            AstarPath.active.data != null &&
            AstarPath.active.data.graphs != null &&
            AstarPath.active.data.graphs.Length > 0;
    }
}
