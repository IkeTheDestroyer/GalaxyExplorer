using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Utilities;
using MRS.FlowManager;
using UnityEngine;
using Random = System.Random;

public class ForceTractorBeam : MonoBehaviour
{
    public event Action<ForceTractorBeam> Destroyed;

    private const int Subdivisions = 10;
    private const float s_divider = (float) 1.0d / int.MaxValue;

    private struct LineState
    {
        public Gradient Gradient;
        public AnimationCurve Width;

        public LineState(Gradient gradient, AnimationCurve width)
        {
            Gradient = new Gradient();
            Width = new AnimationCurve(width.keys);
            Gradient.SetKeys(gradient.colorKeys, gradient.alphaKeys);
            Gradient.mode = gradient.mode;
        }
    }

    private bool _wasActive;
    private MaterialPropertyBlock _tractorBeamMaterialPropertyBlock;
    private MeshFilter _meshFilter;
    private MeshRenderer _tractorMeshRenderer;
    private Vector4[] _points, _normals, _tangents, _biNormals;
    private readonly List<Vector4> _randoms = new List<Vector4>();
    private Dictionary<BaseMixedRealityLineRenderer, LineState> _oldLineStates = new Dictionary<BaseMixedRealityLineRenderer, LineState>();
    private Dictionary<BaseMixedRealityLineRenderer, LineState> _newLineStates = new Dictionary<BaseMixedRealityLineRenderer, LineState>();

    [Range(0,1)]
    public float Coverage;
    public Color TractorBeamColor = Color.green;
    public float TractorBeamWidth = .1f;

    private static readonly int ActivePropertyId = Shader.PropertyToID("_Active");
    private static readonly int CoveragePropertyId = Shader.PropertyToID("_Coverage");
    private static readonly int PointsPropertyId = Shader.PropertyToID("_Points");
    private static readonly int TangentsPropertyId = Shader.PropertyToID("_Tangents");
    private static readonly int NormalsPropertyId = Shader.PropertyToID("_Normals");
    private static readonly int BiNormalsPropertyId = Shader.PropertyToID("_BiNormals");

    public ShellHandRayPointer HandRayPointer { get; private set; }
    public int Seed;

    private void Awake()
    {
        _tractorMeshRenderer = GetComponent<MeshRenderer>();
        _meshFilter = GetComponent<MeshFilter>();
        _tractorBeamMaterialPropertyBlock = new MaterialPropertyBlock();
        _points = new Vector4[Subdivisions];
        _normals = new Vector4[Subdivisions];
        _biNormals = new Vector4[Subdivisions];
        _tangents = new Vector4[Subdivisions];
        HandRayPointer = GetComponentInParent<ShellHandRayPointer>();

//        foreach (var lineRenderer in HandRayPointer.LineRenderers)
//        {
//            _oldLineStates.Add(lineRenderer, new LineState(lineRenderer.LineColor, lineRenderer.LineWidth));
//            _newLineStates.Add(lineRenderer, new LineState(lineRenderer.LineColor, lineRenderer.LineWidth));
//            var newStat = _newLineStates[lineRenderer];
//            var currentColors = newStat.Gradient.colorKeys.ToList();
//            currentColors.Add(new GradientColorKey());
//            newStat.Gradient.colorKeys
//        }
        
        GenerateData();
    }

    private static float GetRandFloat01(Random random)
    {
        return random.Next() * s_divider;
    }

    private void GenerateData()
    {
        if(_meshFilter == null || _meshFilter.sharedMesh == null) return;

        var random = new Random(Seed);

        var mesh = _meshFilter.sharedMesh;
        
        var vertices = mesh.vertices;
        
        _randoms.Clear();
        
        for(var i=0;i<vertices.Length;i++)
        {
            _randoms.Add(new Vector4(
                GetRandFloat01(random),
                GetRandFloat01(random),
                GetRandFloat01(random),
                GetRandFloat01(random)
            ));
        }

        mesh.SetUVs(1, _randoms);
        mesh.UploadMeshData(false);
        GetComponent<MeshFilter>().mesh = mesh;
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
            _tractorBeamMaterialPropertyBlock.SetFloat(ActivePropertyId, 1f);
            UpdatePoints();
            UpdateBounds();
            _wasActive = true;
        }
        _tractorMeshRenderer.SetPropertyBlock(_tractorBeamMaterialPropertyBlock);
    }

//    private void UpdateLine()
//    {
//        foreach (var lineStatePair in _newLineStates)
//        {
//            var gradiant = lineStatePair.Value.Gradient;
//        }
//    }

    private void UpdatePoints()
    {
        for (var i = 0; i < Subdivisions; i++)
        {
            _points[i] = HandRayPointer.LineBase.GetPoint((float)i / (Subdivisions - 1));
        }
        for (var i = 0; i < Subdivisions; i++)
        {
            var a = Mathf.Max(0, i - 1);
            var b = Mathf.Min(Subdivisions - 1, i + 1);
            _tangents[i] = (_points[b] - _points[a]).normalized;
            var rotation = Quaternion.FromToRotation(Vector3.forward, _tangents[i]);
            _normals[i] = rotation * Vector3.up;
            _biNormals[i] = rotation * Vector3.right;
        }
        _tractorBeamMaterialPropertyBlock.SetFloat(CoveragePropertyId, Coverage);
        _tractorBeamMaterialPropertyBlock.SetVectorArray(PointsPropertyId, _points);
        _tractorBeamMaterialPropertyBlock.SetVectorArray(TangentsPropertyId, _tangents);
        _tractorBeamMaterialPropertyBlock.SetVectorArray(NormalsPropertyId, _normals);
        _tractorBeamMaterialPropertyBlock.SetVectorArray(BiNormalsPropertyId, _biNormals);
    }

    private void UpdateBounds()
    {
        var bounds = _meshFilter.sharedMesh.bounds;
        bounds.SetMinMax(Vector3.zero, transform.worldToLocalMatrix.MultiplyPoint(_points[Subdivisions-1]));
        _meshFilter.sharedMesh.bounds = bounds;
    }

    public void Dissipate()
    {
        Coverage = 0f;
        _tractorBeamMaterialPropertyBlock.SetFloat(ActivePropertyId, 0f);
        _wasActive = false;
    }
}
