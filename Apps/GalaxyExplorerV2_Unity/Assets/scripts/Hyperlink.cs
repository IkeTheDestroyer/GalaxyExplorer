// Copyright Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.MixedReality.Toolkit.Input;
using System.Collections;
using UnityEngine;

namespace GalaxyExplorer
{
    public class Hyperlink : MonoBehaviour, IMixedRealityInputHandler
    {
        public string URL;

        private float _coolDownDuration = 1f;
        private bool _inCoolDown = false;

        public void OnInputDown(InputEventData eventData)
        {
            Debug.Log("Link clicked: " + URL.ToString());

            if (!string.IsNullOrEmpty(URL) && !_inCoolDown)
            {
                Application.OpenURL(URL);

                // Since events are currently fired twice, enforce a cooldown before another link can be clicked
                StartCoroutine(CoolDown());
            }
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