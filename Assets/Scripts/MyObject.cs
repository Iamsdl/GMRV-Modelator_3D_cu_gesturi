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

    public bool IsSelected;

    public ProximityDetector proximityDetector;

    // Start is called before the first frame update
    void Start()
    {
        Initialise();
    }

    public void Initialise()
    {
        Mesh = GetComponent<MeshFilter>().mesh;
        material = GetComponent<MeshRenderer>().material;
        proximityDetector = GetComponent<ProximityDetector>();
    }

    // Update is called once per frame
    void Update()
    {
        this.proximityDetector.OnDistance = this.transform.localScale.x*0.7f;
        this.proximityDetector.OffDistance = this.transform.localScale.x * 0.8f;
    }
}
