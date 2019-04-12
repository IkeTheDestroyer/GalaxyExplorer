using System;
using System.Collections.Generic;
using Microsoft.MixedReality.Toolkit.Core.Definitions;
using Microsoft.MixedReality.Toolkit.Core.Definitions.Utilities;
using UnityEngine;
using UnityEngine.Audio;

[CreateAssetMenu(menuName = "Mixed Reality Toolkit/Audio Service", fileName = "AudioServiceProfile", order = (int)CreateProfileMenuItemIndices.RegisteredServiceProviders)]
public class AudioServiceProfile : BaseMixedRealityProfile
{
    [SerializeField] public AudioMixer musicAudioMixer;
    [SerializeField] public List<AudioInfo> audioClips;
}

[Serializable]
public class AudioInfo
{
    public AudioId audioId;
    public AudioClip clip;
    [Range(0, 2)] public float volume = 1;
}

[Serializable]
public enum AudioId
{
    None = 0,
    ____UI___ = 1,
    Focus,
    Select,
    CardSelect,
    CardDeselect,
    ToolboxShow,
    ToolBoxHide,
    
    ____VoiceOvers___ = 5000,
    
    ____Music___ = 10000,
    PlanetaryView
}
