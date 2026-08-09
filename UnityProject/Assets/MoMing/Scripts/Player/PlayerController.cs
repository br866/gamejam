using UnityEngine;

/// <summary>
/// 角色移动控制：WASD 移动、Space 跳跃、E 拾取/丢弃道具(仅人)、F 触发开关(仅人)、狗疾跑(LeftShift)。
/// 支持联动模式(Q)：双角色同时移动。
/// </summary>
public class PlayerController : MonoBehaviour
{
    public enum CharacterType { Human, Dog }

    [Header("Character")]
    public CharacterType characterType = CharacterType.Human;

    [Header("Movement")]
    public float walkSpeed = 4f;
    public float sprintSpeed = 8f;
    public float rotationSpeed = 10f;
    public float jumpHeight = 2f;
    public float groundCheckDistance = 0.15f;

    [Header("Interaction")]
    public float interactRange = 1.5f;

    [Header("Audio")]
    [SerializeField] private AudioClip footstepClip;

    private Rigidbody rb;
    private Collider col;
    private bool isActive = false;
    private bool isTogether = true;
    private bool isLinkedMode = false;
    private bool isGrounded = true;
    private PickupItem heldItem;
    private PushableBox attachedBox;
    private Vector3 pendingMoveDir;
    private float pendingSpeed;
    private Quaternion pendingRot;
    private bool hasPendingRot;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
    }

    void Update()
    {
        if (!isActive) return;

        HandleMovementInput();
        HandleJump();
        HandleItemInteraction();
        HandleSwitchInteraction();
    }

    void FixedUpdate()
    {
        if (col != null)
        {
            float checkDist = col.bounds.extents.y + groundCheckDistance;
            isGrounded = Physics.Raycast(transform.position, Vector3.down, checkDist);
        }

        if (!isActive) return;

        Vector3 newVel = pendingMoveDir * pendingSpeed;
        newVel.y = rb.velocity.y;
        rb.velocity = newVel;

        // 旋转通过rb.MoveRotation，与插值系统配合
        if (hasPendingRot)
        {
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, pendingRot, rotationSpeed * Time.fixedDeltaTime));
            hasPendingRot = false;
        }
    }

    void HandleMovementInput()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        Vector3 inputDir = new Vector3(h, 0f, v).normalized;

        // 移动方向相对于摄像机yaw
        float camYaw = 0f;
        if (Camera.main != null)
            camYaw = Camera.main.transform.eulerAngles.y;
        pendingMoveDir = Quaternion.Euler(0f, camYaw, 0f) * inputDir;

        pendingSpeed = walkSpeed;
        // 狗疾跑：仅分离状态(非联动)且操作狗时
        if (characterType == CharacterType.Dog && !isTogether && !isLinkedMode && Input.GetKey(KeyCode.LeftShift))
            pendingSpeed = sprintSpeed;

        // 计算目标旋转，留给FixedUpdate用rb.MoveRotation执行
        // 挂住箱子时不旋转模型
        if (pendingMoveDir.sqrMagnitude > 0.01f && attachedBox == null)
        {
            pendingRot = Quaternion.LookRotation(pendingMoveDir);
            hasPendingRot = true;
        }
    }

    void HandleJump()
    {
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            float g = Physics.gravity.y;
            float jumpVel = Mathf.Sqrt(2f * Mathf.Abs(g) * jumpHeight);
            rb.velocity = new Vector3(rb.velocity.x, jumpVel, rb.velocity.z);
        }
    }

    // E 键：拾取/丢弃道具（仅人）
    void HandleItemInteraction()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (isLinkedMode) return;
            if (characterType != CharacterType.Human) return;

            // 持有道具时丢弃
            if (heldItem != null)
            {
                heldItem.Drop();
                heldItem = null;
                return;
            }

            // 未持有道具时检测附近可拾取物品
            Vector3 checkPos = rb.position + (rb.rotation * Vector3.forward) * 0.6f;
            Collider[] hits = Physics.OverlapSphere(checkPos, interactRange);
            float nearestDist = float.MaxValue;
            PickupItem nearestItem = null;

            foreach (var hit in hits)
            {
                var item = hit.GetComponent<PickupItem>();
                if (item != null && !item.IsHeld)
                {
                    float d = Vector3.Distance(rb.position, hit.transform.position);
                    if (d < nearestDist)
                    {
                        nearestDist = d;
                        nearestItem = item;
                    }
                }
            }

            if (nearestItem != null)
            {
                heldItem = nearestItem;
                nearestItem.Pickup(transform);
            }
        }
    }

    // F 键：触发开关（仅人）/ 双人模式下挂住箱子
    void HandleSwitchInteraction()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            if (isLinkedMode)
            {
                if (characterType != CharacterType.Human) return;
                HandleBoxAttach();
                return;
            }
            if (characterType != CharacterType.Human) return;

            Collider[] hits = Physics.OverlapSphere(rb.position + (rb.rotation * Vector3.forward) * 0.6f, interactRange);
            foreach (var hit in hits)
            {
                var sw = hit.GetComponent<PuzzleSwitch>();
                if (sw != null)
                {
                    sw.Interact();
                    return;
                }
            }
        }
    }

    void HandleBoxAttach()
    {
        // 已挂住则脱离
        if (attachedBox != null)
        {
            attachedBox.Detach();
            attachedBox = null;
            return;
        }

        // 查找附近可挂住的可推箱子
        Collider[] hits = Physics.OverlapSphere(rb.position + (rb.rotation * Vector3.forward) * 0.6f, interactRange);
        float nearestDist = float.MaxValue;
        PushableBox nearest = null;

        foreach (var hit in hits)
        {
            var box = hit.GetComponent<PushableBox>();
            if (box != null && !box.IsAttached)
            {
                float d = Vector3.Distance(rb.position, hit.transform.position);
                if (d < nearestDist)
                {
                    nearestDist = d;
                    nearest = box;
                }
            }
        }

        if (nearest != null)
        {
            attachedBox = nearest;
            nearest.Attach(transform);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("LevelExit"))
        {
            if (GameManager.Instance != null)
                GameManager.Instance.OnLevelComplete();
        }
    }

    public void SetActive(bool active)
    {
        isActive = active;
    }

    public void SetTogether(bool together)
    {
        isTogether = together;
    }

    public void SetLinkedMode(bool linked)
    {
        isLinkedMode = linked;
        if (!linked && attachedBox != null)
        {
            attachedBox.Detach();
            attachedBox = null;
        }
    }

    public bool HasItem => heldItem != null;
    public bool IsTogether => isTogether;
    public bool IsLinkedMode => isLinkedMode;
    public bool IsBoxAttached => attachedBox != null;
}
