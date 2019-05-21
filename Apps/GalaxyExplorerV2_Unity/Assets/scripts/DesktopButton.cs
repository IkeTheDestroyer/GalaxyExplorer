// Copyright Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

//using HoloToolkit.Examples.InteractiveElements;
//using Microsoft.MixedReality.Toolkit.SDK.UX.Interactable.Events;
using Microsoft.MixedReality.Toolkit.Input;
using UnityEngine;
using UnityEngine.Events;

namespace GalaxyExplorer
{
    // Galaxy Explorer button based on MRTK button

    public class DesktopButton : MonoBehaviour
    {
        [Header("GEInteractiveToggle members")]
        [SerializeField]
        [Tooltip("Is button that stays active even if another button is selected. Its deactivated only if user explicitly selects it again")]
        private bool isPrimaryButton = false;

        public bool IsPrimaryButton
        {
            get { return isPrimaryButton; }
        }

        public UnityEvent OnGazeSelect;
        public UnityEvent OnGazeDeselect;
    }
}