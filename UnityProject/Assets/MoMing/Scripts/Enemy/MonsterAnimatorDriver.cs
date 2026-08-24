using UnityEngine;

[RequireComponent(typeof(MonsterPatrol))]
public class MonsterAnimatorDriver : MonoBehaviour
{
    [SerializeField] private string idleState = "Idle";
    [SerializeField] private string walkState = "Walk";
    [SerializeField] private string runState = "Run";
    [SerializeField] private float crossFadeDuration = 0.15f;
    [SerializeField] private float movingSpeedThreshold = 0.2f;

    private Animator animator;
    private MonsterPatrol patrol;
    private Vector3 lastPosition;
    private string currentStateName;

    void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        patrol = GetComponent<MonsterPatrol>();
        lastPosition = transform.position;
    }

    void LateUpdate()
    {
        if (animator == null || !animator.enabled)
            return;

        string desired = ResolveDesiredState();
        if (desired == null || desired == currentStateName)
            return;

        animator.CrossFadeInFixedTime(desired, crossFadeDuration, 0, 0f);
        currentStateName = desired;
    }

    string ResolveDesiredState()
    {
        Vector3 position = transform.position;
        float deltaTime = Mathf.Max(Time.deltaTime, 0.0001f);
        float speed = (position - lastPosition).magnitude / deltaTime;
        lastPosition = position;

        string desired;
        if (patrol != null && patrol.IsChasing && speed > movingSpeedThreshold)
            desired = runState;
        else if (speed > movingSpeedThreshold)
            desired = walkState;
        else
            desired = idleState;

        return ResolveAnimationState(desired);
    }

    string ResolveAnimationState(string stateName)
    {
        if (string.IsNullOrEmpty(stateName))
            return null;

        if (animator.HasState(0, Animator.StringToHash(stateName)))
            return stateName;

        string fullName = "Base Layer." + stateName;
        return animator.HasState(0, Animator.StringToHash(fullName)) ? fullName : null;
    }
}