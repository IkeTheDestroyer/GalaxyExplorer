using System.Collections;
using Pools;
using UnityEngine;

public class PoolableAudioSource : APoolable
{
    private AudioSource _audioSource;

    public AudioSource AudioSource
    {
        get
        {
            if (_audioSource == null)
            {
                Debug.LogWarning("PoolableAudioSource Audio Source component should never be null (destroyed somewhere)");
                _audioSource = gameObject.AddComponent<AudioSource>();
            }
            return _audioSource;
        }
    }

    public bool IsPlaying
    {
        get
        {
            return AudioSource.isPlaying;
        }
    }

    public override bool IsActive
    {
        get { return AudioSource.isPlaying; }
    }

    private void Awake()
    {
        _audioSource = gameObject.AddComponent<AudioSource>();
    }

    public void PlayClip(
        AudioClip clip,
        float volume = 1)
    {
        AudioSource.clip = clip;
        AudioSource.volume = volume;
        AudioSource.time = 0;
        AudioSource.Play();
        StartCoroutine(DestroyWithDelay(clip.length));
    }

    private IEnumerator DestroyWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay + .1f);
        ReturnToPool();
    }

    protected override void Reset()
    {
        AudioSource.clip = null;
        AudioSource.volume = 1;
        AudioSource.outputAudioMixerGroup = null;
        base.Reset();
    }

    private void OnDisable()
    {
        Invoke("Reset", 0);
    }
}