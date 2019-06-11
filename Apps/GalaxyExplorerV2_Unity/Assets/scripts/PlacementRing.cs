using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[ExecuteInEditMode]
public class PlacementRing : MonoBehaviour
{
    private MeshFilter _meshFilter;
    private Mesh _mesh;
    private MeshCollider _meshCollider;

    public float Diameter = 1f;
    public float Thickness = .05f;
    public int Detail = 50;

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
        _meshFilter = GetComponent<MeshFilter>();
        _meshCollider = GetComponent<MeshCollider>();
        _meshFilter.sharedMesh = _mesh;
        _meshCollider.sharedMesh = _mesh;
        GenerateRing();
    }

    private void OnValidate()
    {
        GenerateRing();
    }

    private void GenerateRing()
    {
        if (_mesh == null)
        {
            return;
        }   
        var slices = Mathf.Max(32, Mathf.FloorToInt(Mathf.PI * Diameter * Detail));
        var loops = Mathf.Max(16, Mathf.FloorToInt(Mathf.PI * Thickness * Detail));

        var vertices = new List<Vector3>();
        var indices = new List<int>();

        for (var i = 0; i < slices; i++)
        {
            var aa = 2*Mathf.PI * ((float)i / slices);
            var c = Diameter * .5f * new Vector3(Mathf.Cos(aa), 0f, Mathf.Sin(aa));
            for (var j = 0; j < loops; j++)
            {
                var ab = 2*Mathf.PI * ((float)j / loops);
                var d = Thickness * .5f * Mathf.Cos(ab) * Vector3.up;
                vertices.Add(Mathf.Sin(ab) * Thickness *.5f * c +d+c);
                
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
        _mesh.SetVertices(vertices);
        _mesh.SetTriangles(indices,0);
        _mesh.RecalculateNormals();
        _mesh.RecalculateTangents();
        _mesh.RecalculateBounds();
        _mesh.UploadMeshData(false);
    }
}
