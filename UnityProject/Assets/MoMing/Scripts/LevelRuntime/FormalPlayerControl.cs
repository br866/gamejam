using UnityEngine;

public class FormalPlayerControl : MonoBehaviour
{
    [SerializeField] private FormalPlayerActor human;
    [SerializeField] private FormalPlayerActor dog;
    private FormalPlayerActor activeActor;
    private CameraFollow cameraFollow;

    public bool IsDogActive => activeActor != null && activeActor.Role == FormalPlayerActor.ActorRole.Dog;

    public event System.Action<bool> ActiveRoleChanged;

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
            if (ActiveRoleChanged != null)
                ActiveRoleChanged(IsDogActive);
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            ToggleMoverEngagement();
        }

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
        FormalCooperativeRailMover railMover = FindEngagedRailMover();
        IFormalPushMover engagedMover = railMover != null ? (IFormalPushMover)railMover : FindEngagedPushable();
        if (engagedMover != null)
        {
            if (Input.GetKey(KeyCode.W))
                engagedMover.SetAttachedPushAnimation();
            else if (Input.GetKey(KeyCode.S))
                engagedMover.SetAttachedPullAnimation();
            else
                engagedMover.SetAttachedIdleAnimation();

            if (railMover != null)
            {
                Vector3 moverDirection = activeActor.transform.forward * vertical;
                railMover.Move(moverDirection, vertical > 0.01f);
            }
            // 物理推箱的移动由其自身 FixedUpdate 读取输入驱动。
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

        foreach (FormalPushableCrate crate in FindObjectsOfType<FormalPushableCrate>())
        {
            if (crate.IsAttached(activeActor))
            {
                crate.Cancel();
                return;
            }

            if (crate.TryEngage(activeActor))
                return;
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
        return FindEngagedRailMover() != null || FindEngagedPushable() != null;
    }

    static FormalCooperativeRailMover FindEngagedRailMover()
    {
        foreach (FormalCooperativeRailMover mover in FindObjectsOfType<FormalCooperativeRailMover>())
            if (mover.IsEngaged)
                return mover;
        return null;
    }

    static FormalPushableCrate FindEngagedPushable()
    {
        foreach (FormalPushableCrate crate in FindObjectsOfType<FormalPushableCrate>())
            if (crate.IsEngaged)
                return crate;
        return null;
    }
}
