using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 体型审计：报告每个 FormalPlayerActor 的模型实测高度、脚底偏差(sink)、
/// 胶囊与模型的贴合差、Body 缩放值。选中若干 actor 则只审它们，否则审全部。
/// </summary>
public static class FormalActorBodyAudit
{
    [MenuItem("Tools/Formal/Actor/Audit Body Fit")]
    static void Audit()
    {
        FormalPlayerActor[] actors = Selection.gameObjects.Length > 0
            ? Selection.gameObjects.Select(go => go.GetComponent<FormalPlayerActor>()).Where(a => a != null).ToArray()
            : Object.FindObjectsOfType<FormalPlayerActor>(true);
        if (actors.Length == 0)
        {
            Debug.LogWarning("[FormalActorBodyAudit] 没有找到 FormalPlayerActor（或选中物体不带该组件）。");
            return;
        }

        StringBuilder sb = new StringBuilder();
        sb.AppendLine($"[FormalActorBodyAudit] actors={actors.Length}");
        foreach (FormalPlayerActor actor in actors)
        {
            float footY = actor.transform.position.y;
            Renderer[] renderers = actor.GetComponentsInChildren<Renderer>(false);
            if (renderers.Length == 0)
            {
                sb.AppendLine($"{actor.name}: 无渲染器（视觉未加载？）");
                continue;
            }

            Bounds model = renderers[0].bounds;
            foreach (Renderer r in renderers)
                model.Encapsulate(r.bounds);

            CapsuleCollider capsule = actor.GetComponentInChildren<CapsuleCollider>();
            Transform body = actor.transform.Find("Body");

            float sink = footY - model.min.y;
            sb.AppendLine($"{actor.name}: bodyScale={(body != null ? body.localScale.x.ToString("F3") : "无Body!")}"
                + $" | modelHeight={model.size.y:F3} sink={sink:F3}"
                + $" | capsuleWorld[{capsule.bounds.min.y:F2}..{capsule.bounds.max.y:F2}]"
                + $" vs model[{model.min.y:F2}..{model.max.y:F2}]"
                + $" topGap={capsule.bounds.max.y - model.max.y:F3}");

            if (System.Math.Abs(sink) > 0.02f)
                sb.AppendLine($"  ^ 警告: 脚底偏差 {sink:F3} 超过 2cm，请微调 Loader 偏移 Y。");
        }
        Debug.Log(sb.ToString());
    }
}
