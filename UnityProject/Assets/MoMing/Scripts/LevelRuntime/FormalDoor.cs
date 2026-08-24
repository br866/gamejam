using UnityEngine;
using UnityEngine.SceneManagement;
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
    public Collider BlockingCollider => blockingCollider;
    public OpeningDirection Direction => openingDirection;
    public float OpenAngle => openAngle;
    public float OpenSpeed => openSpeed;
    public Quaternion OpenRotation => Quaternion.Euler(0f, SignedOpenAngle, 0f);
    public Quaternion ClosedRotation => Quaternion.identity;
    public float SignedOpenAngle => openingDirection == OpeningDirection.Inward ? openAngle : -openAngle;

    public event System.Action<FormalDoor> StateChanged;

    /// <summary>
    /// 按名字片段在所有已加载场景里找一扇门。
    ///
    /// 过关门是摆在 SharedArt 场景里的（比如 ToLevel02_door4），关卡场景里的脚本
    /// 没法在 Inspector 里直接拖跨场景引用，所以留这个按名字找的入口。
    /// </summary>
    public static FormalDoor FindByNameToken(string nameToken)
    {
        if (string.IsNullOrEmpty(nameToken))
            return null;

        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene scene = SceneManager.GetSceneAt(i);
            if (!scene.isLoaded)
                continue;

            foreach (GameObject root in scene.GetRootGameObjects())
                foreach (FormalDoor door in root.GetComponentsInChildren<FormalDoor>(true))
                    if (door.name.IndexOf(nameToken, System.StringComparison.OrdinalIgnoreCase) >= 0)
                        return door;
        }

        return null;
    }

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

        FormalGameFlowController flow = FindObjectOfType<FormalGameFlowController>();
        if (flow != null)
            flow.ReportTransitionDoorOpened(this);

        IsComplete = true;
        if (blockingCollider != null)
            blockingCollider.enabled = false;
        RaiseStateChanged();
    }

    public void OpenPermanently()
    {
        Open();
    }

    public void Close()
    {
        bool wasOpen = IsComplete;
        IsComplete = false;
        if (wasOpen)
            RaiseStateChanged();
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

        bool wasOpen = IsComplete;
        IsComplete = isOpen;
        visualPivot.localRotation = IsComplete ? OpenRotation : ClosedRotation;
        if (blockingCollider != null)
            blockingCollider.enabled = !IsComplete;

        if (wasOpen != isOpen)
            RaiseStateChanged();
    }

    private void RaiseStateChanged()
    {
        StateChanged?.Invoke(this);
    }
}
