using UnityEngine;

public class FormalPlayerControl : MonoBehaviour
{
    [SerializeField] private FormalPlayerActor human;
    [SerializeField] private FormalPlayerActor dog;
    private FormalPlayerActor activeActor;
    private CameraFollow cameraFollow;

    public bool IsDogActive => activeActor != null && activeActor.Role == FormalPlayerActor.ActorRole.Dog;

    void Start()
    {
        activeActor = human;
        cameraFollow = FindObjectOfType<CameraFollow>();
        if (cameraFollow == null)
        {
            GameObject cameraObject = new GameObject("FormalMainCamera");
            cameraObject.tag = "MainCamera";
            cameraObject.AddComponent<Camera>();
            cameraFollow = cameraObject.AddComponent<CameraFollow>();
            cameraFollow.enableMouseRotation = true;
        }

        SetCameraTarget();
    }

    void Update()
    {
        if (human == null)
            return;

        if (dog != null && Input.GetKeyDown(KeyCode.Tab) && !IsMoverEngaged())
        {
            activeActor = activeActor == human ? dog : human;
            SetCameraTarget();
        }

        if (Input.GetKeyDown(KeyCode.F))
            ToggleMoverEngagement();

        if (Input.GetKeyDown(KeyCode.Space) && !IsMoverEngaged())
            activeActor.Jump();
    }

    void FixedUpdate()
    {
        if (activeActor == null)
            return;

        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        Vector3 input = new Vector3(horizontal, 0f, vertical).normalized;
        Vector3 direction = CameraRelativeDirection(input);
        FormalCooperativeRailMover mover = FindEngagedMover();
        if (mover != null)
        {
            if (Input.GetKey(KeyCode.W))
                mover.SetAttachedPushAnimation();
            else if (Input.GetKey(KeyCode.S))
                mover.SetAttachedPullAnimation();
            else
                mover.SetAttachedIdleAnimation();

            Vector3 moverDirection = activeActor.transform.forward * vertical;
            mover.Move(moverDirection, vertical > 0.01f);
            return;
        }

        bool sprint = activeActor.Role == FormalPlayerActor.ActorRole.Dog && Input.GetKey(KeyCode.LeftShift);
        activeActor.Move(direction, sprint);
    }

    void ToggleMoverEngagement()
    {
        foreach (FormalCooperativeRailMover mover in FindObjectsOfType<FormalCooperativeRailMover>())
        {
            if (mover.IsAttached(activeActor))
            {
                mover.Cancel();
                return;
            }

            if (mover.TryEngage(activeActor))
                return;

            // A stale single-actor engagement should never block a fresh F interaction.
            if (!mover.IsEngaged)
                mover.Cancel();
        }
    }

    Vector3 CameraRelativeDirection(Vector3 input)
    {
        if (cameraFollow == null || input.sqrMagnitude < 0.01f)
            return input;

        Vector3 forward = cameraFollow.transform.forward;
        forward.y = 0f;
        forward.Normalize();
        Vector3 right = cameraFollow.transform.right;
        right.y = 0f;
        right.Normalize();
        return (forward * input.z + right * input.x).normalized;
    }

    void SetCameraTarget()
    {
        if (cameraFollow == null || human == null || dog == null)
            return;

        cameraFollow.SetTarget(activeActor.transform);
    }

    bool IsMoverEngaged()
    {
        return FindEngagedMover() != null;
    }

    static FormalCooperativeRailMover FindEngagedMover()
    {
        foreach (FormalCooperativeRailMover mover in FindObjectsOfType<FormalCooperativeRailMover>())
            if (mover.IsEngaged)
                return mover;
        return null;
    }
}
