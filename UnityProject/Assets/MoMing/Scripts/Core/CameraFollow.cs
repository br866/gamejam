using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// 第三人称环绕摄像机（塞尔达 / 原神式）。
///
/// 核心：相机挂在角色背后的一个"球面"上，鼠标 X 绕角色转 360°，鼠标 Y 上下俯仰。
/// 相机永远 LookAt 角色（而不是用固定欧拉角），所以无论被墙压近、还是跟随有延迟，
/// 角色都会稳稳待在画面中心 —— 这是旧版本"视角特别奇怪"的根本原因。
///
/// 保留原有功能：双人联动跟中点、提示镜头 PlayHint、遮挡物隐藏、暂停冻结、鼠标锁定。
/// 全部用协程实现，不使用 async/await，兼容 Unity 2022.3。
/// </summary>
public class CameraFollow : MonoBehaviour
{
    #region Fields

    [Header("Target")]
    public Transform target;

    [Header("第三人称基础")]
    [Tooltip("相机与角色的距离。第三人称建议 4~7，室内窄场景可以更小")]
    public float distance = 5.5f;

    [Tooltip("初始俯仰角（0=平视，正数=从上往下俯视）")]
    [FormerlySerializedAs("tiltAngle")]
    public float angle = 15f;

    [Tooltip("开局水平朝向偏移：0 = 正好在角色背后")]
    public float initialYawOffset = 0f;

    [Tooltip("相机位置跟随速度，越大越跟手（建议 10~20）")]
    public float followSpeed = 14f;

    [Header("按角色身高自适应（人 / 狗）")]
    [Tooltip("开启后，焦点高度自动按角色碰撞体的实际身高算，狗矮就自动压低视线")]
    public bool autoFocusHeight = true;

    [Tooltip("焦点高度 = 角色身高 × 这个比例。0.8 ≈ 肩膀/头部，1.0 = 头顶")]
    [Range(0.1f, 1.2f)]
    public float focusHeightRatio = 0.8f;

    [Tooltip("自适应结果的下限（狗太矮时不至于贴地）")]
    public float minFocusHeight = 0.35f;

    [Tooltip("自适应结果的上限")]
    public float maxFocusHeight = 2f;

    [Tooltip("关掉 autoFocusHeight 时使用的固定焦点高度")]
    public float focusHeight = 1.4f;

    [Tooltip("距离也跟着身高缩放：矮的角色相机自动靠近一点")]
    public bool autoDistance = true;

    [Tooltip("参考身高：身高等于这个值的角色用上面填的 distance 原值")]
    public float referenceHeight = 1.8f;

    [Tooltip("距离缩放强度。0 = 距离完全不变；1 = 严格按身高比例缩放")]
    [Range(0f, 1f)]
    public float distanceScaleStrength = 0.5f;

    [Tooltip("切换角色时，距离过渡的速度（越大切得越快）")]
    public float distanceBlendSpeed = 6f;

    [Header("鼠标视角 (360°)")]
    [Tooltip("鼠标 X：绕角色水平旋转，无限制 360°")]
    public bool enableMouseRotation = true;

    [Tooltip("鼠标 X 灵敏度")]
    public float mouseSensitivity = 3f;

    [Tooltip("鼠标 Y：上下俯仰")]
    public bool enableMouseTilt = true;

    [Tooltip("鼠标 Y 灵敏度")]
    public float mouseYSensitivity = 2f;

    [Tooltip("俯仰下限（度）。负数 = 允许从下往上看")]
    public float minAngle = -25f;

    [Tooltip("俯仰上限（度）。80 以上会翻车，别超过 80")]
    public float maxAngle = 70f;

    [Tooltip("反转 Y 轴（有些人习惯推鼠标向上=镜头向上）")]
    public bool invertY = false;

    [Header("滚轮缩放")]
    public bool enableZoom = true;
    public float minDistance = 2f;
    public float maxDistance = 9f;
    public float zoomSpeed = 3f;

    [Header("Cursor")]
    [Tooltip("是否隐藏鼠标光标并锁定在窗口内")]
    public bool hideCursor = true;

    [Header("双人联动")]
    [Tooltip("联动跟双人中点时，按两人间距自动拉远的系数（0 = 不自动拉远）")]
    public float linkedDistanceBonus = 0.35f;

    [Header("防穿墙 (Camera Collision)")]
    [Tooltip("相机被墙挡住时自动拉近到墙前，避免视角穿到建筑外面")]
    public bool enableCameraCollision = true;

    [Tooltip("相机碰撞检测的 LayerMask")]
    public LayerMask collisionMask = ~0;

