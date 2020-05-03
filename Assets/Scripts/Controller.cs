using Hover.Core.Items.Types;
using Leap.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.VFX;

public class Controller : MonoBehaviour
{
    #region Fields
    private GameObject mainCamera;

    #region Particles
    public ParticlesController ParticlesController;

    public LineRenderer LineRenderer;


    #endregion

    #region detectors
    private ExtendedFingerDetector detectorGrabRight;
    private ExtendedFingerDetector detectorGrabLeft;
    private ExtendedFingerDetector detectorPinchLeft;
    private ExtendedFingerDetector detectorPinchRight;
    #endregion

    #region Lists
    [Header("Lists")]
    public List<GameObject> ObjectsToInstantiate;

    private List<MyObject> AllObjects;
    private List<MyObject> SelectedObjects;

    private List<MyObject> AllVertices;
    private List<MyObject> SelectedVertices;

    private List<MyObject> ActiveSelectedList;
    private List<MyObject> ActiveAllList;

    private MyObject LastSelectedObject
    {
        get
        {
            if (SelectedObjects.Count != 0)
            {
                return SelectedObjects[SelectedObjects.Count - 1];
            }
            else return null;
        }
    }

    #endregion

    #region Select
    [Header("Select")]
    public Dictionary<ObjectStates, Color> ObjectColors = new Dictionary<ObjectStates, Color>
        {
            { ObjectStates.Deselected, Color.gray },
            { ObjectStates.Selected, new Color(1, 0.5f, 0) },
            { ObjectStates.Active, Color.yellow },
            { ObjectStates.Inactive, Color.black }
        };
    #endregion

    #region Transform
    private TransformingState TransformingState;

    [Header("Transform axes")]
    public HoverItemDataCheckbox XAxis;
    public HoverItemDataCheckbox YAxis;
    public HoverItemDataCheckbox ZAxis;

    private float initialScale;
    private float initialDistance;
    #endregion

    #region Edit
    [Header("Edit")]
    public HoverItemDataCheckbox EditModeCheckbox;
    public HoverItemDataCheckbox ExtrudeCheckbox;
    public HoverItemDataSelector FillButton;
    public HoverItemDataSelector AddVertexButton;

    private EditingState EditingState;
    private Vector3[] vertices;
    #endregion

    #region Aux objects
    [Header("Transform aux objects")]
    public GameObject transformAuxObject1;
    private PositionConstraint positionConstraint;
    private RotationConstraint rotationConstraint;
    private ScaleConstraint scaleConstraint;
    public GameObject transformAuxObject2;
    public GameObject leftPalm;
    public GameObject rightPalm;
    private bool drawingPath;
    #endregion
    #endregion

    // Start is called before the first frame update
    void Start()
    {

        mainCamera = GameObject.FindGameObjectWithTag("MainCamera");

        XAxis.OnValueChanged += XAxis_OnValueChanged;
        YAxis.OnValueChanged += YAxis_OnValueChanged;
        ZAxis.OnValueChanged += ZAxis_OnValueChanged;

        positionConstraint = transformAuxObject1.GetComponent<PositionConstraint>();
        rotationConstraint = transformAuxObject1.GetComponent<RotationConstraint>();
        scaleConstraint = transformAuxObject1.GetComponent<ScaleConstraint>();

        InitialiseDetectors();

        AllObjects = new List<MyObject>();
        SelectedObjects = new List<MyObject>();
        AllVertices = new List<MyObject>();
        SelectedVertices = new List<MyObject>();

        ActiveSelectedList = SelectedObjects;
        ActiveAllList = AllObjects;

        TransformingState = TransformingState.Translating;
        EditingState = EditingState.None;

        ParticlesController.AddParticlesButton.OnSelected += AddParticlesButton_OnSelected;

        EditModeCheckbox.OnValueChanged += EditModeCheckbox_OnValueChanged;
        ExtrudeCheckbox.OnValueChanged += ExtrudeCheckbox_OnValueChanged;
        AddVertexButton.OnSelected += AddVertexButton_OnSelected;
        FillButton.OnSelected += FillButton_OnSelected;


    }

    

