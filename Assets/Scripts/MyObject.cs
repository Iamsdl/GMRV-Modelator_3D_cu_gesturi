using Leap.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class MyObject : MonoBehaviour
{
    public Mesh Mesh { get; set; }

    private Material material;
    public Color Color { 
        get
        {
            return material.color;
        }
        set
        {
            material.color = value;
        }
    }
    public FingerDirectionDetector fingerDirectionDetector;

    // Start is called before the first frame update
    void Start()
    {
        Initialise();
    }

    public void Initialise()
    {
        Mesh = GetComponent<MeshFilter>().mesh;
        material = GetComponent<MeshRenderer>().material;
        fingerDirectionDetector = GetComponent<FingerDirectionDetector>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
