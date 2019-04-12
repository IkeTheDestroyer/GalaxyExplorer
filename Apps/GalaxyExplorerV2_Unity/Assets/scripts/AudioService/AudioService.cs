using System.Collections.Generic;
using Microsoft.MixedReality.Toolkit.Core.Definitions;
using Microsoft.MixedReality.Toolkit.Core.Services;
using Pools;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.Audio;

public class AudioService : BaseExtensionService, IAudioService
{
    private Dictionary<AudioId, AudioInfo> audioClipCache;
    private Dictionary<Transform, List<PoolableAudioSource>> playingCache;
    private ObjectPooler objectPooler;
    private Transform mainCameraTransform;
    private AudioServiceProfile audioProfile;
    
    public AudioService(string name, uint priority, BaseMixedRealityProfile profile) : base(name, priority, profile)
    {
#if UNITY_EDITOR
        if (!EditorApplication.isPlaying)
        {
            return;
        }
        
#endif

        audioProfile = ConfigurationProfile as AudioServiceProfile;
        if (audioProfile != null)
        {
            audioClipCache = new Dictionary<AudioId, AudioInfo>();
            playingCache = new Dictionary<Transform, List<PoolableAudioSource>>();
            foreach (var audioInfo in audioProfile.audioClips)
            {
                if (!audioClipCache.ContainsKey(audioInfo.audioId))
                {
                    audioClipCache.Add(audioInfo.audioId,audioInfo);
                }
            }
        }

        objectPooler = ObjectPooler.CreateObjectPool<PoolableAudioSource>(8);

        GetTarget(null);

    }

    public void PlayClip(AudioId audioId, Transform target)
    {
        if (audioClipCache != null && audioClipCache.ContainsKey(audioId))
        {
            var audioInfo = audioClipCache[audioId];
            var source = GetTargetSource(GetTarget(target));
            source.PlayClip(audioInfo.clip, audioInfo.volume);
        }
    }

    public void PlayClip(AudioClip clip, Transform target)
    {
        var source = GetTargetSource(GetTarget(target));
        source.PlayClip(clip);
    }

    public void PlayClip(AudioClip clip, out AudioSource playedSource, Transform target)
    {
        var source = GetTargetSource(GetTarget(target));
        playedSource = source.audioSource;
        source.PlayClip(clip);
    }

    public bool TryTransitionMixerSnapshot(string name, float transitionTime)
    {
        bool transitioned = false;

        if (audioProfile.musicAudioMixer)
        {
            AudioMixerSnapshot snapshot = audioProfile.musicAudioMixer.FindSnapshot(name);

            if (snapshot)
            {
                snapshot.TransitionTo(transitionTime);
                transitioned = true;
            }
            else
            {
                Debug.LogWarning("Couldn't find AudioMixer Snapshot with name " + name);
            }
        }

        return transitioned;
    }

    private Transform GetTarget(Transform target)
    {
        if (mainCameraTransform == null)
        {
            if (Camera.main != null)
            {
                mainCameraTransform = Camera.main.transform;
            }
        }
        return target == null ? mainCameraTransform : target;
    }

    private PoolableAudioSource GetTargetSource(Transform target)
    {
        PoolableAudioSource source = null;
        List<PoolableAudioSource> sources = null;
        if (playingCache.ContainsKey(target))
        {
            sources = playingCache[target];
            foreach (var poolableAudioSource in sources)
            {
                if (!poolableAudioSource.IsPlaying)
                {
                    source = poolableAudioSource;
                    break;
                }
            }
        }
        else
        {
            sources = new List<PoolableAudioSource>();
        }
        if (source == null)
        {
            source = objectPooler.GetNextObject<PoolableAudioSource>(parent:target);
            sources.Add(source);
        }
        playingCache[target] = sources;
        return source;
    }
}
