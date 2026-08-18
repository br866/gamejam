using UnityEngine;
using UnityEngine.Serialization;

public class FormalDoor : MonoBehaviour, IFormalLevelPermanentState, IFormalLevelActuator
{
    public enum OpeningDirection
    {
        Inward,
        Outward
    }

    [SerializeField] private Collider blockingCollider;
    [FormerlySerializedAs("visual")]
    [SerializeField] private Transform visualPivot;
    [SerializeField] private OpeningDirection openingDirection;
    [FormerlySerializedAs("closedAngle")]
    [SerializeField] private float openAngle = 90f;
    [SerializeField] private float openSpeed = 180f;

    public bool IsComplete { get; private set; }
    public bool IsOpen => IsComplete;
    public Transform VisualPivot => visualPivot;
    public OpeningDirection Direction => openingDirection;
    public float OpenAngle => openAngle;
    public float OpenSpeed => openSpeed;
    public Quaternion OpenRotation => Quaternion.Euler(0f, openAngle, 0f);
    public Quaternion ClosedRotation => Quaternion.identity;

    void Awake()
    {
        SetClosedImmediate();
    }

    void Update()
    {
        if (visualPivot == null)
            return;

        Quaternion targetRotation = IsOpen ? OpenRotation : ClosedRotation;
        visualPivot.localRotation = Quaternion.RotateTowards(
            visualPivot.localRotation,
            targetRotation,
            openSpeed * Time.deltaTime);

        if (!IsOpen && visualPivot.localRotation == ClosedRotation && blockingCollider != null)
            blockingCollider.enabled = true;
    }

    public void Open()
    {
        if (IsOpen)
            return;

        IsComplete = true;
        if (blockingCollider != null)
            blockingCollider.enabled = false;
    }

    public void OpenPermanently()
    {
        Open();
    }

    public void Close()
    {
        IsComplete = false;
    }

    public void SetOpenImmediate()
    {
        SetStateImmediate(true);
    }

    public void SetClosedImmediate()
    {
        SetStateImmediate(false);
    }

    public void SetStateImmediate(bool isOpen)
    {
        if (visualPivot == null)
            visualPivot = transform;

        IsComplete = isOpen;
        visualPivot.localRotation = isOpen ? OpenRotation : ClosedRotation;
        if (blockingCollider != null)
            blockingCollider.enabled = !isOpen;
    }
}
