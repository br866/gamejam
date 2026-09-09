using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(AkGameObj))]
public sealed class FormalFluorescentLightAudioEmitter : MonoBehaviour
{
    private AK.Wwise.Event lightEvent;
    private uint playingId;
    private bool audible;
    public bool IsAudible => playingId != 0;
    public void Initialize(AK.Wwise.Event configuredEvent) { lightEvent = configuredEvent; }
    public void SetAudible(bool value)
    {
        audible = value;
        if (value) TryPost();
        else FormalSfxEvents.Stop(ref playingId, 250);
    }
    private void TryPost()
    {
        if (!audible || !isActiveAndEnabled || playingId != 0 || !FormalGameplayState.CanSimulate ||
            !AkUnitySoundEngine.IsInitialized() || lightEvent == null || !lightEvent.IsValid() ||
            !lightEvent.WwiseObjectReference.IsAutoBankLoaded) return;
        playingId = lightEvent.Post(gameObject);
    }
    private void OnEnable() => TryPost();
    private void OnDisable() => FormalSfxEvents.Stop(ref playingId, 150);
    private void OnDestroy() => FormalSfxEvents.Stop(ref playingId, 150);
}
