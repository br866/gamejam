using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// 俯视角摄像机：跟随活跃角色，联动模式跟随双人中点。
/// 支持鼠标水平旋转(Yaw)、垂直俯仰(Pitch)、遮挡物隐藏、鼠标光标隐藏。
/// 提示镜头(PlayHint)：临时平滑聚焦到指定机关/道具，停留后飞回当前角色。
/// 全部使用协程实现，不使用 async/await，兼容 Unity 2022.3。
/// </summary>
public class CameraFollow : MonoBehaviour
{
    #region Fields

    [Header("Target")]
    public Transform target;

    [Header("Camera Settings")]
    [Tooltip("摄像机与目标的总距离")]
    public float distance = 8f;

    [Tooltip("摄像机俯视角（0=平视，90=正俯视）")]
    [FormerlySerializedAs("tiltAngle")]
    public float angle = 20f;

    [Tooltip("跟随平滑速度")]
    public float followSpeed = 8f;

    [Header("Mouse Rotation")]
    [Tooltip("是否启用鼠标水平旋转(Yaw)")]
    public bool enableMouseRotation = false;

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

    [Header("Camera Collision 相机防穿墙")]
    [Tooltip("相机被墙挡住时自动拉近到墙前，避免视角跑到建筑外面（贴墙角时压缩镜头）")]
    public bool enableCameraCollision = true;

    [Tooltip("相机碰撞检测的 LayerMask，默认 Everything")]
    public LayerMask collisionMask = ~0;

    [Tooltip("相机停在墙前的缓冲距离（越大离墙越远）")]
    public float collisionBuffer = 0.3f;

    [Tooltip("相机被压缩时离角色的最近距离（防止贴脸）")]
    public float minCollisionDistance = 1.5f;

    [Header("Focus Smoothing")]
    [Tooltip("普通跟随时焦点平滑时间（越小越跟手，0.03~0.06 接近原手感）")]
    public float focusSmoothTime = 0.04f;

    [Tooltip("切换角色时的转场平滑时间（越大转场越慢越明显）")]
    public float switchSmoothTime = 0.35f;

    [Header("Hint Camera 提示镜头")]
    [Tooltip("聚焦到提示点 / 从提示点返回的时间（秒）")]
    public float hintMoveTime = 0.8f;

    [Tooltip("默认在提示点停留的时间（秒），HintTrigger 可单独覆盖")]
    public float hintHoldTime = 1.5f;

    [Tooltip("提示镜头移动时的焦点平滑时间（越大越像电影运镜）")]
    public float hintFocusSmoothTime = 0.6f;

    private float yaw = 0f;
    private float pitch = 55f;
    private Vector3 velocityRef = Vector3.zero;
    private Transform targetB;
    private bool isLinked = false;
    private System.Collections.Generic.List<Renderer> hiddenRenderers = new System.Collections.Generic.List<Renderer>();

    // 焦点平滑
    private Vector3 currentFocus;
    private Vector3 focusVel = Vector3.zero;
    private bool focusInitialized = false;
    private bool camPosInitialized = false;
    private float switchTimer = 0f;

    // 提示镜头状态
    private bool isHinting = false;
    private Transform hintTarget;
    private bool suppressMouse = false;
    private Coroutine hintRoutine;

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

        // 暂停时（菜单打开）完全冻结镜头：不响应鼠标、不跟随。
        // 双保险——既看时间是否停摆(timeScale==0)，也看暂停菜单是否打开(IsPaused)，
        // 任一条件成立就直接 return，保证暂停时视角被彻底锁死。
        if (Time.timeScale == 0f || GameHUDManager.IsPaused) return;

        HandleMouseInput();

        Vector3 desiredFocus = GetDesiredFocus();

        // 首帧直接对齐，避免开局大幅滑动
        if (!focusInitialized)
        {
            currentFocus = desiredFocus;
            focusInitialized = true;
        }

        // 选择当前平滑时间：提示 > 切换转场 > 普通
        float smoothTime = focusSmoothTime;
        if (switchTimer > 0f)
        {
            switchTimer -= Time.deltaTime;
            smoothTime = switchSmoothTime;
        }
        if (isHinting)
            smoothTime = hintFocusSmoothTime;

        currentFocus = Vector3.SmoothDamp(currentFocus, desiredFocus, ref focusVel, smoothTime);

        UpdateCameraPosition(currentFocus);

        if (enableObstructionHide)
            HandleObstruction(currentFocus);
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
        // 提示镜头期间冻结鼠标转向，保证玩家的移动方向不会因镜头飞走而错乱
        if (enableMouseRotation && !suppressMouse)
            yaw += Input.GetAxis("Mouse X") * mouseSensitivity;

