// Copyright Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GalaxyExplorer
{
    public class ToolManager : MonoBehaviour
    {
        public GameObject RaiseButton;
        public GameObject LowerButton;
        public GameObject ResetButton;
        public GameObject BackButton;

        public float MinZoom = 0.15f;
        public float LargestZoom = 3.0f;

        [SerializeField]
        private AnimationCurve toolsOpacityChange = null;

        [SerializeField]
        private float FadeToolsDuration = 1.0f;

        [HideInInspector]
        public bool ToolsVisible = false;

        private POIPlanetFocusManager POIPlanetFocusManager
        {
            get
            {
                if (_pOIPlanetFocusManager == null)
                {
                    _pOIPlanetFocusManager = FindObjectOfType<POIPlanetFocusManager>();
                }

                return _pOIPlanetFocusManager;
            }
        }

        public delegate void BackButtonNeedsShowingEventHandler(bool show);

        public event BackButtonNeedsShowingEventHandler BackButtonNeedsShowing;

        private ToolPanel panel;
        private POIPlanetFocusManager _pOIPlanetFocusManager;
        private Vector3 _defaultBackButtonLocalPosition;
        private float _fullMenuVisibleBackButtonX;

        public delegate void AboutSlateOnDelegate(bool enable);

        public AboutSlateOnDelegate OnAboutSlateOnDelegate;

        public delegate void BoundingBoxDelegate(bool enable);

        public BoundingBoxDelegate OnBoundingBoxDelegate;

        private float smallestZoom;

        private void Start()
        {
            panel = GetComponentInChildren<ToolPanel>(true) as ToolPanel;
            if (panel == null)
            {
                Debug.LogError("ToolManager couldn't find ToolPanel. Hiding and showing of Tools unavailable.");
            }

            RaiseButton.SetActive(false);
            BackButton.SetActive(false);
            ResetButton.SetActive(false);

            OnBackButtonNeedsShowing(false);

            panel.gameObject.SetActive(false);
            ToolsVisible = false;

            // Store the x value of the local position for the back button when all menu buttons are visible
            _fullMenuVisibleBackButtonX = BackButton.transform.localPosition.x;

            // Since reset is not visible during most of the app states, regard its local position as the default back button local position
            _defaultBackButtonLocalPosition = ResetButton.transform.localPosition;

            // Since the app starts with reset button not visible, move the back button to its spot instead
            BackButton.transform.localPosition = _defaultBackButtonLocalPosition;

            if (GalaxyExplorerManager.Instance.ViewLoaderScript)
            {
                GalaxyExplorerManager.Instance.ViewLoaderScript.OnSceneIsLoaded += OnSceneIsLoaded;
                GalaxyExplorerManager.Instance.ViewLoaderScript.OnLoadNewScene += OnLoadNewScene;
            }

            if (GalaxyExplorerManager.Instance.TransitionManager)
            {
                GalaxyExplorerManager.Instance.TransitionManager.OnResetMRSceneToOriginComplete.AddListener(OnSceneReset);
            }
            // if its unity editor and a non intro scene is active on start then make the menu visible
#if UNITY_EDITOR
            OnSceneIsLoaded();
#endif
        }

        private void OnSceneReset()
        {
            if (POIPlanetFocusManager)
            {
                _pOIPlanetFocusManager.ResetAllForseSolvers();
            }
            else
            {
                Debug.Log("No POIPlanetFocusManager found in currently loaded scenes");
            }
        }

        // Callback when a new scene is requested to be loaded
        public void OnLoadNewScene()
        {
            if (ToolsVisible)
            {
                HideTools();
            }
        }

        public void OnSceneIsLoaded()
        {
            StartCoroutine(OnSceneIsLoadedCoroutine());
        }

        // Callback when a new scene is loaded
        private IEnumerator OnSceneIsLoadedCoroutine()
        {
            // waiting necessary for events in flow manager to be called and
            // stage of intro flow to be correct when executing following code
            yield return new WaitForSeconds(1);

            if (!ToolsVisible && !GalaxyExplorerManager.Instance.TransitionManager.IsInIntroFlow)
            {
                // If tools/menu is not visible and intro flow has finished then make menu visible
                while (GalaxyExplorerManager.Instance.TransitionManager.InTransition)
                {
                    yield return null;
                }

                if (!GalaxyExplorerManager.IsHoloLens2)
                {
                    ShowToolPanel();
                }
                // If there is previous scene then user is able to go back so activate the back button
                BackButton?.SetActive(GalaxyExplorerManager.Instance.ViewLoaderScript.IsTherePreviousScene());
                CheckIfResetButtonNeedsShowing();
                OnBackButtonNeedsShowing(GalaxyExplorerManager.Instance.ViewLoaderScript.IsTherePreviousScene());
            }

            yield return null;
        }

        private void CheckIfResetButtonNeedsShowing()
        {
            if (!GalaxyExplorerManager.Instance.TransitionManager.IsInIntroFlow)
            {
                if (POIPlanetFocusManager != null && !ResetButton.activeInHierarchy)
                {
                    // When the POIPlanetFocusManager is present in the currently loaded scenes, this means we are in the solar system and the reset button should be visible
                    ResetButton.SetActive(true);
                    BackButton.transform.localPosition = new Vector3(_fullMenuVisibleBackButtonX, 0f, 0f);
                }
                else if (POIPlanetFocusManager == null && ResetButton.activeInHierarchy)
                {
                    // When the POIPlanetFocusManager isn't present in the currently loaded scenes, this means we're not in the solar system and the reset button shouldn't show up
                    ResetButton.SetActive(false);
                    BackButton.transform.localPosition = _defaultBackButtonLocalPosition;
                }
            }
        }

        private void OnBackButtonNeedsShowing(bool show)
        {
            if (BackButtonNeedsShowing != null)
            {
                BackButtonNeedsShowing(show);
            }
        }

        public void LowerTools()
        {
            panel.IsLowered = true;

            if (RaiseButton && LowerButton)
            {
                RaiseButton.SetActive(true);
                LowerButton.SetActive(false);
            }
        }

        public void RaiseTools()
        {
            panel.IsLowered = false;

            if (RaiseButton && LowerButton)
            {
                RaiseButton.SetActive(false);
                LowerButton.SetActive(true);
            }
        }

        [ContextMenu("Hide Tools")]
        public void HideTools()
        {
            StartCoroutine(HideToolsAsync());
        }

        [ContextMenu("Show Tools")]
        public void ShowToolPanel()
        {
            StartCoroutine(ShowToolsAsync());
        }

        // Hide tools by deactivating button colliders and fade out button materials
        private IEnumerator HideToolsAsync()
        {
            ToolsVisible = false;

            Fader[] allToolFaders = GetComponentsInChildren<Fader>();
            GalaxyExplorerManager.Instance.GeFadeManager.Fade(allToolFaders, GEFadeManager.FadeType.FadeOut, FadeToolsDuration, toolsOpacityChange);

            yield return null;
        }

        // Show tools by activating button colliders and fade in button materials
        private IEnumerator ShowToolsAsync()
        {
            if (GalaxyExplorerManager.IsHoloLensGen1 || GalaxyExplorerManager.IsImmersiveHMD)
            {
                panel.gameObject.SetActive(true);
                ToolsVisible = true;

                Fader[] allToolFaders = GetComponentsInChildren<Fader>();
                GalaxyExplorerManager.Instance.GeFadeManager.Fade(allToolFaders, GEFadeManager.FadeType.FadeIn, FadeToolsDuration, toolsOpacityChange);
                yield return null;
            }
        }

        public void OnAboutSlateButtonPressed(bool enable)
        {
            OnAboutSlateOnDelegate?.Invoke(enable);
        }
    }
}