// Copyright Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.MixedReality.Toolkit.Input;
using System.Collections;
using UnityEngine;

namespace GalaxyExplorer
{
    public class Hyperlink : MonoBehaviour, IMixedRealityInputHandler
    {
        [SerializeField]
        private string URL;

        private float _coolDownDuration = 1f;
        private bool _inCoolDown = false;

        public void OpenURL()
        {
            Debug.Log("Link clicked by mouse: " + URL.ToString());

            if (!string.IsNullOrEmpty(URL) && !_inCoolDown)
            {
                Application.OpenURL(URL);

                // Since events are currently fired twice, enforce a cooldown before another link can be clicked
                StartCoroutine(CoolDown());
            }
        }

        public void OnInputDown(InputEventData eventData = null)
        {
            Debug.Log("Link clicked: " + URL.ToString());

            OpenURL();
        }

        private IEnumerator CoolDown()
        {
            _inCoolDown = true;

            yield return new WaitForSeconds(_coolDownDuration);

            _inCoolDown = false;
        }

        public void OnInputUp(InputEventData eventData)
        {
        }
    }
}