using Hover.Core.Items.Types;
using Leap.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.VFX;

public partial class Controller : MonoBehaviour
{

#region Nested classes - States

    private void SetMode(ModeEnum state)
    {
        this.mode = modes[state];
    }
    #endregion

    Dictionary<ModeEnum, Mode> modes;
    private Mode mode;

    private enum ModeEnum
    {
        ObjectMode,
        EditMode
    }
    private enum TransformTypeEnum
    {
        Translating,
        RotoScaling
    }



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

    private Modes EditingState;
    private Vector3[] vertices;
    #endregion

    #region Aux objects
    [Header("Transform aux objects")]
    public GameObject transformAuxObject1OP;
    private PositionConstraint positionConstraint;
    private RotationConstraint rotationConstraint;
    private ScaleConstraint scaleConstraint;
    public GameObject transformAuxObject2CT;
    public GameObject leftPalm;
    public GameObject rightPalm;
    private bool drawingPath;
    #endregion
    #endregion

    // Start is called before the first frame update
    void Start()
    {
        this.modes = new Dictionary<ModeEnum, Mode>()
        {
            { ModeEnum.ObjectMode, new ObjectMode(this) },
            { ModeEnum.EditMode, new EditMode(this) }
        };
        this.mode = modes[ModeEnum.ObjectMode];

        mainCamera = GameObject.FindGameObjectWithTag("MainCamera");

        XAxis.OnValueChanged += XAxis_OnValueChanged;
        YAxis.OnValueChanged += YAxis_OnValueChanged;
        ZAxis.OnValueChanged += ZAxis_OnValueChanged;

        positionConstraint = transformAuxObject1OP.GetComponent<PositionConstraint>();
        rotationConstraint = transformAuxObject1OP.GetComponent<RotationConstraint>();
        scaleConstraint = transformAuxObject1OP.GetComponent<ScaleConstraint>();

        InitialiseDetectors();

        AllObjects = new List<MyObject>();
        SelectedObjects = new List<MyObject>();
        AllVertices = new List<MyObject>();
        SelectedVertices = new List<MyObject>();

        ActiveSelectedList = SelectedObjects;

        ParticlesController.AddParticlesButton.OnSelected += AddParticlesButton_OnSelected;

        EditModeCheckbox.OnValueChanged += EditModeCheckbox_OnValueChanged;
        ExtrudeCheckbox.OnValueChanged += ExtrudeCheckbox_OnValueChanged;
        AddVertexButton.OnSelected += AddVertexButton_OnSelected;
        FillButton.OnSelected += FillButton_OnSelected;
    }

    #region Detectors initialisation
    private void InitialiseDetectors()
    {
        detectorGrabLeft = this.GetComponents<ExtendedFingerDetector>()[0];
        detectorGrabLeft.OnActivate.AddListener(DetectorGrabLeft_OnActivate);
        detectorGrabLeft.OnDeactivate.AddListener(DetectorGrabLeft_OnDeactivate);

        detectorGrabRight = this.GetComponents<ExtendedFingerDetector>()[1];
        detectorGrabRight.OnActivate.AddListener(DetectorGrabRight_OnActivate);
        detectorGrabRight.OnDeactivate.AddListener(DetectorGrabRight_OnDeactivate);

        detectorPinchLeft = this.GetComponents<ExtendedFingerDetector>()[2];
        detectorPinchLeft.OnActivate.AddListener(DetectorPinchLeft_OnActivate);
        detectorPinchLeft.OnDeactivate.AddListener(DetectorPinchLeft_OnDeactivate);

        detectorPinchRight = this.GetComponents<ExtendedFingerDetector>()[3];
        detectorPinchRight.OnActivate.AddListener(DetectorPinchRight_OnActivate);
        detectorPinchRight.OnDeactivate.AddListener(DetectorPinchRight_OnDeactivate);
    }

    private void DetectorGrabLeft_OnActivate()
    {
        mode.GrabLeft_OnActivate();
    }
    private void DetectorGrabLeft_OnDeactivate()
    {
        mode.GrabLeft_OnDeactivate();
    }
    private void DetectorGrabRight_OnActivate()
    {
        mode.GrabRight_OnActivate();
    }
    private void DetectorGrabRight_OnDeactivate()
    {
        mode.GrabRight_OnDeactivate();
    }
    private void DetectorPinchLeft_OnActivate()
    {
        mode.PinchLeft_OnActivate();
    }
    private void DetectorPinchLeft_OnDeactivate()
    {
        mode.PinchLeft_OnDeactivate();
    }
    private void DetectorPinchRight_OnActivate()
    {
        mode.PinchRight_OnActivate();
    }
    private void DetectorPinchRight_OnDeactivate()
    {
        mode.PinchRight_OnDeactivate();
    }
    #endregion

