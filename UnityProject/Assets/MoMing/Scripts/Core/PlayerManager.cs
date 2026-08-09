using UnityEngine;

/// <summary>
/// 角色切换(Tab)、距离判定(Together/Separated)、联动模式(Q)、脚印可见性管理。
/// </summary>
public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance { get; private set; }

    [Header("Players")]
    public PlayerController human;
    public PlayerController dog;

    [Header("Distance")]
    public float togetherRadius = 3f;
    public float separationRadius = 4.5f;

    [Header("Linked Mode")]
    public float linkRequireRadius = 3f;

    public System.Action<bool> OnTogetherChanged;
    public System.Action<bool> OnLinkedModeChanged;
    public System.Action<bool> OnActiveCharacterChanged;

    private bool isTogether = true;
    private bool isLinkedMode = false;
    private PlayerController activePlayer;
    private CameraFollow camFollow;
    private Vector3 dogFollowVel = Vector3.zero;

    public bool IsActiveDog => activePlayer == dog;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        camFollow = FindObjectOfType<CameraFollow>();
        activePlayer = human;
        human.SetActive(true);
        dog.SetActive(false);
        OnActiveCharacterChanged?.Invoke(false);
    }

    void Update()
    {
        HandleSwitch();
        HandleLinkedMode();
        UpdateDistance();
        UpdateLinkedMovement();
    }

    void HandleSwitch()
    {
        if (isLinkedMode) return;

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (activePlayer == human)
            {
                activePlayer = dog;
                human.SetActive(false);
                dog.SetActive(true);
            }
            else
            {
                activePlayer = human;
                dog.SetActive(false);
                human.SetActive(true);
            }

            if (camFollow != null)
                camFollow.SetTarget(activePlayer.transform);

            OnActiveCharacterChanged?.Invoke(IsActiveDog);
            Debug.Log("[PlayerManager] Switched to " + (IsActiveDog ? "Dog" : "Human"));
        }
    }

    void HandleLinkedMode()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (isLinkedMode)
            {
                // 退出联动模式
                isLinkedMode = false;
                human.SetLinkedMode(false);
                dog.SetLinkedMode(false);

                // 恢复单角色控制
                activePlayer = human;
                human.SetActive(true);
                dog.SetActive(false);

                // 恢复人与狗的碰撞
                var humanCol = human.GetComponent<Collider>();
                var dogCol = dog.GetComponent<Collider>();
                if (humanCol != null && dogCol != null)
                    Physics.IgnoreCollision(humanCol, dogCol, false);

                if (camFollow != null)
                    camFollow.SetTarget(human.transform);

                OnLinkedModeChanged?.Invoke(false);
                OnActiveCharacterChanged?.Invoke(false);
                Debug.Log("[PlayerManager] Exited Linked Mode.");
            }
            else if (isTogether)
            {
                // 进入联动模式（需在一起）
                isLinkedMode = true;
                human.SetLinkedMode(true);
                dog.SetLinkedMode(true);
                human.SetActive(true);
                dog.SetActive(true);

                // 禁用人与狗的碰撞，避免互相阻挡
                var humanCol = human.GetComponent<Collider>();
                var dogCol = dog.GetComponent<Collider>();
                if (humanCol != null && dogCol != null)
                    Physics.IgnoreCollision(humanCol, dogCol, true);

                if (camFollow != null)
                    camFollow.SetLinkedTargets(human.transform, dog.transform);

                OnLinkedModeChanged?.Invoke(true);
                Debug.Log("[PlayerManager] Entered Linked Mode! Both move together.");
            }
            else
            {
                Debug.Log("[PlayerManager] Cannot enter Linked Mode: players too far apart!");
            }
        }
    }

    void UpdateLinkedMovement()
    {
        if (!isLinkedMode) return;

        // 联动模式下狗跟随人，保持固定偏移
        Vector3 offset = new Vector3(1.5f, 0f, 0f);
        Vector3 targetDogPos = human.transform.position + offset;
        targetDogPos.y = dog.transform.position.y;

        // 狗在空中（跳跃中）时不覆盖位置，让物理自由落体
        var dogRb = dog.GetComponent<Rigidbody>();
        bool dogAirborne = dogRb != null && Mathf.Abs(dogRb.velocity.y) > 0.01f;
        if (!dogAirborne)
            dog.transform.position = Vector3.SmoothDamp(dog.transform.position, targetDogPos, ref dogFollowVel, 0.1f);

        // 狗朝向与人一致
        if (human.transform.rotation != dog.transform.rotation)
            dog.transform.rotation = Quaternion.Slerp(dog.transform.rotation, human.transform.rotation, 10f * Time.deltaTime);
    }

    void UpdateDistance()
    {
        if (human == null || dog == null) return;

        float dist = Vector3.Distance(human.transform.position, dog.transform.position);

        if (isTogether)
        {
            if (dist > separationRadius)
            {
                // 分离时自动退出联动
                if (isLinkedMode)
                {
                    isLinkedMode = false;
                    human.SetLinkedMode(false);
                    dog.SetLinkedMode(false);
                    dog.SetActive(false);
                    OnLinkedModeChanged?.Invoke(false);
                    Debug.Log("[PlayerManager] Auto-exit Linked Mode: separated!");
                }

                isTogether = false;
                human.SetTogether(false);
                dog.SetTogether(false);
                if (GameManager.Instance != null)
                    GameManager.Instance.SetSeparated(true);
                OnTogetherChanged?.Invoke(false);
                Debug.Log("[PlayerManager] Separated! Anxiety rising...");
            }
        }
        else
        {
            if (dist < togetherRadius)
            {
                isTogether = true;
                human.SetTogether(true);
                dog.SetTogether(true);
                if (GameManager.Instance != null)
                    GameManager.Instance.SetSeparated(false);
                OnTogetherChanged?.Invoke(true);
                Debug.Log("[PlayerManager] Together again. Safe.");
            }
        }
    }

    public bool IsTogether => isTogether;
    public bool IsLinkedMode => isLinkedMode;
    public PlayerController ActivePlayer => activePlayer;
}
