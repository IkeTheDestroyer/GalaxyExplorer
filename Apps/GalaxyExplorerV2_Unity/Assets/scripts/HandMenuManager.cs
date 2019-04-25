using UnityEngine;

public class HandMenuManager : MonoBehaviour
{
    [SerializeField]
    private HandMenu _handMenuLeft;

    [SerializeField]
    private HandMenu _handMenuRight;

    [SerializeField]
    private AudioSource _movableAudioSource;

    [SerializeField]
    private AudioClip _menuAppearAudioClip;

    [SerializeField]
    private AudioClip _menuDisappearAudioClip;

    public bool IsAMenuVisible
    {
        get { return _handMenuLeft.IsVisible || _handMenuRight.IsVisible; }
    }

    public void PlayMenuAudio(Vector3 position, bool isAppearing)
    {
        if (isAppearing)
        {
            _movableAudioSource.clip = _menuAppearAudioClip;
        }
        else
        {
            _movableAudioSource.clip = _menuDisappearAudioClip;
        }

        _movableAudioSource.transform.position = position;
        _movableAudioSource.Play();
    }
}