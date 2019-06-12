// Copyright Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.MixedReality.Toolkit.Input;

using System;
using UnityEngine;

namespace GalaxyExplorer
{
    public class Hyperlink : MonoBehaviour, IMixedRealityInputHandler
    {
        public string URL;

        public event Action Clicked;

        public void OnInputDown(InputEventData eventData)
        {
            Debug.Log("Link clicked: " + URL.ToString());

            if (Clicked != null)
            {
                Clicked();
            }

            if (!string.IsNullOrEmpty(URL))
            {
                //#if NETFX_CORE
                //                            UnityEngine.WSA.Application.InvokeOnUIThread(() =>
                //                            {
                //                                var uri = new System.Uri(URL);
                //                                var unused = Windows.System.Launcher.LaunchUriAsync(uri);
                //                            }, false);
                //#else
                Application.OpenURL(URL);
                //#endif
            }
        }

        public void OnInputUp(InputEventData eventData)
        {
            throw new NotImplementedException();
        }

        //private void OnMouseDown()
        //{
        //    OnInputUp(null);
        //}

        //public void OnHoldCompleted()
        //{
        //    OnInputUp(null);
        //}

        //public void OnTouchpadReleased(InputEventData eventData)
        //{
        //    OnInputClicked(null);

        //    eventData.Use();
        //}

        //        public void OnInputClicked(InputClickedEventData eventData)
        //        {
        //            if (Clicked != null)
        //            {
        //                Clicked();
        //            }

        //            if (!string.IsNullOrEmpty(URL))
        //            {
        //#if NETFX_CORE
        //                UnityEngine.WSA.Application.InvokeOnUIThread(() =>
        //                {
        //                    var uri = new System.Uri(URL);
        //                    var unused = Windows.System.Launcher.LaunchUriAsync(uri);
        //                }, false);
        //#else
        //                Application.OpenURL(URL);
        //#endif
        //            }
        //        }
    }
}