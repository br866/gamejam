using UnityEngine;

public class FormalOrderedMechanism : MonoBehaviour, IFormalLevelTemporaryState
{
    [SerializeField] private FormalMechanismState[] orderedStates;
    [SerializeField] private FormalMechanismState completionState;

    private int nextIndex;

    void Awake()
    {
        FormalLevelController level = FormalLevelActors.FindLevelController(gameObject.scene);
        if (level != null)
            level.RegisterTemporaryState(this);
    }

    public void CompleteNext(FormalMechanismState state)
    {
        if (state == null || completionState == null || completionState.IsComplete ||
            orderedStates == null || nextIndex >= orderedStates.Length || orderedStates[nextIndex] != state)
            return;

        state.Complete();
        nextIndex++;
        if (nextIndex == orderedStates.Length)
            completionState.Complete();
    }

    public void ResetTemporaryState()
    {
        nextIndex = 0;
    }
}
