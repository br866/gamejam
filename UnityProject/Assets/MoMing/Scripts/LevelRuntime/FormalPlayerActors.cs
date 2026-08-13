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
    }
}