    private void InitialiseDetectors()
    {
        detectorGrabLeft = this.GetComponents<ExtendedFingerDetector>()[0];
        detectorGrabLeft.OnActivate.AddListener(ActivateRotoScale);
        detectorGrabLeft.OnDeactivate.AddListener(DeactivateRotoScale);

        detectorGrabRight = this.GetComponents<ExtendedFingerDetector>()[1];
        detectorGrabRight.OnActivate.AddListener(EnableTransformAll);
        detectorGrabRight.OnDeactivate.AddListener(DisableTransform);

        detectorPinchLeft = this.GetComponents<ExtendedFingerDetector>()[2];
        //detectorPinchLeft.OnActivate.AddListener(() => { TransformingState = TransformingState.RotoScale; });
        //detectorPinchLeft.OnDeactivate.AddListener(() => { TransformingState = TransformingState.Translating; });

        detectorPinchRight = this.GetComponents<ExtendedFingerDetector>()[3];
        detectorPinchRight.OnActivate.AddListener(EnableTransformSelected);
        detectorPinchRight.OnDeactivate.AddListener(DisableTransform);
    }

    // Update is called once per frame
    void Update()
    {
        if (this.TransformingState == TransformingState.Translating)
        {
            Translate();
        }
        else if (this.TransformingState == TransformingState.RotoScale)
        {
            Rotate();
            Scale();
        }
        if (EditingState == EditingState.Vertices)
        {
            if (AllVertices.Count != 0)
            {
                try
                {
                    for (int i = 0; i < AllVertices.Count; i++)
                    {
                        AllVertices[i].transform.parent = null;
                        vertices[i] = LastSelectedObject.transform.InverseTransformPoint(AllVertices[i].transform.position);
                    }
                    foreach (var item in ActiveSelectedList)
                    {
                        item.transform.parent = transformAuxObject1.transform;
                    }
                    LastSelectedObject.Mesh.vertices = vertices;

                }
                catch (Exception e)
                {
                    //handles[i] might be deleted midframe, this is expected so continue
                    Debug.Log(e.Message);
                }
            }
        }
        if (drawingPath)
        {
            if (LineRenderer.positionCount == 0 || Vector3.Distance(rightPalm.transform.position, LineRenderer.GetPosition(LineRenderer.positionCount - 1)) > 0.05)
            {
                LineRenderer.positionCount++;
                LineRenderer.SetPosition(LineRenderer.positionCount - 1, rightPalm.transform.position);
            }

        }
    }

    #region Transforming
    private void ActivateRotoScale()
    {
        TransformingState = TransformingState.RotoScale;
    }
    private void DeactivateRotoScale()
    {
        TransformingState = TransformingState.Translating;
    }
    private void Translate()
    {
        //no need to do anything, unity parenting/constraints take care of things.
    }
    private void Rotate()
    {
        transformAuxObject2.transform.LookAt(rightPalm.transform);
    }
    private void Scale()
    {
        float distance = Vector3.Distance(this.transformAuxObject2.transform.position, leftPalm.transform.position);
        float newScale = initialScale + distance - initialDistance;

        transformAuxObject2.transform.localScale = new Vector3(newScale, newScale, newScale);
    }
    #endregion

    #region Particles
    private void AddParticlesButton_OnSelected(IItemDataSelectable pItem)
    {
        detectorPinchRight.OnActivate.RemoveListener(EnableTransformSelected);
        detectorPinchRight.OnDeactivate.RemoveListener(DisableTransform);

        detectorGrabRight.OnActivate.RemoveListener(EnableTransformAll);
        detectorGrabRight.OnDeactivate.RemoveListener(DisableTransform);

        detectorGrabRight.OnActivate.AddListener(StartDrawingPath);
        detectorGrabRight.OnDeactivate.AddListener(StopDrawingPath);
    }
    private void StartDrawingPath()
    {
        LineRenderer.positionCount = 0;
        drawingPath = true;
    }
    private void StopDrawingPath()
    {
        drawingPath = false;

        ParticlesController.StopDrawingPath(LineRenderer);

        detectorPinchRight.OnActivate.AddListener(EnableTransformSelected);
        detectorPinchRight.OnDeactivate.AddListener(DisableTransform);

        detectorGrabRight.OnActivate.AddListener(EnableTransformAll);
        detectorGrabRight.OnDeactivate.AddListener(DisableTransform);

        detectorGrabRight.OnActivate.RemoveListener(StartDrawingPath);
        detectorGrabRight.OnDeactivate.RemoveListener(StopDrawingPath);
    }

    
    #endregion

