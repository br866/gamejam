using UnityEngine;

public interface IFormalPushMover
{
    bool IsEngaged { get; }
    bool IsAttached(FormalPlayerActor actor);
    bool TryEngage(FormalPlayerActor actor);
    void Cancel();
    void Move(Vector3 worldDirection, bool pushingAnimation);
    void SetAttachedPushAnimation();
    void SetAttachedPullAnimation();
    void SetAttachedIdleAnimation();
}