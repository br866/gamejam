using UnityEngine;

[CreateAssetMenu(menuName = "MoMing/Audio/SFX Mix Profile")]
public sealed class FormalSfxMixProfile : ScriptableObject
{
    public const string ResourcePath = "MoMing/FormalSfxMixProfile";
    [Range(0, 8)] public int maximumNearbyLamps = 3;
    [Range(5f, 30f)] public float lampRadius = 18f;
    [Range(5f, 30f)] public float ambienceRadius = 22f;
    public Vector2 draftInterval = new Vector2(17f, 26f);
    public Vector2 metalInterval = new Vector2(20f, 38f);
    [Min(0f)] public float metalSeparation = 5f;
    private static FormalSfxMixProfile instance;
    public static FormalSfxMixProfile Instance
    {
        get
        {
            if (instance == null) instance = Resources.Load<FormalSfxMixProfile>(ResourcePath);
            if (instance == null) instance = CreateInstance<FormalSfxMixProfile>();
            return instance;
        }
    }
    private void OnValidate()
    {
        draftInterval.x = Mathf.Max(13f, draftInterval.x);
        draftInterval.y = Mathf.Max(draftInterval.x, draftInterval.y);
        metalInterval.x = Mathf.Max(5f, metalInterval.x);
        metalInterval.y = Mathf.Max(metalInterval.x, metalInterval.y);
    }
}
