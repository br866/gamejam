using UnityEngine;

/// <summary>
/// 门/栅栏控制器：Open()/Close() 方法，通过上下位移实现开关。
/// </summary>
public class GateController : MonoBehaviour, IFormalLevelActuator
{
    [Header("Animation")]
    public float openYOffset = 5f;
    public float moveSpeed = 3f;

    private Vector3 closedPosition;
    private Vector3 openPosition;
    private bool isOpen = false;

    void Awake()
    {
        closedPosition = transform.position;
        openPosition = closedPosition + new Vector3(0f, openYOffset, 0f);
    }

    void Start()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnLevelReset += ResetGate;

        FormalLevelController formalLevel = FormalLevelActors.FindLevelController(gameObject.scene);
        if (formalLevel != null)
            formalLevel.RegisterTemporaryState(new FormalGateResetState(this));
    }

    void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnLevelReset -= ResetGate;
    }

    void Update()
    {
        Vector3 targetPos = isOpen ? openPosition : closedPosition;
        if (Vector3.Distance(transform.position, targetPos) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
        }
    }

    public void Open()
    {
        if (!isOpen)
            isOpen = true;
    }

    public void Close()
    {
        if (isOpen)
            isOpen = false;
    }

    void ResetGate()
    {
        isOpen = false;
    }

    public bool IsOpen => isOpen;

    class FormalGateResetState : IFormalLevelTemporaryState
    {
        readonly GateController gate;

        public FormalGateResetState(GateController gate)
        {
            this.gate = gate;
        }

        public void ResetTemporaryState()
        {
            gate.Close();
        }
    }
}