        if (enableMouseTilt && !suppressMouse)
        {
            pitch -= Input.GetAxis("Mouse Y") * mouseYSensitivity;
            pitch = Mathf.Clamp(pitch, minAngle, maxAngle);
        }
        else if (!enableMouseTilt)
        {
            pitch = angle;
        }
    }

    Vector3 GetDesiredFocus()
    {
        if (isHinting && hintTarget != null)
            return hintTarget.position;
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

        // 相机防穿墙：从角色朝“理想相机位”发射线，若中途撞到墙，就把相机拉到墙前，
        // 这样贴墙角时镜头会被压缩，而不会穿出建筑跑到盒子外面。
        if (enableCameraCollision)
        {
            Vector3 camDir = targetPos - focusPos;
            float camDist = camDir.magnitude;
            if (camDist > 0.01f)
            {
                Vector3 dirN = camDir / camDist;
                RaycastHit[] hits = Physics.RaycastAll(focusPos, dirN, camDist, collisionMask);
                float closest = camDist;
                foreach (var hit in hits)
                {
                    // 跳过角色自身和触发器（触发器不算墙）
                    if (hit.transform == target || (targetB != null && hit.transform == targetB)) continue;
                    if (target != null && hit.transform.IsChildOf(target)) continue;
                    if (targetB != null && hit.transform.IsChildOf(targetB)) continue;
                    if (hit.collider != null && hit.collider.isTrigger) continue;
                    if (hit.distance < closest) closest = hit.distance;
                }
                if (closest < camDist)
                {
                    float pulled = Mathf.Max(closest - collisionBuffer, minCollisionDistance);
                    targetPos = focusPos + dirN * pulled;
                }
            }
        }

        if (!camPosInitialized)
        {
            // 开局第一帧瞬间归位，不做平滑——避免从编辑器摆放的位置"滑动/摆动"到俯视位。
            transform.position = targetPos;
            camPosInitialized = true;
        }
        else
        {
            transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref velocityRef, 1f / followSpeed);
        }
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

            // 连子物体一起隐藏：导入的 FBX 模型常常碰撞体在父物体、网格渲染器在子物体，
            // 只隐藏被射线击中的那个物体会漏掉子物体的模型，导致仍然挡住视线。
            var rends = hit.transform.GetComponentsInChildren<Renderer>();
            foreach (var rend in rends)
            {
                if (rend != null && rend.enabled)
                {
                    rend.enabled = false;
                    hiddenRenderers.Add(rend);
                }
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
        // 已初始化的情况下触发一次转场平滑（切角色时的镜头过渡）
        if (focusInitialized)
            switchTimer = switchSmoothTime;
    }

    public void SetLinkedTargets(Transform a, Transform b)
    {
        target = a;
        targetB = b;
        isLinked = true;
        if (focusInitialized)
            switchTimer = switchSmoothTime;
    }

    public void SetCursorVisible(bool visible)
    {
        hideCursor = !visible;
        ApplyCursorState();
    }

    /// <summary>是否正在播放提示镜头。</summary>
    public bool IsHinting => isHinting;

    /// <summary>播放提示镜头：平滑聚焦到 focus，停留默认时长后飞回当前角色。</summary>
    public void PlayHint(Transform focus)
    {
        PlayHint(focus, hintHoldTime);
    }

    /// <summary>播放提示镜头：平滑聚焦到 focus，停留 hold 秒后飞回当前角色。</summary>
    public void PlayHint(Transform focus, float hold)
    {
        if (focus == null) return;
        if (isHinting) return; // 已在播放则忽略，避免叠加
        hintRoutine = StartCoroutine(HintRoutine(focus, hold));
    }

    /// <summary>立即中断提示镜头并飞回角色。</summary>
    public void StopHint()
    {
        if (hintRoutine != null) StopCoroutine(hintRoutine);
        EndHintState();
    }

    #endregion

    #region Hint Coroutine

    IEnumerator HintRoutine(Transform focus, float hold)
    {
        isHinting = true;
        hintTarget = focus;
        suppressMouse = true;
        if (PlayerManager.Instance != null)
            PlayerManager.Instance.SetHintActive(true);

        // 飞向提示点
        yield return new WaitForSeconds(hintMoveTime);
        // 停留
        yield return new WaitForSeconds(hold);

        // 取消聚焦，焦点目标自动回到当前角色
        isHinting = false;
        // 飞回角色
        yield return new WaitForSeconds(hintMoveTime);

        EndHintState();
    }

    void EndHintState()
    {
        isHinting = false;
        hintTarget = null;
        suppressMouse = false;
        hintRoutine = null;
        if (PlayerManager.Instance != null)
            PlayerManager.Instance.SetHintActive(false);
    }

    #endregion
}
