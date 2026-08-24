#if UNITY_EDITOR
using Pathfinding;
using UnityEngine;

/// <summary>
/// Editor-only GM harness for the isolated door-navigation test scene.
/// Never referenced by production scenes or builds.
/// </summary>
public class DoorNavTestGm : MonoBehaviour
{
    [Header("Wiring (auto-filled on start)")]
    public Camera viewCamera;
    public Transform chaseTargetMarker;
    public Vector3 deepRoutePoint = new Vector3(-40f, 10f, -11f);

    [Header("Auto snap")]
    public bool snapToGround = true;
    public float snapProbeHeight = 6f;
    public LayerMask groundMask = ~0;

    FormalDoor[] doors;
    LevelMonsterNavigation[] navigations;
    MonsterPatrol[] patrols;
    GUIStyle labelStyle;
    GUIStyle headerStyle;

    void Start()
    {
        doors = FindObjectsOfType<FormalDoor>(true);
        navigations = FindObjectsOfType<LevelMonsterNavigation>();
        patrols = FindObjectsOfType<MonsterPatrol>();

        if (viewCamera == null)
            viewCamera = Camera.main;
        FrameCamera();

        if (chaseTargetMarker == null)
        {
            GameObject marker = GameObject.Find("GM_ChaseTarget");
            if (marker == null)
            {
                marker = new GameObject("GM_ChaseTarget");
                SnapToFloor(marker.transform);
            }
            chaseTargetMarker = marker.transform;
        }
    }

    void FrameCamera()
    {
        if (viewCamera == null || navigations == null || navigations.Length == 0)
            return;

        Vector3 sum = Vector3.zero;
        foreach (LevelMonsterNavigation nav in navigations)
            sum += nav.areaCenter;
        Vector3 center = sum / navigations.Length;

        viewCamera.transform.position = center + new Vector3(0f, 55f, -35f);
        viewCamera.transform.LookAt(center);
    }

