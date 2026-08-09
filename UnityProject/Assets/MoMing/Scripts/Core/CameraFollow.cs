using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// 俯视角摄像机：跟随活跃角色，联动模式跟随双人中点。
/// 支持鼠标水平旋转(Yaw)、垂直俯仰(Pitch)、遮挡物隐藏、鼠标光标隐藏。
/// </summary>
public class CameraFollow : MonoBehaviour
{
    #region Fields

    [Header("Target")]
    public Transform target;

    [Header("Camera Settings")]
    [Tooltip("摄像机与目标的总距离")]
    public float distance = 15f;

    [Tooltip("摄像机俯视角（0=平视，90=正俯视）")]
    [FormerlySerializedAs("tiltAngle")]
    public float angle = 55f;

    [Tooltip("跟随平滑速度")]
    public float followSpeed = 8f;

    [Header("Mouse Rotation")]
    [Tooltip("是否启用鼠标水平旋转(Yaw)")]
    public bool enableMouseRotation = true;

    [Tooltip("鼠标X轴旋转灵敏度")]
    public float mouseSensitivity = 3f;

    [Tooltip("是否启用鼠标垂直俯仰(Pitch)")]
    public bool enableMouseTilt = false;

    [Tooltip("鼠标Y轴俯仰灵敏度")]
    public float mouseYSensitivity = 2f;

    [Tooltip("俯仰角下限（度）")]
    public float minAngle = 20f;

    [Tooltip("俯仰角上限（度）")]
    public float maxAngle = 80f;

    [Header("Cursor")]
    [Tooltip("是否隐藏鼠标光标并锁定在窗口内")]
    public bool hideCursor = true;

    [Header("Obstruction")]
    [Tooltip("是否启用遮挡物隐藏")]
    public bool enableObstructionHide = true;

    [Tooltip("遮挡检测的LayerMask，默认Everything")]
    public LayerMask obstructionMask = ~0;

    private float yaw = 0f;
    private float pitch = 55f;
    private Vector3 velocityRef = Vector3.zero;
    private Transform targetB;
    private bool isLinked = false;
    private System.Collections.Generic.List<Renderer> hiddenRenderers = new System.Collections.Generic.List<Renderer>();

    #endregion

    #region Unity Lifecycle

    void Start()
    {
        pitch = angle;
        var cam = GetComponent<Camera>();
        if (cam != null) cam.fieldOfView = 50f;
        ApplyCursorState();
    }

    void LateUpdate()
    {
        if (target == null) return;

        HandleMouseInput();

        Vector3 focusPos = GetFocusPosition();
        UpdateCameraPosition(focusPos);

        if (enableObstructionHide)
            HandleObstruction(focusPos);
    }

    void OnDestroy()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    #endregion

    #region Camera Logic

    void HandleMouseInput()
    {
        if (enableMouseRotation)
            yaw += Input.GetAxis("Mouse X") * mouseSensitivity;

        if (enableMouseTilt)
        {
            pitch -= Input.GetAxis("Mouse Y") * mouseYSensitivity;
            pitch = Mathf.Clamp(pitch, minAngle, maxAngle);
        }
        else
        {
            pitch = angle;
        }
    }

    Vector3 GetFocusPosition()
    {
        if (isLinked && targetB != null)
            return (target.position + targetB.position) * 0.5f;
        return target.position;
    }

    void UpdateCameraPosition(Vector3 focusPos)
    {
        float currentAngle = enableMouseTilt ? pitch : angle;

        float rad = currentAngle * Mathf.Deg2Rad;
        float h = distance * Mathf.Sin(rad);
        float horiz = distance * Mathf.Cos(rad);

        Vector3 horizontalOffset = Quaternion.Euler(0f, yaw, 0f) * new Vector3(0f, 0f, -horiz);
        Vector3 targetPos = focusPos + new Vector3(0f, h, 0f) + horizontalOffset;
        transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref velocityRef, 1f / followSpeed);
        transform.rotation = Quaternion.Euler(currentAngle, yaw, 0f);
    }

    void HandleObstruction(Vector3 focusPos)
    {
        foreach (var r in hiddenRenderers)
        {
            if (r != null) r.enabled = true;
        }
        hiddenRenderers.Clear();

        Vector3 dir = transform.position - focusPos;
        float dist = dir.magnitude;
        if (dist < 0.01f) return;

        RaycastHit[] hits = Physics.RaycastAll(focusPos, dir.normalized, dist, obstructionMask);
        foreach (var hit in hits)
        {
            if (hit.transform == target || (targetB != null && hit.transform == targetB)) continue;
            if (target != null && hit.transform.IsChildOf(target)) continue;
            if (targetB != null && hit.transform.IsChildOf(targetB)) continue;

            var rend = hit.transform.GetComponent<Renderer>();
            if (rend != null && rend.enabled)
            {
                rend.enabled = false;
                hiddenRenderers.Add(rend);
            }
        }
    }

    void ApplyCursorState()
    {
        Cursor.visible = !hideCursor;
        Cursor.lockState = hideCursor ? CursorLockMode.Locked : CursorLockMode.None;
    }

    #endregion

    #region Public API

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        isLinked = false;
        targetB = null;
    }

    public void SetLinkedTargets(Transform a, Transform b)
    {
        target = a;
        targetB = b;
        isLinked = true;
    }

    public void SetCursorVisible(bool visible)
    {
        hideCursor = !visible;
        ApplyCursorState();
    }

    #endregion
}
