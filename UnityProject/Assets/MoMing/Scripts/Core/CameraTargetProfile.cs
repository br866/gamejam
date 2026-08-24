using UnityEngine;

/// <summary>
/// 可选组件：挂在人 / 狗身上，手动覆盖 CameraFollow 自动算出来的镜头参数。
///
/// 不挂这个组件时，CameraFollow 会按角色碰撞体的实际身高自动算焦点高度和距离，
/// 一般够用了。只有当某个角色（比如狗）自动值不好看时，才挂上来手调。
/// </summary>
[DisallowMultipleComponent]
public class CameraTargetProfile : MonoBehaviour
{
    [Header("焦点高度覆盖")]
    [Tooltip("勾上后，相机对准的高度用下面的固定值，不再按身高自动算")]
    public bool overrideFocusHeight = false;

    [Tooltip("相机对准角色脚底往上多少米。狗建议 0.4~0.7，人建议 1.2~1.6")]
    public float focusHeight = 1.4f;

    [Header("距离覆盖")]
    [Tooltip("勾上后，相机距离 = CameraFollow 的 distance × 下面的倍数")]
    public bool overrideDistanceScale = false;

    [Tooltip("距离倍数。1 = 和人一样远，0.7 = 近三成（狗一般用 0.6~0.8）")]
    [Range(0.2f, 2f)]
    public float distanceScale = 1f;
}
