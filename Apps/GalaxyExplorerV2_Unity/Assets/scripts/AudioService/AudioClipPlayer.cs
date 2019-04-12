using System.Collections;
using System.Collections.Generic;
using Microsoft.MixedReality.Toolkit.Core.EventDatum.Input;
using Microsoft.MixedReality.Toolkit.Core.Interfaces.InputSystem.Handlers;
using Microsoft.MixedReality.Toolkit.Core.Services;
using UnityEngine;

public class AudioClipPlayer : MonoBehaviour, IMixedRealityPointerHandler, IMixedRealityFocusHandler
{
    [SerializeField] private AudioId onFocus;
    [SerializeField] private AudioId onClick;

    private IAudioService audioService;
    
    void Awake()
    {
        audioService = MixedRealityToolkit.Instance.GetService<IAudioService>();
    }

    #region IMixedRealityPointerHandlerFunctions
    public void OnPointerUp(MixedRealityPointerEventData eventData)
    {
    }

    public void OnPointerDown(MixedRealityPointerEventData eventData)
    {
        audioService.PlayClip(onClick);
    }

    public void OnPointerClicked(MixedRealityPointerEventData eventData)
    {
    }
    #endregion

    #region IMixedRealityFocusHandlerFunctions
    public void OnBeforeFocusChange(FocusEventData eventData)
    {
    }

    public void OnFocusChanged(FocusEventData eventData)
    {
    }

    public void OnFocusEnter(FocusEventData eventData)
    {
        audioService.PlayClip(onFocus);
    }

    public void OnFocusExit(FocusEventData eventData)
    {
    }
    #endregion
}
