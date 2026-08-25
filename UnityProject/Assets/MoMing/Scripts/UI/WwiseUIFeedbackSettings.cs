using System;
using UnityEngine;

/// <summary>
/// Shared authored Wwise references for UI feedback. Keeping these as serialized
/// Wwise Types lets the integration load the matching Auto-Defined SoundBanks.
/// </summary>
public sealed class WwiseUIFeedbackSettings : ScriptableObject
{
    public const string ResourcesPath = "MoMing/WwiseUIFeedbackSettings";

    [SerializeField] private AK.Wwise.Event hoverEvent = new AK.Wwise.Event();
    [SerializeField] private AK.Wwise.Event clickEvent = new AK.Wwise.Event();
    [SerializeField] private AK.Wwise.Event fluorescentLightEvent = new AK.Wwise.Event();
    [SerializeField] private AK.Wwise.RTPC musicVolumeRtpc = new AK.Wwise.RTPC();
    [SerializeField] private AK.Wwise.RTPC sfxVolumeRtpc = new AK.Wwise.RTPC();

    public AK.Wwise.Event HoverEvent => hoverEvent;
    public AK.Wwise.Event ClickEvent => clickEvent;
    public AK.Wwise.Event FluorescentLightEvent => fluorescentLightEvent;
    public AK.Wwise.RTPC MusicVolumeRtpc => musicVolumeRtpc;
    public AK.Wwise.RTPC SfxVolumeRtpc => sfxVolumeRtpc;

    public bool HasValidHoverEvent => hoverEvent != null && hoverEvent.IsValid();
    public bool HasValidClickEvent => clickEvent != null && clickEvent.IsValid();
    public bool HasValidFluorescentLightEvent =>
        fluorescentLightEvent != null && fluorescentLightEvent.IsValid();
    public bool HasValidMusicVolumeRtpc => musicVolumeRtpc != null && musicVolumeRtpc.IsValid();
    public bool HasValidSfxVolumeRtpc => sfxVolumeRtpc != null && sfxVolumeRtpc.IsValid();
    public bool IsConfigured => HasValidHoverEvent && HasValidClickEvent;

#if UNITY_EDITOR
    /// <summary>
    /// Uses Wwise's own reference creation path. This avoids hand-authoring the
    /// generated WwiseEventReference assets and remains safe to run repeatedly.
    /// </summary>
    public void ConfigureEditorReferences(
        string hoverName,
        Guid hoverGuid,
        string clickName,
        Guid clickGuid,
        string fluorescentLightName,
        Guid fluorescentLightGuid,
        string musicVolumeName,
        Guid musicVolumeGuid,
        string sfxVolumeName,
        Guid sfxVolumeGuid)
    {
        if (hoverEvent == null)
            hoverEvent = new AK.Wwise.Event();
        if (clickEvent == null)
            clickEvent = new AK.Wwise.Event();
        if (fluorescentLightEvent == null)
            fluorescentLightEvent = new AK.Wwise.Event();
        if (musicVolumeRtpc == null)
            musicVolumeRtpc = new AK.Wwise.RTPC();
        if (sfxVolumeRtpc == null)
            sfxVolumeRtpc = new AK.Wwise.RTPC();

        if (!hoverEvent.IsValid()
            || hoverEvent.Name != hoverName
            || hoverEvent.ObjectReference.Guid != hoverGuid)
        {
            hoverEvent.SetupReference(hoverName, hoverGuid);
        }

        if (!clickEvent.IsValid()
            || clickEvent.Name != clickName
            || clickEvent.ObjectReference.Guid != clickGuid)
        {
            clickEvent.SetupReference(clickName, clickGuid);
        }

        if (!fluorescentLightEvent.IsValid()
            || fluorescentLightEvent.Name != fluorescentLightName
            || fluorescentLightEvent.ObjectReference.Guid != fluorescentLightGuid)
        {
            fluorescentLightEvent.SetupReference(fluorescentLightName, fluorescentLightGuid);
        }

        if (!musicVolumeRtpc.IsValid()
            || musicVolumeRtpc.Name != musicVolumeName
            || musicVolumeRtpc.ObjectReference.Guid != musicVolumeGuid)
        {
            musicVolumeRtpc.SetupReference(musicVolumeName, musicVolumeGuid);
        }

        if (!sfxVolumeRtpc.IsValid()
            || sfxVolumeRtpc.Name != sfxVolumeName
            || sfxVolumeRtpc.ObjectReference.Guid != sfxVolumeGuid)
        {
            sfxVolumeRtpc.SetupReference(sfxVolumeName, sfxVolumeGuid);
        }
    }
#endif
}