    #region Axis constraints
    public void UpdateConstraints()
    {
        Axis x = XAxis.Value == true ? Axis.X : Axis.None;
        Axis y = YAxis.Value == true ? Axis.Y : Axis.None;
        Axis z = ZAxis.Value == true ? Axis.Z : Axis.None;

        positionConstraint.translationAxis = x | y | z;
        rotationConstraint.rotationAxis = x | y | z;
        scaleConstraint.scalingAxis = x | y | z;
    }
    private void ZAxis_OnValueChanged(IItemDataSelectable<bool> pItem)
    {
        UpdateConstraints();
    }
    private void YAxis_OnValueChanged(IItemDataSelectable<bool> pItem)
    {
        UpdateConstraints();
    }
    private void XAxis_OnValueChanged(IItemDataSelectable<bool> pItem)
    {
        UpdateConstraints();
    }
    #endregion

    #region Grabbing
    private void EnableTransformAll()
    {
        EnableTransform(ActiveAllList);
    }
    private void EnableTransformSelected()
    {
        EnableTransform(ActiveSelectedList);
    }
    public void EnableTransform(List<MyObject> objects)
    {
        ReleaseObjects();

        switch (TransformingState)
        {
            case TransformingState.Translating:
                PrepareTranslate();
                break;
            case TransformingState.RotoScale:
                PrepareRotate();
                PrepareScale();
                break;
            default:
                break;
        }

        GrabObjects(objects);
    }
    #region Used in EnableTransform
    private void ReleaseObjects()
    {
        transformAuxObject1.transform.DetachChildren();
    }
    private void PrepareTranslate()
    {
        positionConstraint.constraintActive = true;
        transformAuxObject2.transform.parent = rightPalm.transform;
    }
    private void PrepareRotate()
    {
        transformAuxObject2.transform.LookAt(rightPalm.transform);
        transformAuxObject1.transform.LookAt(rightPalm.transform);
        rotationConstraint.constraintActive = true;
    }
    private void PrepareScale()
    {
        transformAuxObject2.transform.localScale = transformAuxObject1.transform.localScale;
        scaleConstraint.constraintActive = true;

        initialScale = transformAuxObject2.transform.localScale.x;
        initialDistance = Vector3.Distance(transformAuxObject2.transform.position, leftPalm.transform.position);
    }
    private void GrabObjects(List<MyObject> objects)
    {
        if (objects.Count != 0)
        {
            transformAuxObject1.transform.position = GetMedianPoint(objects);
            transformAuxObject2.transform.position = transformAuxObject1.transform.position;

            foreach (var item in objects)
            {
                item.transform.parent = transformAuxObject1.transform;
            }
        }
    }
    private static Vector3 GetMedianPoint(List<MyObject> objectList)
    {
        Vector3 sum = Vector3.zero;
        foreach (MyObject item in objectList)
        {
            sum += item.transform.position;
        }
        sum /= objectList.Count;
        return sum;
    }
    private static Vector3 GetMedianPoint(Vector3[] points)
    {
        Vector3 sum = Vector3.zero;
        foreach (Vector3 item in points)
        {
            sum += item;
        }
        sum /= points.Length;
        return sum;
    }
    #endregion
    public void DisableTransform()
    {
        switch (TransformingState)
        {
            case TransformingState.Translating:
                positionConstraint.constraintActive = false;
                break;
            case TransformingState.RotoScale:
                rotationConstraint.constraintActive = false;
                scaleConstraint.constraintActive = false;
                break;
            default:
                break;
        }
        transformAuxObject2.transform.parent = null;
    }
    #endregion

    #region Edit mode
    private void EditModeCheckbox_OnValueChanged(IItemDataSelectable<bool> pItem)
    {
        if (EditModeCheckbox.Value)
        {
            EnableEditing();
            ExtrudeCheckbox.IsEnabled = true;
            FillButton.IsEnabled = true;
            AddVertexButton.IsEnabled = true;
        }
        else
        {
            DisableEditing();
            ExtrudeCheckbox.IsEnabled = false;
            FillButton.IsEnabled = false;
            AddVertexButton.IsEnabled = false;
        }
    }
    public void EnableEditing()
    {
        if (LastSelectedObject != null)
        {
            vertices = LastSelectedObject.Mesh.vertices;
            foreach (var vertex in vertices)
            {
                AllVertices.Add(CreateVertex(LastSelectedObject.transform.TransformPoint(vertex)));
            }
            foreach (var item in AllObjects)
            {
                item.Color = ObjectColors[ObjectStates.Inactive];
            }
            ActiveSelectedList = SelectedVertices;
            ActiveAllList = AllVertices;
            this.EditingState = EditingState.Vertices;
        }
    }
    public void DisableEditing()
    {
        while (AllVertices.Count != 0)
        {
            var temp = AllVertices[0];
            AllVertices.Remove(temp);
            Destroy(temp.gameObject);
        }
        SelectedVertices.Clear();
        ActiveSelectedList = SelectedObjects;
        ActiveAllList = AllObjects;
        this.EditingState = EditingState.None;
    }

