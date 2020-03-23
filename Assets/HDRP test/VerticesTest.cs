using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VerticesTest : MonoBehaviour
{
    private Mesh Mesh;
    public Vector3[] vertices;
    public int[] indices;
    public Vector3[] normals;
    private GameObject[] selectedVertices;

    // Start is called before the first frame update
    void Start()
    {
        Mesh = this.GetComponent<MeshFilter>().mesh;
        this.vertices = Mesh.vertices;
        this.indices = Mesh.triangles;
        this.normals = Mesh.normals;
    }

    // Update is called once per frame
    void Update()
    {
        Mesh.vertices = vertices;
        Mesh.triangles = indices;
        Mesh.normals = normals;
    }
}
