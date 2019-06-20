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

        private bool _aboutIsActive;
        private ZoomInOut _zoomInOut;
        private Collider[] _otherSceneColliders;
        private Renderer[] _hyperlinkRenderers;

        private void Start()
        {
            AboutMaterial.SetFloat("_TransitionAlpha", 0f);
            _aboutIsActive = false;

            _hyperlinkRenderers = SlateContentParent.GetComponentsInChildren<Renderer>();
            SetHyperlinksTransitionAplha((0f));

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

            AboutMaterial.SetFloat("_TransitionAlpha", 1f);
            SetHyperlinksTransitionAplha((1f));
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
            SetActiveColliders(false);

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

            Slate.SetActive(true);
            SetActiveHyperlinkParent(true);

            while (timeLeft > 0)
            {
                float transitionAlpha = Mathf.Lerp(target, 1 - target, timeLeft / TransitionDuration);
                AboutMaterial.SetFloat("_TransitionAlpha", transitionAlpha);
                SetHyperlinksTransitionAplha((transitionAlpha));

                yield return null;

                timeLeft -= Time.deltaTime;
            }

            AboutMaterial.SetFloat("_TransitionAlpha", target);
            SetHyperlinksTransitionAplha(target);

            if (target > 0)
            {
                SetActiveHyperlinkParent(true);
                gameObject.SetActive(true);
                _aboutIsActive = true;
            }
            else
            {
                SetActiveHyperlinkParent(false);
                Slate.SetActive(false);
                gameObject.SetActive(false);
                _aboutIsActive = false;
                SetActiveColliders(true);
            }
        }

        private void SetHyperlinksTransitionAplha(float transitionAplha)
        {
            if (_hyperlinkRenderers == null) { return; }

            foreach (Renderer renderer in _hyperlinkRenderers)
            {
                Color newColor = renderer.material.GetColor("_FaceColor"); ;
                newColor.a = transitionAplha;
                renderer.material.SetColor("_FaceColor", newColor);
            }
        }

        private void SetActiveHyperlinkParent(bool isActive)
        {
            SlateContentParent.SetActive(isActive);
        }

        private void SetActiveColliders(bool enable)
        {
            if (!enable)
            {
                _otherSceneColliders = null;

                // This colliders need to be tracked until the about slate gets disabled, since this would otherwise look for the colliders of the next scene and disable them
                if (_zoomInOut.GetNextScene != null)
                {
                    _otherSceneColliders = _zoomInOut.GetNextScene.GetComponentsInChildren<Collider>();
                }
            }

            if (_otherSceneColliders != null)
            {
                foreach (Collider collider in _otherSceneColliders)
                {
                    collider.enabled = enable;
                }
            }
        }
    }
}