    private void ExtrudeCheckbox_OnValueChanged(IItemDataSelectable<bool> pItem)
    {
        if (ExtrudeCheckbox.Value)
        {
            detectorPinchRight.OnActivate.RemoveListener(EnableTransformSelected);

            detectorPinchRight.OnActivate.AddListener(Extrude);
        }
        else
        {
            detectorPinchRight.OnActivate.AddListener(EnableTransformSelected);

            detectorPinchRight.OnActivate.RemoveListener(Extrude);
        }
    }

    private void Extrude()
    {
        if (SelectedVertices.Count > 1 && SelectedVertices.Count < 5)
        {
            MyObject[] newVertices = DuplicateSelectedVertices();

            int badPractice = SelectedVertices.Count == 2 ? 1 : 0;
            for (int i = 0; i < SelectedVertices.Count - badPractice; i++)
            {
                MakeFace(SelectedVertices[i],
                         SelectedVertices[(i + 1) % SelectedVertices.Count],
                         newVertices[(i + 1) % SelectedVertices.Count],
                         newVertices[i]);
            }

            switch (SelectedVertices.Count)
            {
                case 3:
                    MakeFace(newVertices[0], newVertices[1], newVertices[2]);
                    break;
                case 4:
                    MakeFace(newVertices[0], newVertices[1], newVertices[2], newVertices[3]);
                    break;
                default:
                    break;
            }

            DeselectAllVertices();

            Select(newVertices);

            EnableTransformSelected();
        }
    }

    private void AddVertexButton_OnSelected(IItemDataSelectable pItem)
    {
        MyObject[] newVertices = DuplicateSelectedVertices();
        DeselectAllVertices();
        Select(newVertices);
    }

    private void DeselectAllVertices()
    {
        foreach (var item in AllVertices)
        {
            if (item.IsSelected)
            {
                Deselect(item);
            }
        }
    }

    private MyObject[] DuplicateSelectedVertices()
    {
        int k = SelectedVertices.Count;
        Vector3[] newVerticesArray = new Vector3[vertices.Length + k];

        vertices.CopyTo(newVerticesArray, 0);
        for (int i = 0; i < k; i++)
        {
            int j = AllVertices.IndexOf(SelectedVertices[i]);
            Vector3 vertex = newVerticesArray[j];
            newVerticesArray[vertices.Length + i] = vertex;
        }

        //SelectedVertices.Select(x => x.transform.position).ToArray().CopyTo(newVerticesArray, vertices.Length);

        vertices = newVerticesArray;
        LastSelectedObject.Mesh.vertices = vertices;


        MyObject[] createdVertices = new MyObject[k];
        int n = newVerticesArray.Length;
        for (int i = n - k; i < n; i++)
        {
            MyObject newVertexObject = CreateVertex(LastSelectedObject.transform.TransformPoint(newVerticesArray[i]));
            AllVertices.Add(newVertexObject);
            createdVertices[i - (n - k)] = newVertexObject;
        }

        return createdVertices;
    }

    private void FillButton_OnSelected(IItemDataSelectable pItem)
    {
        if (SelectedVertices.Count == 3)
        {
            MakeFace(SelectedVertices[0],
                         SelectedVertices[1],
                         SelectedVertices[2]);
        }
        else if (SelectedVertices.Count == 4)
        {
            MakeFace(SelectedVertices[0],
                     SelectedVertices[1],
                     SelectedVertices[2],
                     SelectedVertices[3]);
        }
    }

    private void MakeFace(MyObject myObject1, MyObject myObject2, MyObject myObject3, MyObject myObject4)
    {
        MakeFace(myObject1, myObject2, myObject3);
        MakeFace(myObject3, myObject4, myObject1);
    }

    private void MakeFace(MyObject obj1, MyObject obj2, MyObject obj3)
    {
        int v1 = AllVertices.IndexOf(obj1);
        int v2 = AllVertices.IndexOf(obj2);
        int v3 = AllVertices.IndexOf(obj3);
        MakeFace(v1, v2, v3);
        MakeFace(v1, v3, v2);
    }