    void SnapToFloor(Transform target)
    {
        if (!snapToGround || target == null)
            return;

        Vector3 origin = target.position + Vector3.up * snapProbeHeight;
        if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, snapProbeHeight * 2f, groundMask, QueryTriggerInteraction.Ignore))
        {
            Vector3 pos = target.position;
            pos.y = hit.point.y;
            target.position = pos;
        }
    }

    void Update()
    {
        HandleDoorKeys();

        if (Input.GetMouseButtonDown(1))
            OrderChaseToMouse();

        if (Input.GetKeyDown(KeyCode.F))
            OrderDeepRoutePursuit();

        if (Input.GetKeyDown(KeyCode.G))
            RescanAll();

        if (Input.GetKeyDown(KeyCode.P))
            ResetAll();
    }

    void HandleDoorKeys()
    {
        if (doors == null || doors.Length == 0)
            return;

        KeyCode[] keys =
        {
            KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3, KeyCode.Alpha4, KeyCode.Alpha5,
            KeyCode.Alpha6, KeyCode.Alpha7, KeyCode.Alpha8, KeyCode.Alpha9, KeyCode.Alpha0
        };

        int count = Mathf.Min(doors.Length, keys.Length);
        for (int i = 0; i < count; i++)
        {
            if (!Input.GetKeyDown(keys[i]) || doors[i] == null)
                continue;

            if (doors[i].IsOpen)
                doors[i].Close();
            else
                doors[i].Open();
        }
    }

    /// <summary>
    /// Snaps a world point to the closest walkable nav-grid node so test orders
    /// never land inside walls or eroded door frames.
    /// </summary>
    static bool TrySnapToWalkable(Vector3 point, out Vector3 snapped)
    {
        snapped = point;
        if (AstarPath.active == null)
            return false;

        NNInfo info = AstarPath.active.GetNearest(point, NearestNodeConstraint.Walkable);
        if (info.node == null || !info.node.Walkable)
            return false;

        snapped = (Vector3)info.node.position;
        return true;
    }

    void OrderChaseToMouse()
    {
        if (viewCamera == null)
            return;

        Ray ray = viewCamera.ScreenPointToRay(Input.mousePosition);
        Vector3 point;
        float fallbackPlaneY = navigations != null && navigations.Length > 0 ? navigations[0].areaCenter.y : 0f;
        Plane plane = new Plane(Vector3.up, new Vector3(0f, fallbackPlaneY, 0f));

        if (Physics.Raycast(ray, out RaycastHit hit, 500f, groundMask, QueryTriggerInteraction.Ignore))
            point = hit.point;
        else if (plane.Raycast(ray, out float enter))
            point = ray.GetPoint(enter);
        else
            return;

        if (!TrySnapToWalkable(point, out Vector3 snapped))
            return;

        if (chaseTargetMarker == null)
        {
            GameObject marker = new GameObject("GM_ChaseTarget");
            chaseTargetMarker = marker.transform;
        }

        chaseTargetMarker.position = snapped;
        SnapToFloor(chaseTargetMarker);
        OrderAllMonstersToMarker();
    }

    void OrderDeepRoutePursuit()
    {
        if (!TrySnapToWalkable(deepRoutePoint, out Vector3 snapped))
            return;

        if (chaseTargetMarker == null)
        {
            GameObject marker = new GameObject("GM_ChaseTarget");
            chaseTargetMarker = marker.transform;
        }

        chaseTargetMarker.position = snapped;
        SnapToFloor(chaseTargetMarker);
        OrderAllMonstersToMarker();
    }

    void OrderAllMonstersToMarker()
    {
        if (patrols == null || chaseTargetMarker == null)
            return;

        foreach (MonsterPatrol patrol in patrols)
            if (patrol != null)
                patrol.BeginForcedChase(chaseTargetMarker);
    }

    void RescanAll()
    {
        if (navigations == null)
            return;

        foreach (LevelMonsterNavigation nav in navigations)
            if (nav != null)
                nav.RescanGraph();
    }

    void ResetAll()
    {
        if (patrols == null)
            return;

        foreach (MonsterPatrol patrol in patrols)
            if (patrol != null)
                patrol.ResetPatrol();
    }

    void OnGUI()
    {
        if (labelStyle == null)
        {
            labelStyle = new GUIStyle(GUI.skin.label) { fontSize = 14 };
            headerStyle = new GUIStyle(GUI.skin.label) { fontSize = 16, fontStyle = FontStyle.Bold };
        }

        float x = 12f, y = 12f;

        GUI.Label(new Rect(x, y, 700, 24), "DOOR NAV TEST — 1~0: toggle doors | RightClick: all monsters chase point | F: deep-route pursuit | G: rescan | P: reset", headerStyle);
        y += 28f;

        if (doors != null)
        {
            GUI.Label(new Rect(x, y, 500, 22), "-- Doors --", headerStyle);
            y += 24f;
            for (int i = 0; i < doors.Length; i++)
            {
                if (doors[i] == null)
                    continue;
                string keyHint = i < 9 ? (i + 1).ToString() : "0";
                string state = doors[i].IsOpen ? "OPEN" : "CLOSED";
                GUI.Label(new Rect(x, y, 500, 20), string.Format("[{0}] {1} : {2}", keyHint, doors[i].name, state), labelStyle);
                y += 20f;
            }
        }

        y += 6f;
        GUI.Label(new Rect(x, y, 600, 22), "-- Monsters --", headerStyle);
        y += 24f;

        if (navigations != null)
        {
            for (int i = 0; i < navigations.Length; i++)
            {
                LevelMonsterNavigation nav = navigations[i];
                if (nav == null)
                    continue;

                string mode = nav.DynamicDoorNavigation ? "dynamic" : "STATIC";
                string pathState = nav.LastPathFailed ? "PATH FAILED" : (nav.HasArrived ? "arrived/idle" : "moving");
                GUI.Label(
                    new Rect(x, y, 650, 20),
                    string.Format("#{0} {1} | pos {2} | {3}", i + 1, mode, nav.transform.position.ToString("F1"), pathState),
                    labelStyle);
                y += 20f;
            }
        }

        if (patrols != null)
        {
            foreach (MonsterPatrol patrol in patrols)
            {
                if (patrol == null)
                    continue;
                GUI.Label(new Rect(x, y, 650, 20), string.Format("state={0} chasing={1}", patrol.IsChasing ? "Chase" : "Patrol", patrol.IsChasing), labelStyle);
                y += 20f;
            }
        }

        if (chaseTargetMarker != null)
        {
            GUI.Label(new Rect(x, y + 6f, 650, 20), string.Format("chase marker @ {0}", chaseTargetMarker.position.ToString("F1")), labelStyle);
        }
    }
}
#endif
