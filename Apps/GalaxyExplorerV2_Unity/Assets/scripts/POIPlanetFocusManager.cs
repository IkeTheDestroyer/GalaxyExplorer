using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class POIPlanetFocusManager : MonoBehaviour
{
    private PlanetForceSolver[] _planetForceSolvers;
    private PlanetForceSolver _currentlyFocusedPlanet;

    private void Awake()
    {
        _planetForceSolvers = GetComponentsInChildren<PlanetForceSolver>();
    }

    private void Start()
    {
        foreach (var forceSolver in _planetForceSolvers)
        {
            forceSolver.SetToAttract.AddListener(OnSolverAttraction);
            forceSolver.SetToRoot.AddListener(OnSolverRoot);
        }
    }

    public void OnSolverAttraction(ForceSolver solver)
    {
        var pForceSolver = solver as PlanetForceSolver;
        if (_currentlyFocusedPlanet != null)
        {
            _currentlyFocusedPlanet.ResetToRoot();
        }
        _currentlyFocusedPlanet = pForceSolver;
        foreach (var planetForceSolver in _planetForceSolvers)
        {
            if (pForceSolver == planetForceSolver)
            {
                continue;
            }
            planetForceSolver.ResetToRoot();
        }
    }

    public void OnSolverRoot(ForceSolver solver)
    {
        var pForceSolver = solver as PlanetForceSolver;
        if (_currentlyFocusedPlanet == pForceSolver)
        {
            _currentlyFocusedPlanet = null;
        }
    }
}