    // Update is called once per frame
    void Update()
    {
        mode.Update();


        if (drawingPath)
        {
            if (LineRenderer.positionCount == 0 || Vector3.Distance(rightPalm.transform.position, LineRenderer.GetPosition(LineRenderer.positionCount - 1)) > 0.05)
            {
                LineRenderer.positionCount++;
                LineRenderer.SetPosition(LineRenderer.positionCount - 1, rightPalm.transform.position);
            }

        }
    }

    #region Particles
    private void AddParticlesButton_OnSelected(IItemDataSelectable pItem)
    {
        detectorPinchRight.OnActivate.RemoveListener(DetectorPinchRight_OnActivate);
        detectorPinchRight.OnDeactivate.RemoveListener(DetectorPinchRight_OnDeactivate);

        detectorGrabRight.OnActivate.RemoveListener(DetectorGrabRight_OnActivate);
        detectorGrabRight.OnDeactivate.RemoveListener(DetectorGrabRight_OnDeactivate);

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

        detectorPinchRight.OnActivate.AddListener(DetectorPinchRight_OnActivate);
        detectorPinchRight.OnDeactivate.AddListener(DetectorPinchRight_OnDeactivate);

        detectorGrabRight.OnActivate.AddListener(DetectorGrabRight_OnActivate);
        detectorGrabRight.OnDeactivate.AddListener(DetectorGrabRight_OnDeactivate);

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


    private void EnableTransformSelected()
    {
        mode.EnableTransform(ActiveSelectedList);
    }
    #region Used in EnableTransform
    private void ReleaseObjects()
    {
        transformAuxObject1OP.transform.DetachChildren();
    }
    private void PrepareTranslate()
    {
        transformAuxObject1OP.transform.position = rightPalm.transform.position;
        transformAuxObject2CT.transform.position = rightPalm.transform.position;

        transformAuxObject2CT.transform.parent = rightPalm.transform;
        positionConstraint.constraintActive = true;
    }
    private void PrepareRotate(List<MyObject> objects)
    {
        transformAuxObject1OP.transform.position = GetMedianPoint(objects);
        transformAuxObject2CT.transform.position = transformAuxObject1OP.transform.position;

        transformAuxObject2CT.transform.LookAt(rightPalm.transform);
        transformAuxObject1OP.transform.LookAt(rightPalm.transform);

        rotationConstraint.constraintActive = true;
    }
    private void PrepareScale()
    {
        transformAuxObject2CT.transform.localScale = transformAuxObject1OP.transform.localScale;

        initialScale = transformAuxObject2CT.transform.localScale.x;
        initialDistance = Vector3.Distance(transformAuxObject2CT.transform.position, rightPalm.transform.position);

        scaleConstraint.constraintActive = true;
    }
    private void GrabObjects(List<MyObject> objects)
    {
        if (objects.Count != 0)
        {
            foreach (var item in objects)
            {
                item.transform.parent = transformAuxObject1OP.transform;
            }
        }
    }
    private static Vector3 GetMedianPoint(List<MyObject> objects)
    {
        Vector3 sum = Vector3.zero;
        if (objects.Any())
        {
            foreach (MyObject item in objects)
            {
                sum += item.transform.position;
            }
            sum /= objects.Count;
        }
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
            this.SetMode(ModeEnum.EditMode);
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
        this.SetMode(ModeEnum.ObjectMode);
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
    public void SelectOrDeselect(MyObject obj)
    {
        mode.SelectOrDeselect(obj);
    }
    #region used in SelectOrDeselect
    private void Select(params MyObject[] objects)
    {
        foreach (var myObject in objects)
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
    private void Deselect(MyObject objects)
    {
        if (ActiveSelectedList.Contains(objects))
        {
            ActiveSelectedList.Remove(objects);
            objects.IsSelected = false;
            objects.Color = ObjectColors[ObjectStates.Deselected];
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



public enum Modes
{
    ObjectMode,
    EditMode
}

public enum ObjectStates
{
    Active,
    Selected,
    Deselected,
    Inactive
}