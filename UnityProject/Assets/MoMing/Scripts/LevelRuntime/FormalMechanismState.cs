using UnityEngine;
using UnityEngine.Events;

public enum FormalMechanismResetPolicy
{
    Permanent,
    Resettable
}

public class FormalMechanismState : MonoBehaviour, IFormalLevelTemporaryState, IFormalLevelPermanentState
{
    [SerializeField] private FormalMechanismResetPolicy resetPolicy = FormalMechanismResetPolicy.Resettable;
    [SerializeField] private UnityEvent onCompleted;
    [SerializeField] private UnityEvent onReset;

    public bool IsComplete { get; private set; }
    public bool IsPermanent => resetPolicy == FormalMechanismResetPolicy.Permanent;

    void Awake()
    {
        FormalLevelController level = FormalLevelActors.FindLevelController(gameObject.scene);
        if (level != null)
            level.RegisterTemporaryState(this);
    }

    public void Complete()
    {
        if (IsComplete)
            return;

        IsComplete = true;
        onCompleted?.Invoke();
    }

    public void ResetTemporaryState()
    {
        if (IsPermanent || !IsComplete)
            return;

        IsComplete = false;
        onReset?.Invoke();
    }
}
