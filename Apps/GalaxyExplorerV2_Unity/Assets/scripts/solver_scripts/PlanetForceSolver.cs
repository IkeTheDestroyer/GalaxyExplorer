using System;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
using UnityEngine;

public class PlanetForceSolver : ForceSolver
{
    private PlanetOffsetScaleController _scaleController;
    private Vector3 _editScaleTarget = Vector3.one;
    private float _oldBlend;
    private PlanetHighlighter _planetHighlighter;
    private IAudioService _audioService;
    private AudioSource _voAudioSource, _ambientAudioSource;

    [SerializeField]
    private AudioClip planetAudioClip;
    [SerializeField]
    private AudioClip planetAmbiantClip;

    protected override void Awake()
    {
        base.Awake();
        _scaleController = GetComponentInChildren<PlanetOffsetScaleController>();
        if (_scaleController != null)
        {
            _editScaleTarget = Vector3.one *
                               (PlanetOffsetScaleController.TargetEditScaleCm /
                                _scaleController.transform.localScale.x);

        }

        _planetHighlighter = GetComponentInChildren<PlanetHighlighter>();
        _audioService = MixedRealityToolkit.Instance.GetService<IAudioService>();
    }

    private void StopAudio()
    {
        if (_voAudioSource != null)
        {
            _voAudioSource.Stop();
        }

        if (_ambientAudioSource != null)
        {
            _ambientAudioSource.Stop();
        }
    }

    private void StartAudio()
    {
        _audioService.PlayClip(planetAudioClip, out _voAudioSource, transform);
        _audioService.PlayClip(planetAmbiantClip, out _ambientAudioSource, transform);
    }

    protected override void OnStartRoot()
    {
        base.OnStartRoot();
        _planetHighlighter.gameObject.SetActive(true);
        StopAudio();
    }

    protected override void OnStartAttraction()
    {
        base.OnStartAttraction();
        _planetHighlighter.gameObject.SetActive(false);
        StartAudio();
    }

    public override void SolverUpdate()
    {
        base.SolverUpdate();
        switch (ForceState)
        {
            case State.Root:
                GoalScale = Vector3.one;
                UpdateWorkingScaleToGoal();
                break;
            case State.Attraction:
                if (_scaleController == null) break;
                GoalScale = _editScaleTarget;
                UpdateWorkingScaleToGoal();
                break;
            case State.Free:
            case State.Manipulation:
            case State.None:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public override void OnFocusExit(FocusEventData eventData)
    {
        base.OnFocusExit(eventData);
        _planetHighlighter.SetFocused(false);
    }

    public override void OnFocusEnter(FocusEventData eventData)
    {
        base.OnFocusEnter(eventData);
        _planetHighlighter.SetFocused(true);
    }
}
