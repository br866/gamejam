using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(AkGameObj))]
public sealed class FormalMechanismAudioEmitter : MonoBehaviour
{
    private FormalDoor door;
    private GateController gate;
    private Vector3 previousPosition;
    private Vector3 previousEuler;
    private bool initialized;
    public bool MetalDoor { get; private set; }
    private bool moving;
    private float stillTime;
    private uint movementId;
    private uint releaseId;

    public void Initialize(FormalDoor d, GateController g)
    {
        door = d; gate = g; Snapshot(); initialized = true;
        MetalDoor = false;
        foreach (var renderer in GetComponentsInChildren<Renderer>())
        {
            string n = renderer.name.ToLowerInvariant();
            foreach (var material in renderer.sharedMaterials) if (material != null) n += " " + material.name.ToLowerInvariant();
            if (n.Contains("door2") || n.Contains("door3")) MetalDoor = true;
        }
    }
    private Transform Pivot => door != null && door.VisualPivot != null ? door.VisualPivot : transform;
    private void Snapshot() { previousPosition = transform.position; previousEuler = Pivot.localEulerAngles; }
    private void LateUpdate()
    {
        if (!initialized) return;
        float distance = (transform.position - previousPosition).magnitude;
        Vector3 euler = Pivot.localEulerAngles;
        // Quaternion.Angle loses tiny rotations to floating-point rounding at high frame rates.
        float angle = new Vector3(Mathf.DeltaAngle(previousEuler.x, euler.x),
            Mathf.DeltaAngle(previousEuler.y, euler.y), Mathf.DeltaAngle(previousEuler.z, euler.z)).magnitude;
        Snapshot();
        if (!FormalGameplayState.CanSimulate || distance > Mathf.Max(2f, 12f * Time.deltaTime) || angle > 80f)
        {
            StopPlayback();
            return;
        }
        bool changed = gate != null ? distance > Mathf.Max(0.000001f, 0.002f * Time.deltaTime)
            : angle > Mathf.Max(0.00001f, 0.5f * Time.deltaTime);
        if (changed)
        {
            stillTime = 0;
            if (!moving)
            {
                FormalSfxEvents.Stop(ref releaseId);
                movementId = FormalSfxEvents.Post(gate != null ? "Play_Gate_Drive" : MetalDoor ? "Play_DoorMetal_Move" : "Play_Door_Move", gameObject);
                moving = movementId != 0;
            }
        }
        else if (moving)
        {
            stillTime += Time.deltaTime;
            if (stillTime < 0.06f) return;
            FormalSfxEvents.Stop(ref movementId);
            if (gate != null || (door != null && !door.IsOpen))
                releaseId = FormalSfxEvents.Post(gate != null ? "Play_Gate_Stop" : MetalDoor ? "Play_DoorMetal_Close" : "Play_Door_Close", gameObject);
            moving = false;
        }
    }
    private void StopPlayback()
    {
        FormalSfxEvents.Stop(ref movementId);
        FormalSfxEvents.Stop(ref releaseId);
        moving = false;
        stillTime = 0;
    }
    private void OnDisable() => StopPlayback();
    private void OnEnable() { if (initialized) Snapshot(); }
    private void OnDestroy() => StopPlayback();
}
