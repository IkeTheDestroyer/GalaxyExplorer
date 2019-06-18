// Copyright Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.MixedReality.Toolkit;
using System.Collections;
using UnityEngine;

namespace GalaxyExplorer
{
    public class AboutSlate : MonoBehaviour
    {
        public Material AboutMaterial;
        public GameObject Slate;
        public GameObject SlateContentParent;
        public float TransitionDuration = 1.0f;

        private SpiralGalaxy _galacticPlane;
        private bool _aboutIsActive;
        private bool _isTransitioning;
        private ZoomInOut _zoomInOut;
        private Collider[] _colliders;

        private void Start()
        {
            AboutMaterial.SetFloat("_TransitionAlpha", 0);
            _aboutIsActive = false;
            _isTransitioning = false;
            _zoomInOut = FindObjectOfType<ZoomInOut>();

            transform.localScale = transform.localScale * GalaxyExplorerManager.SlateScaleFactor;

            MixedRealityToolkit.InputSystem.Register(gameObject);
        }

        private void SceneManager_sceneLoaded(UnityEngine.SceneManagement.Scene arg0, UnityEngine.SceneManagement.LoadSceneMode arg1)
        {
            Hide();
        }

        private void OnEnable()
        {
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += SceneManager_sceneLoaded;
        }

        private void OnDisable()
        {
            UnityEngine.SceneManagement.SceneManager.sceneLoaded -= SceneManager_sceneLoaded;
        }

        public void ToggleAboutButton()
        {
            if (_aboutIsActive)
            {
                Hide();
            }
            else
            {
                Show();
            }
        }

        private void Show()
        {
            SetCollidersActivation(false);

            transform.position = Camera.main.transform.position + Camera.main.transform.forward * 2f;
            transform.rotation = Camera.main.transform.rotation;

            gameObject.SetActive(true);

            StartCoroutine(AnimateToOpacity(1));
        }

        private void Hide()
        {
            StartCoroutine(AnimateToOpacity(0));
        }

        private IEnumerator AnimateToOpacity(float target)
        {
            var timeLeft = TransitionDuration;

            DisableLinks();
            Slate.SetActive(true);
            _isTransitioning = true;

            while (timeLeft > 0)
            {
                AboutMaterial.SetFloat("_TransitionAlpha", Mathf.Lerp(target, 1 - target, timeLeft / TransitionDuration));
                yield return null;

                timeLeft -= Time.deltaTime;
            }

            _isTransitioning = false;
            AboutMaterial.SetFloat("_TransitionAlpha", target);

            if (target > 0)
            {
                EnableLinks();
                gameObject.SetActive(true);
                _aboutIsActive = true;
            }
            else
            {
                DisableLinks();
                Slate.SetActive(false);
                gameObject.SetActive(false);
                _aboutIsActive = false;
                SetCollidersActivation(true);
            }
        }

        private void EnableLinks()
        {
            _galacticPlane = FindObjectOfType<SpiralGalaxy>();

            if (_galacticPlane != null)
            {
                _galacticPlane.GetComponentInChildren<Collider>().enabled = false;
            }

            SlateContentParent.SetActive(true);
        }

        private void DisableLinks()
        {
            if (_galacticPlane != null)
            {
                _galacticPlane.GetComponentInChildren<Collider>().enabled = true;
            }

            SlateContentParent.SetActive(false);
        }

        private void SetCollidersActivation(bool enable)
        {
            if (enable)
            {
                // This colliders need to be tracked until the about slate gets disabled, since it would therwise look fr the colliders of the next scene and disable them
                if (_zoomInOut.GetNextScene != null)
                {
                    _colliders = _zoomInOut.GetNextScene.GetComponentsInChildren<Collider>();
                }
            }

            if (_colliders != null)
            {
                foreach (Collider collider in _colliders)
                {
                    collider.enabled = enable;
                }
            }
        }
    }
}