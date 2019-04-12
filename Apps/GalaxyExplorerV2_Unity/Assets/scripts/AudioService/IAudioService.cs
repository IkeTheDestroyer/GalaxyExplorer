using System;
using Microsoft.MixedReality.Toolkit.Core.Interfaces;
using UnityEngine;

public interface IAudioService<IdType> : IMixedRealityExtensionService where IdType : Enum 
{
    void PlayClip(IdType audioId, Transform target = null);

    void PlayClip(AudioClip clip, Transform target = null);

    void PlayClip(AudioClip clip, out AudioSource playedSource, Transform target = null);

    bool TryTransitionMixerSnapshot(string name, float transitionTime);
}

public interface IAudioService : IAudioService<AudioId>
{
    
}
