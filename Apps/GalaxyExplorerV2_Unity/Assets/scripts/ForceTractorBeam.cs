using System;
using Microsoft.MixedReality.Toolkit.Input;
using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class ForceTractorBeam : MonoBehaviour
{
    public event Action<ForceTractorBeam> Destroyed;

    private const int Subdivisions = 10;
    private const float s_divider = (float) 1.0d / int.MaxValue;

    private bool _wasActive;

    private LineRenderer _lineRenderer, _handRayLineRenderer;
    private MaterialPropertyBlock _tractorBeamMaterialPropertyBlock;

    [Range(0,1)]
    public float Coverage;
    public float TractorBeamWidth = .02f;


    public ShellHandRayPointer HandRayPointer { get; private set; }
    private static readonly int LineLength = Shader.PropertyToID("_LineLength");
    private static readonly int LineWidth = Shader.PropertyToID("_LineWidth");
    private static readonly int Active = Shader.PropertyToID("_Active");
    private static readonly int CoverageProperty = Shader.PropertyToID("_Coverage");

    private void Awake()
    {
        _tractorBeamMaterialPropertyBlock = new MaterialPropertyBlock();
        HandRayPointer = GetComponentInParent<ShellHandRayPointer>();
        _lineRenderer = GetComponent<LineRenderer>();
        _handRayLineRenderer = HandRayPointer.GetComponent<LineRenderer>();
        
        UpdateLine();
        Dissipate();
    }

    private void CopyLinePositions()
    {
        if (_lineRenderer.positionCount != _handRayLineRenderer.positionCount)
        {
            _lineRenderer.positionCount = _handRayLineRenderer.positionCount;
        }
        for (var i = 0; i < _handRayLineRenderer.positionCount; i++)
        {
            _lineRenderer.SetPosition(i,_handRayLineRenderer.GetPosition(i));
        }
    }

    private void OnDestroy()
    {
        Destroyed?.Invoke(this);
    }

    private void Update()
    {
        var isTargetingForceSolver = HandRayPointer.FocusTarget is ForceSolver;
        if (!_wasActive && !HandRayPointer.IsActive)
        {
            return;
        }
        if (!HandRayPointer.IsActive || !isTargetingForceSolver || Math.Abs(Coverage) < float.Epsilon)
        {
            Dissipate();
        }
        else
        {
            _lineRenderer.enabled = true;
            UpdateLine();
            _wasActive = true;
        }
        _tractorBeamMaterialPropertyBlock.SetFloat(CoverageProperty, Coverage);
        _lineRenderer.SetPropertyBlock(_tractorBeamMaterialPropertyBlock);
    }

    private void UpdateLine()
    {
        CopyLinePositions();
        _lineRenderer.widthMultiplier = TractorBeamWidth;
        var lineLength = HandRayPointer.LineBase.UnClampedWorldLength;
        _tractorBeamMaterialPropertyBlock.SetFloat(Active, 1f);
        _tractorBeamMaterialPropertyBlock.SetFloat(LineLength, lineLength);
        _tractorBeamMaterialPropertyBlock.SetFloat(LineWidth, TractorBeamWidth);
    }

    public void Dissipate()
    {
        Coverage = 0f;
        _tractorBeamMaterialPropertyBlock.SetFloat(Active, 0f);
        
        // have to check for null in case the pointer object has been destroyed while having focus
        if (_lineRenderer.Equals(null))
        {
            _lineRenderer.enabled = false;
        }
        
        _wasActive = false;
    }
}
