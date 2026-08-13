using UnityEngine;

public class FormalDoor : MonoBehaviour, IFormalLevelPermanentState
{
    [SerializeField] private Collider blockingCollider;
    [SerializeField] private Transform visual;
    [SerializeField] private Vector3 openOffset = new Vector3(0f, 3f, 0f);
    [SerializeField] private float openSpeed = 3f;

    private Vector3 closedPosition;
    private Vector3 openPosition;

    public bool IsComplete { get; private set; }

    void Awake()
    {
        if (visual == null)
            visual = transform;

        closedPosition = visual.localPosition;
        openPosition = closedPosition + openOffset;
    }

    void Update()
    {
        if (IsComplete)
            visual.localPosition = Vector3.MoveTowards(visual.localPosition, openPosition, openSpeed * Time.deltaTime);
    }

    public void OpenPermanently()
    {
        if (IsComplete)
            return;

        IsComplete = true;
        if (blockingCollider != null)
            blockingCollider.enabled = false;
    }
}
