using UnityEngine;

/// <summary>Counts actual horizontal travel, including corners, without emitting on teleports.</summary>
public struct FootstepDistanceTracker
{
    private Vector3 previous;
    private float travel;
    private float stillTime;
    private bool initialized;

    public void Reset(Vector3 position)
    {
        previous = position;
        travel = 0f;
        stillTime = 0f;
        initialized = true;
    }

    public bool Tick(Vector3 position, float stride, bool audible, float deltaTime)
    {
        if (!initialized) { Reset(position); return false; }
        Vector3 delta = position - previous;
        previous = position;
        delta.y = 0f;
        float distance = delta.magnitude;
        if (!audible || distance > Mathf.Max(3.5f, 12f * deltaTime))
        {
            travel = 0f;
            stillTime = 0f;
            return false;
        }
        if (distance < Mathf.Max(0.000001f, 0.01f * deltaTime))
        {
            stillTime += deltaTime;
            if (stillTime > 0.1f) travel = 0f;
            return false;
        }
        stillTime = 0f;
        travel += distance;
        stride = Mathf.Max(0.1f, stride);
        if (travel < stride) return false;
        travel %= stride;
        return true;
    }
}
