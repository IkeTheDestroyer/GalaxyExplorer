﻿// Copyright Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using HoloToolkit.Unity.InputModule;
using UnityEngine;

/// <summary>
/// Its attached to a poi if the poi is supposed to load a new planet scene when selected
/// </summary>
namespace GalaxyExplorer
{
    public class PlanetPOI : PointOfInterest
    {
        [SerializeField]
        private string SceneToLoad = "";

        [SerializeField]
        private GameObject Planet = null;

        public string GetSceneToLoad
        {
            get { return SceneToLoad; }
        }

        public GameObject PlanetObject
        {
            get { return Planet; }
        }

        protected override void Start()
        {
            base.Start();

            Collider[] allPlanetCollders = (Planet) ? Planet.GetComponentsInChildren<Collider>() : null;
            foreach (var item in allPlanetCollders)
            {
                allPoiColliders.Add(item);
            }
        }

        public override void OnInputClicked(InputClickedEventData eventData)
        {
            base.OnInputClicked(eventData);

            // Fade out card description material
            if (CardDescription)
            {
                StartCoroutine(GalaxyExplorerManager.Instance.GeFadeManager.FadeMaterial(CardDescriptionMaterial, GEFadeManager.FadeType.FadeOut, GalaxyExplorerManager.Instance.CardPoiManager.POIFadeOutTime, GalaxyExplorerManager.Instance.CardPoiManager.POIOpacityCurve));
            }

            GalaxyExplorerManager.Instance.TransitionManager.LoadNextScene(SceneToLoad);
        }

        public override void OnFocusEnter()
        {
            base.OnFocusEnter();
        }

        public override void OnFocusExit()
        {
            base.OnFocusExit();
        }
    }
}
