using System.Collections;
using Pools;
using UnityEngine;

public class PoolableAudioSource : APoolable
{
    [SerializeField] public AudioSource audioSource;

    private Coroutine updateRoutine;

    public bool IsPlaying
    {
        get { return audioSource.isPlaying; }
    }

    public override bool IsActive
    {
        get {  return audioSource.isPlaying; }
    }

    private void Awake()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
    }
    
    public void PlayClip(
        AudioClip clip, 
        float volume = 1) 
    {
        audioSource.clip = clip;
        audioSource.volume = volume;
        audioSource.time = 0;
        audioSource.Play();
        if (updateRoutine != null)
        {
            StopCoroutine(updateRoutine);
        }
        updateRoutine = StartCoroutine(SlowUpdate());
    }

    private IEnumerator SlowUpdate()
    {
        var waitForOneSecond = new WaitForSeconds(1);
        while (gameObject.activeInHierarchy)
        {
            yield return waitForOneSecond;
            if (!IsPlaying)
            {
                Destroy();
            }
        }
        updateRoutine = null;
    }


    public override void Destroy()
    {
        audioSource.clip = null;
        audioSource.volume = 1;
        audioSource.outputAudioMixerGroup = null;
        base.Destroy();
    }

    private void OnDisable()
    {
        if (updateRoutine != null)
        {
            StopCoroutine(updateRoutine);
        }
        updateRoutine = null;
    }
}
