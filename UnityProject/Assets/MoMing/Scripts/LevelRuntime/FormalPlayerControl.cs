using UnityEngine;

public class FormalPlayerControl : MonoBehaviour
{
    [SerializeField] private FormalPlayerActor human;
    [SerializeField] private FormalPlayerActor dog;
    [SerializeField] private float linkRequireRadius = 3f;
    [SerializeField] private Vector3 linkedDogOffset = new Vector3(1.5f, 0f, 0f);

    private FormalPlayerActor activeActor;
    private bool linked;
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
        if (human == null || dog == null)
            return;

        if (Input.GetKeyDown(KeyCode.Tab) && !linked)
        {
            activeActor = activeActor == human ? dog : human;
            SetCameraTarget();
        }

        if (Input.GetKeyDown(KeyCode.Q))
            ToggleLinkedMode();

        if (Input.GetKeyDown(KeyCode.Space) && !linked)
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
        bool sprint = !linked && activeActor.Role == FormalPlayerActor.ActorRole.Dog && Input.GetKey(KeyCode.LeftShift);

        activeActor.Move(direction, sprint);
        if (linked)
        {
            dog.SetPosition(human.transform.position + linkedDogOffset);
            dog.transform.rotation = human.transform.rotation;
            human.SetLinked(direction.sqrMagnitude > 0.01f);
            dog.SetLinked(direction.sqrMagnitude > 0.01f);
        }
    }

    void ToggleLinkedMode()
    {
        if (linked)
        {
            linked = false;
            activeActor = human;
            dog.Stop();
            SetCameraTarget();
            return;
        }

        if (Vector3.Distance(human.transform.position, dog.transform.position) > linkRequireRadius)
            return;

        linked = true;
        activeActor = human;
        dog.SetLinked(false);
        SetCameraTarget();
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

        if (linked)
            cameraFollow.SetLinkedTargets(human.transform, dog.transform);
        else
            cameraFollow.SetTarget(activeActor.transform);
    }
}