    private void MakeFace(int v1, int v2, int v3)
    {
        int[] oldIndices = LastSelectedObject.Mesh.GetIndices(0);
        int[] newIndices = new int[oldIndices.Length + 3];
        oldIndices.CopyTo(newIndices, 0);
        newIndices[oldIndices.Length] = v1;
        newIndices[oldIndices.Length + 1] = v2;
        newIndices[oldIndices.Length + 2] = v3;
        LastSelectedObject.Mesh.SetIndices(newIndices, MeshTopology.Triangles, 0);
    }
    #endregion

    #region Select objects
    public void SelectOrDeselect(MyObject myObject)
    {
        if (EditingState == EditingState.Vertices)
        {
            if (AllObjects.Contains(myObject))
            {
                return;
            }
        }

        if (myObject.IsSelected)
        {
            Deselect(myObject);
        }
        else
        {
            Select(myObject);
        }
    }
    #region used in SelectOrDeselect
    private void Select(params MyObject[] myObjects)
    {
        foreach (var myObject in myObjects)
        {
            if (!ActiveSelectedList.Contains(myObject))
            {
                if (LastSelectedObject != null)
                {
                    LastSelectedObject.Color = ObjectColors[ObjectStates.Selected];
                }
                ActiveSelectedList.Add(myObject);
                myObject.IsSelected = true;
                EditModeCheckbox.IsEnabled = true;
                myObject.Color = ObjectColors[ObjectStates.Active];
            }
        }
    }
    private void Deselect(MyObject myObject)
    {
        if (ActiveSelectedList.Contains(myObject))
        {
            ActiveSelectedList.Remove(myObject);
            myObject.IsSelected = false;
            myObject.Color = ObjectColors[ObjectStates.Deselected];
            if (LastSelectedObject != null)
            {
                LastSelectedObject.Color = ObjectColors[ObjectStates.Active];
            }
            else
            {
                EditModeCheckbox.IsEnabled = false;
            }
        }
    }
    #endregion
    #endregion
    #region Create objects
    private void CreateObject(ObjectType objectType)
    {
        Vector3 position = mainCamera.transform.position;
        Quaternion rotation = mainCamera.transform.rotation;
        MyObject obj = Instantiate(ObjectsToInstantiate[Convert.ToInt32(objectType)], position, rotation).GetComponent<MyObject>();
        obj.transform.Translate(new Vector3(0, 0, 1), mainCamera.transform);

        obj.gameObject.SetActive(true);

        obj.Initialise();
        obj.proximityDetector.OnActivate.AddListener(delegate { SelectOrDeselect(obj); });
        obj.Color = ObjectColors[ObjectStates.Deselected];
        AllObjects.Add(obj);
    }
    public void CreateQuad()
    {
        CreateObject(ObjectType.Quad);
    }
    public void CreateCube()
    {
        CreateObject(ObjectType.Cube);
    }
    public void CreateSphere()
    {
        CreateObject(ObjectType.Sphere);
    }
    public MyObject CreateVertex(Vector3 position)
    {
        MyObject obj = Instantiate(ObjectsToInstantiate[Convert.ToInt32(ObjectType.Vertex)], position, Quaternion.identity).GetComponent<MyObject>();
        obj.gameObject.SetActive(true);
        obj.Initialise();
        obj.proximityDetector.OnActivate.AddListener(delegate { SelectOrDeselect(obj); });
        obj.Color = ObjectColors[ObjectStates.Deselected];
        return obj;
    }
    #endregion

    #region Delete objects
    public void DeleteSelected()
    {
        foreach (var item in ActiveSelectedList)
        {
            AllObjects.Remove(item);
            Destroy(item.gameObject);
        }
        SelectedObjects.Clear();
    }
    #endregion

    public void Quit()
    {
        Application.Quit();
        UnityEditor.EditorApplication.isPlaying = false;
    }
}

public enum ObjectType
{
    Quad = 0,
    Cube,
    Sphere,
    Vertex
}

public enum SelectingState
{
    None,
    Selecting,
    Deselecting
}

public enum TransformingState
{
    None,
    Translating,
    RotoScale
}

public enum EditingState
{
    None,
    Vertices
}

public enum ObjectStates
{
    Active,
    Selected,
    Deselected,
    Inactive
}