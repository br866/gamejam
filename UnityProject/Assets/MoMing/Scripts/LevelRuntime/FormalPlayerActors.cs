using UnityEngine;

public class FormalPlayerActors : MonoBehaviour
{
    [SerializeField] private FormalPlayerActor human;
    [SerializeField] private FormalPlayerActor dog;

    public static FormalPlayerActors Instance { get; private set; }
    public FormalPlayerActor Human => human;
    public FormalPlayerActor Dog => dog;

    void Awake()
    {
        Instance = this;

        // 人和狗互相不碰撞，避免切换角色时把对方挤开。
        if (human != null && dog != null)
        {
            Collider humanCollider = human.GetComponentInChildren<Collider>();
            Collider dogCollider = dog.GetComponentInChildren<Collider>();
            if (humanCollider != null && dogCollider != null)
                Physics.IgnoreCollision(humanCollider, dogCollider, true);
        }
    }
}
