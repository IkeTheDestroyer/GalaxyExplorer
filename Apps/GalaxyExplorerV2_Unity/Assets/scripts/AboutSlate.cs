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
        private bool _isActive;
        private bool _isTransitioning;

        private void Awake()
        {
            AboutMaterial.SetFloat("_TransitionAlpha", 0);
            _isActive = false;
            _isTransitioning = false;

            transform.localScale = transform.localScale * GalaxyExplorerManager.SlateScaleFactor;
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

        private void Start()
        {
            MixedRealityToolkit.InputSystem.Register(gameObject);
        }

        public void ToggleAboutButton()
        {
            if (_isActive)
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
            transform.position = Camera.main.transform.position + Camera.main.transform.forward * 2f;
            transform.rotation = Camera.main.transform.rotation;

            gameObject.SetActive(true);

            StartCoroutine(AnimateToOpacity(1));
        }

        private void Hide()
        {
            if (gameObject.activeSelf)
            {
                StartCoroutine(AnimateToOpacity(0));
            }
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
                _isActive = true;
            }
            else
            {
                DisableLinks();
                Slate.SetActive(false);
                gameObject.SetActive(false);
                _isActive = false;
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
    }
}