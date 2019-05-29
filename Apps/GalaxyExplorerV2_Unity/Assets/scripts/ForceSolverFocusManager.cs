using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForceSolverFocusManager : MonoBehaviour
{
    private ForceSolver[] _planetForceSolvers;
    private ForceSolver _currentlyActiveSolver, _currentlyFocusedDwellingSolver;

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
            forceSolver.SetToManipulate.AddListener(OnSolverManipulate);
            forceSolver.SetToFree.AddListener(OnSolverFree);
            forceSolver.SetToDwell.AddListener(OnSolverDwell);
        }
    }

    public void OnSolverDwell(ForceSolver solver)
    {
        if (_currentlyFocusedDwellingSolver == solver)
        {
            return;
        }

        var currentDwellProgress =
            _currentlyFocusedDwellingSolver == null ? 0f : _currentlyFocusedDwellingSolver.CurrentRelativeDwell;
        
        _currentlyFocusedDwellingSolver = solver;
        
        Debug.Assert(_currentlyFocusedDwellingSolver.ForceSetDwellTimer(currentDwellProgress));
    }

    public void OnSolverAttraction(ForceSolver solver)
    {
        if (_currentlyActiveSolver == solver)
        {
            return;
        }
        
        if (_currentlyActiveSolver != null)
        {
            _currentlyActiveSolver.ResetToRoot();
        }

        Debug.Assert(_currentlyFocusedDwellingSolver == null || _currentlyFocusedDwellingSolver == solver);
        _currentlyFocusedDwellingSolver = null;
        
        _currentlyActiveSolver = solver;
        foreach (var planetForceSolver in _planetForceSolvers)
        {
            if (solver == planetForceSolver)
            {
                continue;
            }
            planetForceSolver.ResetToRoot();
            planetForceSolver.EnableForce = false;
        }
    }

    public void OnSolverRoot(ForceSolver solver)
    {
        if (_currentlyActiveSolver == solver)
        {
            _currentlyActiveSolver = null;
        }

        if (_currentlyFocusedDwellingSolver == solver)
        {
            _currentlyFocusedDwellingSolver = null;
        }
    }

    public void OnSolverManipulate(ForceSolver solver)
    {
        Debug.Assert(_currentlyFocusedDwellingSolver == null || _currentlyFocusedDwellingSolver == solver);
        _currentlyFocusedDwellingSolver = null;
        
        if (solver == _currentlyActiveSolver)
        {
            return;
        }
        OnSolverAttraction(solver);
    }

    public void OnSolverFree(ForceSolver solver)
    {
        foreach (var planetForceSolver in _planetForceSolvers)
        {
            if (solver == planetForceSolver)
            {
                continue;
            }
            planetForceSolver.EnableForce = true;
        }
    }

    public void ResetAllForseSolvers()
    {
        foreach (var planetForceSolver in _planetForceSolvers)
        {
            planetForceSolver.ResetToRoot();
            planetForceSolver.EnableForce = true;
        }
        
    }
}
