using System.Collections.Generic;
using Microsoft.MixedReality.Toolkit.Input;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[ExecuteInEditMode]
public class PlacementRing : MonoBehaviour, IMixedRealityPointerHandler
{
    private MeshFilter _meshFilter;
    private Mesh _mesh, _colliderMesh;
    private MeshCollider _meshCollider;
    private MaterialPropertyBlock _materialPropertyBlock;
    private MeshRenderer _meshRenderer;
    private Vector4[] _controllers = {Vector4.zero, Vector4.zero};
    private bool _pointerDown;

    public float Diameter = 1f;
    public float Thickness = .05f;
    public float ColliderThickness = 1.5f;
    public int Detail = 50;
    public float HighlightDistance = .1f;
    public ControllerTransformTracker ControllerTransformTracker;
    
    private static readonly int ControllerPos = Shader.PropertyToID("_ControllersPos");
    private static readonly int DistanceSqrd = Shader.PropertyToID("_DistanceSqrd");
    private static readonly int Pinch = Shader.PropertyToID("_Pinch");

    private void Start()
    {
        Init();
    }

    private void OnEnable()
    {
        Init();
    }

    private void Init()
    {
        _mesh = new Mesh();
        _colliderMesh = new Mesh();
        _meshFilter = GetComponent<MeshFilter>();
        _meshCollider = GetComponent<MeshCollider>();
        _meshFilter.sharedMesh = _mesh;
        _meshCollider.sharedMesh = _colliderMesh;
        _meshRenderer = GetComponent<MeshRenderer>();
        _materialPropertyBlock = new MaterialPropertyBlock();
        GenerateRing();
    }

    private void OnValidate()
    {
        GenerateRing();
    }

    private void Update()
    {
        var ltr = ControllerTransformTracker.LeftTransform;
        var ltrb = !ControllerTransformTracker.LeftSide;
        _controllers[0] =  ltrb ? Vector3.zero : ltr.position;
        _controllers[0].w =  ltrb ? 0f : 1f;
        var rtr = ControllerTransformTracker.RightTransform;
        var rtrb = !ControllerTransformTracker.RightSide;
        _controllers[1] = rtrb ? Vector3.zero : rtr.position;
        _controllers[1].w =  rtrb ? 0f : 1f;
        
        _materialPropertyBlock.SetVectorArray(ControllerPos, _controllers);
        _materialPropertyBlock.SetFloat(DistanceSqrd, HighlightDistance*HighlightDistance);
        _materialPropertyBlock.SetFloat(Pinch, _pointerDown?1f:0f);
        _meshRenderer.SetPropertyBlock(_materialPropertyBlock);
    }

    private void GenerateRing()
    {
        if (_mesh == null || _colliderMesh == null)
        {
            return;
        }   
        var slices = Mathf.Max(32, Mathf.FloorToInt(Mathf.PI * Diameter * Detail));
        var loops = Mathf.Max(16, Mathf.FloorToInt(Mathf.PI * Thickness * Detail));

        var vertices = new List<Vector3>();
        var colliderVertices = new List<Vector3>();
        var indices = new List<int>();

        for (var i = 0; i < slices; i++)
        {
            var aa = 2*Mathf.PI * ((float)i / slices);
            var c = Diameter * .5f * new Vector3(Mathf.Cos(aa), 0f, Mathf.Sin(aa));
            for (var j = 0; j < loops; j++)
            {
                var ab = 2*Mathf.PI * ((float)j / loops);
                var d = Thickness * .5f * Mathf.Cos(ab) * Vector3.up;
                var dc = ColliderThickness * .5f * Mathf.Cos(ab) * Vector3.up;
                vertices.Add(Mathf.Sin(ab) * Thickness *.5f * c +d+c);
                colliderVertices.Add(Mathf.Sin(ab) * ColliderThickness *.5f * c +dc+c);
                
                
                // add 2 tris from base vertex
                indices.Add(i*loops+j);                       //bot left
                indices.Add((i+1)%slices*loops+(j+1)%loops);  //top right
                indices.Add(i*loops+(j+1)%loops);             //top left
                indices.Add(i*loops+j);                       //bot left
                indices.Add((i+1)%slices*loops+j);            //bot right
                indices.Add((i+1)%slices*loops+(j+1)%loops);  //top right
            }
        }
        
        _mesh.Clear();
        _colliderMesh.Clear();
        _mesh.SetVertices(vertices);
        _colliderMesh.SetVertices(colliderVertices);
        _mesh.SetTriangles(indices,0);
        _colliderMesh.SetTriangles(indices, 0);
        _mesh.RecalculateNormals();
        _colliderMesh.RecalculateNormals();
        _mesh.RecalculateTangents();
        _colliderMesh.RecalculateTangents();
        _mesh.RecalculateBounds();
        _colliderMesh.RecalculateBounds();
        _mesh.UploadMeshData(false);
        _colliderMesh.UploadMeshData(false);
    }

    public void OnPointerUp(MixedRealityPointerEventData eventData)
    {
        _pointerDown = false;
    }

    public void OnPointerDown(MixedRealityPointerEventData eventData)
    {
        _pointerDown = true;
    }

    public void OnPointerClicked(MixedRealityPointerEventData eventData)
    {
    }
}