    [Tooltip("相机的\"体积\"半径，用球形检测代替射线，避免擦着墙角穿过去")]
    public float collisionRadius = 0.25f;

    [Tooltip("相机停在墙前的缓冲距离")]
    public float collisionBuffer = 0.2f;

    [Tooltip("被压缩时离角色的最近距离（太小会看到角色内部）")]
    public float minCollisionDistance = 0.8f;

    [Tooltip("离开墙面后镜头拉回原距离的速度（贴墙拉近是瞬间的，拉回是渐进的）")]
    public float collisionRecoverSpeed = 4f;

    [Header("遮挡物隐藏")]
    [Tooltip("挡在相机和角色之间的物体临时隐形。第三人称距离近时通常不太需要")]
    public bool enableObstructionHide = true;

    [Tooltip("遮挡检测的 LayerMask")]
    public LayerMask obstructionMask = ~0;

    [Header("焦点平滑")]
    [Tooltip("普通跟随时焦点平滑时间（越小越跟手，0.03~0.08）")]
    public float focusSmoothTime = 0.05f;

    [Tooltip("切换角色时的转场平滑时间")]
    public float switchSmoothTime = 0.35f;

    [Header("提示镜头 Hint Camera")]
    [Tooltip("聚焦到提示点 / 从提示点返回的时间（秒）")]
    public float hintMoveTime = 0.8f;

    [Tooltip("默认在提示点停留的时间（秒），HintTrigger 可单独覆盖")]
    public float hintHoldTime = 1.5f;

    [Tooltip("提示镜头移动时的焦点平滑时间（越大越像电影运镜）")]
    public float hintFocusSmoothTime = 0.6f;

    // ---- 运行时状态 ----
    private float yaw = 0f;
    private float pitch = 15f;
    private float zoomDistance;          // 玩家滚轮调出来的目标距离
    private float currentDistance;       // 考虑防穿墙后的实际距离
    private float baseDistanceSmoothed;  // 未考虑防穿墙、但已按身高缩放并平滑过的距离
    private bool baseDistanceInit = false;

    // 角色身高缓存：Transform -> (身高, 焦点高度)。只在切换角色时量一次。
    private Transform measuredA, measuredB;
    private float heightA = -1f, heightB = -1f;
    private float focusHeightA, focusHeightB;
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

    /// <summary>当前水平朝向（角度）。PlayerController 可以用它做"相对相机"的移动方向。</summary>
    public float Yaw => yaw;

    void Start()
    {
        pitch = Mathf.Clamp(angle, minAngle, maxAngle);
        zoomDistance = distance;
        currentDistance = distance;

        var cam = GetComponent<Camera>();
        if (cam != null && cam.fieldOfView < 1f) cam.fieldOfView = 55f;

        ApplyCursorState();
    }

    void LateUpdate()
    {
        if (target == null) return;

        // 暂停时（菜单打开）完全冻结镜头：不响应鼠标、不跟随。
        // 双保险 —— 时间停摆(timeScale==0) 或 暂停菜单打开(IsPaused) 任一成立就直接 return。
        if (Time.timeScale == 0f || GameHUDManager.IsPaused) return;

        // 按角色实际身高更新焦点高度 / 距离（内部有缓存，只在换角色时真正测量）
        RefreshTargetMetrics();

        // 第一帧：把相机瞬间摆到角色正后方，避免从编辑器摆放的位置滑过来
        if (!camPosInitialized)
        {
            yaw = target.eulerAngles.y + initialYawOffset;
            pitch = Mathf.Clamp(angle, minAngle, maxAngle);
        }

        HandleMouseInput();
        HandleZoomInput();

        Vector3 desiredFocus = GetDesiredFocus();

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

        UpdateCameraTransform(currentFocus);

        if (enableObstructionHide)
            HandleObstruction(currentFocus);
    }

    void OnDestroy()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    #endregion

    #region Input

    void HandleMouseInput()
    {
        // 提示镜头期间冻结鼠标转向，保证玩家的移动方向不会因镜头飞走而错乱
        if (suppressMouse) return;

        if (enableMouseRotation)
        {
            yaw += Input.GetAxis("Mouse X") * mouseSensitivity;
            // 360° 无限制，只做数值回绕防止 float 越滚越大
            if (yaw > 360f) yaw -= 360f;
            else if (yaw < -360f) yaw += 360f;
        }

        if (enableMouseTilt)
        {
            float dy = Input.GetAxis("Mouse Y") * mouseYSensitivity;
            pitch += invertY ? dy : -dy;
            pitch = Mathf.Clamp(pitch, minAngle, maxAngle);
        }
        else
        {
            pitch = Mathf.Clamp(angle, minAngle, maxAngle);
        }
    }

