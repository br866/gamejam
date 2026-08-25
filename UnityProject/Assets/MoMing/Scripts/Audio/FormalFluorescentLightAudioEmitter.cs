using UnityEngine;

/// <summary>
/// Owns the single Wwise playback instance emitted by one decorative ceiling lamp.
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(AkGameObj))]
public sealed class FormalFluorescentLightAudioEmitter : MonoBehaviour
{
    private AK.Wwise.Event lightEvent;
    private uint playingId = AkUnitySoundEngine.AK_INVALID_PLAYING_ID;

    public void Initialize(AK.Wwise.Event configuredEvent)
    {
        lightEvent = configuredEvent;
        TryPost();
    }

    private void OnEnable()
    {
        TryPost();
    }

    private void OnDisable()
    {
        StopPlayback();
    }

    private void OnDestroy()
    {
        StopPlayback();
    }

    private void TryPost()
    {
        if (!isActiveAndEnabled
            || playingId != AkUnitySoundEngine.AK_INVALID_PLAYING_ID
            || lightEvent == null
            || !lightEvent.IsValid())
        {
            return;
        }

        playingId = lightEvent.Post(gameObject);
    }

    private void StopPlayback()
    {
        if (playingId == AkUnitySoundEngine.AK_INVALID_PLAYING_ID)
            return;

        AkUnitySoundEngine.ExecuteActionOnPlayingID(
            AkActionOnEventType.AkActionOnEventType_Stop,
            playingId,
            100,
            AkCurveInterpolation.AkCurveInterpolation_Linear);
        playingId = AkUnitySoundEngine.AK_INVALID_PLAYING_ID;
    }
}
