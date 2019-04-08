using System.Collections;
using System.Collections.Generic;
using Microsoft.MixedReality.Toolkit.Core.Interfaces;
using Microsoft.MixedReality.Toolkit.Core.Interfaces.Devices;
using UnityEngine;

public interface IAudioService : IMixedRealityExtensionService
{
    void PlayClip(AudioId audioId);    
}
