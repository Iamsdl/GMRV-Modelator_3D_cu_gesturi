using Hover.Core.Items.Types;
using Leap.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

public class Controller : MonoBehaviour
{
    private GameObject mainCamera;

    private ExtendedFingerDetector detectorGrabRight;
    private ExtendedFingerDetector detectorGrabLeft;
    private ExtendedFingerDetector detectorPinchLeft;
    private ExtendedFingerDetector detectorPinchRight;

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
            if (ActiveSelectedList.Count != 0)
            {
                return SelectedObjects[SelectedObjects.Count - 1];
            }
            else return null;
        }
    }
    #endregion
    
    #region Select
    [Header("Select")]
    public HoverItemDataCheckbox deselectingCheckbox;
    private SelectingState SelectingState;
    public Dictionary<ObjectStates, Color> ObjectColors;
    #endregion

    #region Transform
    private TransformingState TransformingState;

    [Header("Transform types")]
    public HoverItemDataRadio translatingRadio;
    public HoverItemDataRadio RotatingRadio;
    public HoverItemDataRadio ScalingRadio;

    [Header("Transform axes")]
    public HoverItemDataCheckbox xAxis;
    public HoverItemDataCheckbox yAxis;
    public HoverItemDataCheckbox zAxis;

    [Header("Transform aux objects")]
    public GameObject transformAuxObject1;
    private PositionConstraint positionConstraint;
    private RotationConstraint rotationConstraint;
    private ScaleConstraint scaleConstraint;
    public GameObject transformAuxObject2;
    public GameObject leftPalm;
    public GameObject rightPalm;

    private float initialScale;
    private float initialDistance;
    #endregion

    #region Edit
    [Header("Edit")]
    private EditingState EditingState;
    private Vector3[] vertices;
    #endregion

    // Start is called before the first frame update
    void Start()
    {
        xAxis.OnValueChanged += XAxis_OnValueChanged;
        yAxis.OnValueChanged += YAxis_OnValueChanged;
        zAxis.OnValueChanged += ZAxis_OnValueChanged;

        positionConstraint = transformAuxObject1.GetComponent<PositionConstraint>();
        rotationConstraint = transformAuxObject1.GetComponent<RotationConstraint>();
        scaleConstraint = transformAuxObject1.GetComponent<ScaleConstraint>();

        mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
        InitialiseDetectors();

        AllObjects = new List<MyObject>();
        SelectedObjects = new List<MyObject>();
        AllVertices = new List<MyObject>();
        SelectedVertices = new List<MyObject>();

        ActiveSelectedList = SelectedObjects;

        SelectingState = SelectingState.None;
        TransformingState = TransformingState.Translating;
        EditingState = EditingState.None;

        ObjectColors = new Dictionary<ObjectStates, Color>
        {
            { ObjectStates.Deselected, Color.gray },
            { ObjectStates.Selected, new Color(1, 0.5f, 0) },
            { ObjectStates.Active, Color.yellow },
            { ObjectStates.Inactive, Color.black }
        };
    }

    private void InitialiseDetectors()
    {
        detectorGrabLeft = this.GetComponents<ExtendedFingerDetector>()[0];
        detectorGrabLeft.OnActivate.AddListener(() => { TransformingState = TransformingState.RotoScale; });
        detectorGrabLeft.OnDeactivate.AddListener(() => { TransformingState = TransformingState.Translating; });

        detectorGrabRight = this.GetComponents<ExtendedFingerDetector>()[1];
        detectorGrabRight.OnActivate.AddListener(() => { EnableTransform(AllObjects); });
        detectorGrabRight.OnDeactivate.AddListener(() => { DisableTransform(); });

        detectorPinchLeft = this.GetComponents<ExtendedFingerDetector>()[2];
        detectorPinchLeft.OnActivate.AddListener(() => { TransformingState = TransformingState.RotoScale; });
        detectorPinchLeft.OnDeactivate.AddListener(() => { TransformingState = TransformingState.Translating; });

        detectorPinchRight = this.GetComponents<ExtendedFingerDetector>()[3];
        detectorPinchRight.OnActivate.AddListener(() => { EnableTransform(SelectedObjects); });
        detectorPinchRight.OnDeactivate.AddListener(() => { DisableTransform(); });
    }

    // Update is called once per frame
    void Update()
    {
        if (this.TransformingState == TransformingState.Translating)
        {
            Translate();
        }
        if (this.TransformingState == TransformingState.RotoScale)
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
                    Debug.Log(e.Message);
                    //handles[i] might be deleted midframe, this is expected so continue
                }
            }
        }
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


    public void UpdateConstraints()
    {
        Axis x = xAxis.Value == true ? Axis.X : Axis.None;
        Axis y = yAxis.Value == true ? Axis.Y : Axis.None;
        Axis z = zAxis.Value == true ? Axis.Z : Axis.None;

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

    private void ReleaseObjects()
    {
        transformAuxObject1.transform.DetachChildren();
    }


    //public void DisableTransforming(Hover.InterfaceModules.Cast.HovercastRowSwitchingInfo.RowEntryType rowEntryType)
    //{
    //    if (rowEntryType == Hover.InterfaceModules.Cast.HovercastRowSwitchingInfo.RowEntryType.FromInside)
    //    {
    //        if (this.TransformingState != TransformingState.None)
    //        {
    //            this.TransformingState = TransformingState.None;
    //            transformAuxObject1.transform.DetachChildren();
    //            return;
    //        }
    //        DisableEditing(rowEntryType);
    //    }

    //}

    //public void ToggleSelection()
    //{
    //    if (deselectingCheckbox.Value)
    //    {
    //        this.SelectingState = SelectingState.Deselecting;
    //    }
    //    else
    //    {
    //        this.SelectingState = SelectingState.Selecting;
    //    }
    //}

    //public void DisableSelection(Hover.InterfaceModules.Cast.HovercastRowSwitchingInfo.RowEntryType rowEntryType)
    //{
    //    if (rowEntryType == Hover.InterfaceModules.Cast.HovercastRowSwitchingInfo.RowEntryType.FromInside)
    //    {
    //        this.SelectingState = SelectingState.None;
    //    }
    //}

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
            this.EditingState = EditingState.Vertices;
        }
    }

    public void DisableEditing(Hover.InterfaceModules.Cast.HovercastRowSwitchingInfo.RowEntryType rowEntryType)
    {
        if (rowEntryType == Hover.InterfaceModules.Cast.HovercastRowSwitchingInfo.RowEntryType.FromInside)
        {
            while (AllVertices.Count != 0)
            {
                var temp = AllVertices[0];
                AllVertices.Remove(temp);
                Destroy(temp.gameObject);
            }
            SelectedVertices.Clear();
            ActiveSelectedList = SelectedObjects;
            this.EditingState = EditingState.None;
        }
    }

    public void SelectOrDeselect(MyObject myObject)
    {
        if (EditingState == EditingState.Vertices)
        {
            if (AllObjects.Contains(myObject))
            {
                return;
            }
        }

        if(myObject.IsSelected)
        {
            Deselect(myObject);
        }
        else
        {
            Select(myObject);
        }
    }
    private void Select(MyObject myObject)
    {
        if (!ActiveSelectedList.Contains(myObject))
        {
            if (LastSelectedObject != null)
            {
                LastSelectedObject.Color = ObjectColors[ObjectStates.Selected];
            }
            ActiveSelectedList.Add(myObject);
            myObject.IsSelected = true;
            myObject.Color = ObjectColors[ObjectStates.Active];
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
        }
    }

    public void DeleteSelected()
    {
        foreach (var item in ActiveSelectedList)
        {
            AllObjects.Remove(item);
            Destroy(item.gameObject);
        }
        SelectedObjects.Clear();
    }

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

    public void Quit()
    {
        Application.Quit();
        UnityEditor.EditorApplication.isPlaying = false;
    }
}

public enum ObjectType
{
    Cube = 0,
    Sphere = 1,
    Vertex = 2
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