using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(FormalDoor))]
public class FormalDoorEditor : Editor
{
    private bool isPreviewing;
    private double previewStartedAt;
    private Quaternion previewStartRotation;
    private Quaternion previewEndRotation;
    private Transform previewPivot;

    void OnEnable()
    {
        EditorApplication.update += UpdatePreview;
    }

    void OnDisable()
    {
        EditorApplication.update -= UpdatePreview;
        StopPreview();
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        FormalDoor door = (FormalDoor)target;
        EditorGUILayout.Space();
        EditorGUILayout.HelpBox(
            "Close is DoorPivot local rotation 0, 0, 0. Open is DoorPivot local rotation 0, Open Angle, 0. " +
            "Use a negative Open Angle to rotate the other way. Preview animation remains at its final state.",
            MessageType.Info);

        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Preview Open Animation"))
                StartPreview(door, true);

            if (GUILayout.Button("Preview Close Animation"))
                StartPreview(door, false);
        }

        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Set Open Immediately"))
                SetImmediate(door, true);

            if (GUILayout.Button("Set Closed Immediately"))
                SetImmediate(door, false);
        }
    }

    void OnSceneGUI()
    {
        FormalDoor door = (FormalDoor)target;
        Transform pivot = door.VisualPivot;
        if (pivot == null)
            return;

        Handles.color = Color.green;
        Handles.ArrowHandleCap(0, pivot.position, Quaternion.LookRotation(pivot.up), 0.7f, EventType.Repaint);

        Handles.color = door.OpenAngle >= 0f ? Color.cyan : Color.magenta;
        Vector3 openDirection = Quaternion.AngleAxis(door.OpenAngle, pivot.up) * pivot.forward;
        Handles.ArrowHandleCap(0, pivot.position, Quaternion.LookRotation(openDirection), 0.9f, EventType.Repaint);
        Handles.Label(pivot.position + pivot.up * 0.45f, "Open");
    }

    void StartPreview(FormalDoor door, bool opening)
    {
        Transform pivot = door.VisualPivot;
        if (pivot == null)
            return;

        StopPreview();
        previewPivot = pivot;
        previewStartRotation = opening ? door.ClosedRotation : door.OpenRotation;
        previewEndRotation = opening ? door.OpenRotation : door.ClosedRotation;
        previewStartedAt = EditorApplication.timeSinceStartup;
        isPreviewing = true;
    }

    void UpdatePreview()
    {
        if (!isPreviewing || previewPivot == null)
            return;

        FormalDoor door = (FormalDoor)target;
        float duration = Quaternion.Angle(previewStartRotation, previewEndRotation) / Mathf.Max(door.OpenSpeed, 0.01f);
        float progress = duration <= 0f
            ? 1f
            : Mathf.Clamp01((float)((EditorApplication.timeSinceStartup - previewStartedAt) / duration));
        previewPivot.localRotation = Quaternion.Slerp(previewStartRotation, previewEndRotation, Mathf.SmoothStep(0f, 1f, progress));
        SceneView.RepaintAll();

        if (progress >= 1f)
            isPreviewing = false;
    }

    void StopPreview()
    {
        isPreviewing = false;
        previewPivot = null;
    }

    static void SetImmediate(FormalDoor door, bool open)
    {
        if (door.VisualPivot == null)
            return;

        Undo.RecordObject(door.VisualPivot, open ? "Set Door Open" : "Set Door Closed");
        door.SetStateImmediate(open);
        EditorUtility.SetDirty(door.VisualPivot);
        SceneView.RepaintAll();
    }
}
