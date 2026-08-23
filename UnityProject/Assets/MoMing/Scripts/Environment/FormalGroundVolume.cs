using UnityEngine;

/// <summary>
/// 正式路线的统一地面体：一个 BoxCollider，顶面即全路线唯一可踩地面高度。
/// 只暴露 topHeight（世界坐标表面高度）与 thickness（向下厚度），
/// 碰撞体的位置/尺寸由本组件自动同步，不要手改 BoxCollider 的 Y。
/// 足迹(X/Z 尺寸)可直接在 Inspector 改 BoxCollider 的 Size.x/z。
/// </summary>
[ExecuteAlways]
[RequireComponent(typeof(BoxCollider))]
public class FormalGroundVolume : MonoBehaviour
{
    [SerializeField, Tooltip("世界坐标的可踩表面高度（全局唯一地面高度）")]
    private float topHeight = 0f;

    [SerializeField, Min(0.01f), Tooltip("碰撞体从表面向下的厚度")]
    private float thickness = 5f;

    public float TopHeight { get { return topHeight; } set { topHeight = value; SyncCollider(); } }
    public float Thickness => thickness;

    private bool warnedNonUniformTransform;

    public BoxCollider Box => GetComponent<BoxCollider>();

    void OnValidate()
    {
        SyncCollider();
    }

    void Update()
    {
        if (!Application.isPlaying)
            SyncCollider();
    }

    void OnEnable()
    {
        SyncCollider();
    }

    /// <summary>把 BoxCollider 的 Y 同步到 topHeight；X/Z 足迹保持用户设置。</summary>
    public void SyncCollider()
    {
        Transform self = transform;
        if (self.parent != null || self.lossyScale != Vector3.one || self.localRotation != Quaternion.identity)
        {
            if (!warnedNonUniformTransform)
            {
                warnedNonUniformTransform = true;
                Debug.LogWarning("[FormalGroundVolume] 必须挂在场景根级、无旋转无缩放。当前变换会影响世界对齐。", this);
            }
        }
        else
        {
            warnedNonUniformTransform = false;
        }

        BoxCollider box = Box;
        Vector3 center = box.center;
        center.y = topHeight - self.position.y - Mathf.Max(thickness, 0.01f) * 0.5f;
        box.center = center;
        Vector3 size = box.size;
        size.y = Mathf.Max(thickness, 0.01f);
        box.size = size;
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        BoxCollider box = Box;
        Gizmos.color = new Color(0.2f, 1f, 0.4f, 0.15f);
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawCube(box.center, box.size);
        Gizmos.color = new Color(0.2f, 1f, 0.4f, 0.8f);
        Vector3 topCenter = box.center;
        topCenter.y += box.size.y * 0.5f;
        Vector3 topSize = box.size;
        topSize.y = 0.02f;
        Gizmos.DrawWireCube(topCenter, topSize);
        UnityEditor.Handles.Label(
            transform.TransformPoint(topCenter) + Vector3.up * 0.25f,
            $"Ground top = {topHeight:F3}");
    }
#endif
}