    void HandleZoomInput()
    {
        if (!enableZoom || suppressMouse) return;

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.0001f)
        {
            zoomDistance = Mathf.Clamp(zoomDistance - scroll * zoomSpeed * 10f, minDistance, maxDistance);
        }
    }

    #endregion

    #region Camera Logic

    #region 身高自适应

    /// <summary>
    /// 量一个角色的"身高"：取它自己和所有子物体上非触发器碰撞体的包围盒顶点，
    /// 减去 transform 原点（角色脚底）的高度。没有碰撞体就退回用 Renderer。
    /// 只在切换角色时调用一次，不是每帧 —— 动画会让包围盒抖动。
    /// </summary>
    /// <returns>量到的身高；量不到（物体还没激活等）返回 -1，调用方下一帧会重试。</returns>
    float MeasureHeight(Transform t)
    {
        if (t == null) return -1f;

        bool found = false;
        float top = t.position.y;

        var cols = t.GetComponentsInChildren<Collider>();
        foreach (var c in cols)
        {
            if (c == null || !c.enabled || c.isTrigger) continue;
            if (!c.gameObject.activeInHierarchy) continue;
            if (c.bounds.max.y > top) top = c.bounds.max.y;
            found = true;
        }

        if (!found)
        {
            var rends = t.GetComponentsInChildren<Renderer>();
            foreach (var r in rends)
            {
                if (r == null || !r.enabled) continue;
                if (!r.gameObject.activeInHierarchy) continue;
                if (r.bounds.max.y > top) top = r.bounds.max.y;
                found = true;
            }
        }

        if (!found) return -1f;
        return Mathf.Max(top - t.position.y, 0.1f);
    }

    /// <summary>算出某个角色该用的焦点高度：优先看它身上的 CameraTargetProfile 覆盖。</summary>
    float ComputeFocusHeight(Transform t, float height)
    {
        var profile = t != null ? t.GetComponent<CameraTargetProfile>() : null;
        if (profile != null && profile.overrideFocusHeight)
            return profile.focusHeight;

        if (!autoFocusHeight)
            return focusHeight;

        return Mathf.Clamp(height * focusHeightRatio, minFocusHeight, maxFocusHeight);
    }

    /// <summary>
    /// 切换角色 / 每帧兜底：量身高并缓存。量失败（角色还没激活）时不写缓存，下一帧自动重试。
    /// </summary>
    void RefreshTargetMetrics()
    {
        if (target != measuredA || heightA < 0f)
        {
            float h = MeasureHeight(target);
            if (h > 0f)
            {
                measuredA = target;
                heightA = h;
                focusHeightA = ComputeFocusHeight(target, h);
            }
        }

        if (targetB == null)
        {
            measuredB = null;
            heightB = -1f;
            focusHeightB = 0f;
        }
        else if (targetB != measuredB || heightB < 0f)
        {
            float h = MeasureHeight(targetB);
            if (h > 0f)
            {
                measuredB = targetB;
                heightB = h;
                focusHeightB = ComputeFocusHeight(targetB, h);
            }
        }
    }

    /// <summary>当前该用的焦点高度。联动模式取两人里较高的那个，保证两人都在画面里。</summary>
    float ActiveFocusHeight()
    {
        if (heightA < 0f) return focusHeight;   // 还没量到，先用固定值兜底

        if (isLinked && heightB > 0f)
            return Mathf.Max(focusHeightA, focusHeightB);

        return focusHeightA;
    }

    /// <summary>当前该用的身高（用于距离缩放）。</summary>
    float ActiveHeight()
    {
        if (heightA < 0f) return referenceHeight;

        if (isLinked && heightB > 0f)
            return Mathf.Max(heightA, heightB);

        return heightA;
    }

    /// <summary>强制重新测量角色身高。改了角色模型/碰撞体后可以手动调一次。</summary>
    public void RemeasureTargets()
    {
        heightA = -1f;
        heightB = -1f;
        measuredA = null;
        measuredB = null;
        RefreshTargetMetrics();
    }

    #endregion

    Vector3 GetDesiredFocus()
    {
        float fh = ActiveFocusHeight();

        if (isHinting && hintTarget != null)
            return hintTarget.position + Vector3.up * fh;

        if (isLinked && targetB != null)
            return (target.position + targetB.position) * 0.5f + Vector3.up * fh;

        return target.position + Vector3.up * fh;
    }

    /// <summary>
    /// 相机基准距离：滚轮值 → 按角色身高缩放 → 联动模式按双人间距额外拉远。
    /// 狗矮，所以自动比人靠近一点，视野比例才对得上。
    /// </summary>
    float GetBaseDistance()
    {
        float d = enableZoom ? zoomDistance : distance;

        if (autoDistance && referenceHeight > 0.01f)
        {
            var profile = target != null ? target.GetComponent<CameraTargetProfile>() : null;
            if (profile != null && profile.overrideDistanceScale)
            {
                d *= profile.distanceScale;
            }
            else
            {
                float ratio = ActiveHeight() / referenceHeight;
                d *= Mathf.Lerp(1f, ratio, distanceScaleStrength);
            }
        }

        if (isLinked && targetB != null && linkedDistanceBonus > 0f)
        {
            Vector3 a = target.position; a.y = 0f;
            Vector3 b = targetB.position; b.y = 0f;
            d += Vector3.Distance(a, b) * linkedDistanceBonus;
        }

        return Mathf.Max(d, minCollisionDistance);
    }

    void UpdateCameraTransform(Vector3 focusPos)
    {
        // 1) 球面轨道：pitch/yaw 决定相机在角色周围的方位
        Quaternion orbit = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 dir = orbit * Vector3.back;   // 从焦点指向相机的方向（角色背后）

        // 基准距离先做一次平滑：这样切人/切狗时相机是"推拉"过去的，不会瞬移
        float wantedRaw = GetBaseDistance();
        if (!baseDistanceInit)
        {
            baseDistanceSmoothed = wantedRaw;
            baseDistanceInit = true;
        }
        else
        {
            baseDistanceSmoothed = Mathf.Lerp(
                baseDistanceSmoothed, wantedRaw, 1f - Mathf.Exp(-distanceBlendSpeed * Time.deltaTime));
        }
        float wanted = baseDistanceSmoothed;

        // 2) 防穿墙：用 SphereCast 而不是 Raycast，避免擦着墙角/柱子边缘钻出去
        float resolved = wanted;
        if (enableCameraCollision)
        {
            RaycastHit[] hits = Physics.SphereCastAll(
                focusPos, collisionRadius, dir, wanted, collisionMask, QueryTriggerInteraction.Ignore);

            foreach (var hit in hits)
            {
                if (hit.collider == null) continue;
                if (hit.distance <= 0.0001f) continue;      // 起点就重叠，SphereCast 会返回 0，忽略
                if (IsSelf(hit.transform)) continue;
                if (hit.distance < resolved) resolved = hit.distance;
            }

            if (resolved < wanted)
                resolved = Mathf.Max(resolved - collisionBuffer, minCollisionDistance);
        }

        // 撞墙时瞬间拉近（否则会穿模），离开墙后渐进拉回（否则会"弹"）
        if (resolved < currentDistance)
            currentDistance = resolved;
        else
            currentDistance = Mathf.Lerp(currentDistance, resolved, 1f - Mathf.Exp(-collisionRecoverSpeed * Time.deltaTime));

        Vector3 targetPos = focusPos + dir * currentDistance;

        // 3) 位置
        if (!camPosInitialized)
        {
            transform.position = targetPos;
            camPosInitialized = true;
        }
        else
        {
            transform.position = Vector3.SmoothDamp(
                transform.position, targetPos, ref velocityRef, 1f / Mathf.Max(followSpeed, 0.01f));
        }

        // 4) 关键：永远看向焦点，而不是用固定欧拉角。
        //    这样即使被墙压近、或位置平滑滞后，角色也始终在画面中心。
        Vector3 look = focusPos - transform.position;
        if (look.sqrMagnitude > 0.0001f)
            transform.rotation = Quaternion.LookRotation(look, Vector3.up);
    }

    bool IsSelf(Transform t)
    {
        if (t == null) return false;
        if (target != null && (t == target || t.IsChildOf(target))) return true;
        if (targetB != null && (t == targetB || t.IsChildOf(targetB))) return true;
        return false;
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

        RaycastHit[] hits = Physics.RaycastAll(
            focusPos, dir / dist, dist, obstructionMask, QueryTriggerInteraction.Ignore);

        foreach (var hit in hits)
        {
            if (IsSelf(hit.transform)) continue;

            // 连子物体一起隐藏：导入的 FBX 常常碰撞体在父物体、网格渲染器在子物体
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

    /// <summary>把镜头立刻甩到角色正后方（过场结束、传送后可以调）。</summary>
    public void SnapBehindTarget()
    {
        if (target == null) return;
        yaw = target.eulerAngles.y + initialYawOffset;
        pitch = Mathf.Clamp(angle, minAngle, maxAngle);
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
