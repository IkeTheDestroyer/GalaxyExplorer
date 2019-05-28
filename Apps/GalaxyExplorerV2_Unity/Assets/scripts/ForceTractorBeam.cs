using System;
using Microsoft.MixedReality.Toolkit.Input;
using UnityEngine;

public class ForceTractorBeam : MonoBehaviour
{
    private static int _subdivisions = 10;

    private bool _wasActive;
    private Material _tractorBeamMaterial;
    private Vector4[] _points;
    
    public ShellHandRayPointer HandRayPointer;
    [Range(0,1)]
    public float Coverage;

    private static readonly int ActivePropertyId = Shader.PropertyToID("_Active");
    private static readonly int CoveragePropertyId = Shader.PropertyToID("_Coverage");
    private static readonly int PointsPropertyId = Shader.PropertyToID("_Points");

    private void Awake()
    {
        _tractorBeamMaterial = GetComponent<MeshRenderer>().material; //instanced!
        _points = new Vector4[_subdivisions];
    }

    private void Update()
    {
        if (!_wasActive && !HandRayPointer.IsActive)
        {
            return;
        }
        if (!HandRayPointer.IsActive || Math.Abs(Coverage) < float.Epsilon)
        {
            _tractorBeamMaterial.SetFloat(ActivePropertyId, 0f);
            _wasActive = false;
            return;
        }

        if (!_wasActive && HandRayPointer.IsActive)
        {
            _tractorBeamMaterial.SetFloat(ActivePropertyId, 1f);
            
        }
        
        _tractorBeamMaterial.SetFloat(CoveragePropertyId, Coverage);
        UpdatePoints();
        _tractorBeamMaterial.SetVectorArray(PointsPropertyId, _points);
    }

    private void UpdatePoints()
    {
        for (var i = 0; i < _subdivisions; i++)
        {
            _points[i] = HandRayPointer.LineBase.GetPoint(i / (_subdivisions - 1));
        }
    }
}
