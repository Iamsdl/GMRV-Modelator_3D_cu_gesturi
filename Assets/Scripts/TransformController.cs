//using Hover.Core.Items.Types;
//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.Animations;

//public class TransformController : MonoBehaviour
//{
//    [Header("Transform axes")]
//    public HoverItemDataCheckbox XAxis;
//    public HoverItemDataCheckbox YAxis;
//    public HoverItemDataCheckbox ZAxis;


//    public GameObject transformAuxObject1;
//    public GameObject transformAuxObject2;

//    private float initialScale;
//    private float initialDistance;

//    private PositionConstraint positionConstraint;
//    private RotationConstraint rotationConstraint;
//    private ScaleConstraint scaleConstraint;

//    // Start is called before the first frame update
//    void Start()
//    {
//        XAxis.OnValueChanged += XAxis_OnValueChanged;
//        YAxis.OnValueChanged += YAxis_OnValueChanged;
//        ZAxis.OnValueChanged += ZAxis_OnValueChanged;

//        positionConstraint = transformAuxObject1.GetComponent<PositionConstraint>();
//        rotationConstraint = transformAuxObject1.GetComponent<RotationConstraint>();
//        scaleConstraint = transformAuxObject1.GetComponent<ScaleConstraint>();
//    }

//    private void XAxis_OnValueChanged(IItemDataSelectable<bool> pItem)
//    {
//        UpdateConstraints();
//    }
//    private void YAxis_OnValueChanged(IItemDataSelectable<bool> pItem)
//    {
//        UpdateConstraints();
//    }
//    private void ZAxis_OnValueChanged(IItemDataSelectable<bool> pItem)
//    {
//        UpdateConstraints();
//    }
//    public void UpdateConstraints()
//    {
//        Axis x = XAxis.Value == true ? Axis.X : Axis.None;
//        Axis y = YAxis.Value == true ? Axis.Y : Axis.None;
//        Axis z = ZAxis.Value == true ? Axis.Z : Axis.None;

//        positionConstraint.translationAxis = x | y | z;
//        rotationConstraint.rotationAxis = x | y | z;
//        scaleConstraint.scalingAxis = x | y | z;
//    }

//    public void EnableTransform(List<MyObject> objects)
//    {
//        ReleaseObjects();

//        switch (TransformingState)
//        {
//            case TransformingState.Translating:
//                PrepareTranslate();
//                break;
//            case TransformingState.RotoScale:
//                PrepareRotate();
//                PrepareScale();
//                break;
//            default:
//                break;
//        }

//        GrabObjects(objects);
//    }
//    private void ReleaseObjects()
//    {
//        transformAuxObject1.transform.DetachChildren();
//    }

//    private void GrabObjects(List<MyObject> objects)
//    {
//        if (objects.Count != 0)
//        {
//            transformAuxObject1.transform.position = GetMedianPoint(objects);
//            transformAuxObject2.transform.position = transformAuxObject1.transform.position;

//            foreach (var item in objects)
//            {
//                item.transform.parent = transformAuxObject1.transform;
//            }
//        }
//    }


//    private void PrepareTranslate()
//    {
//        positionConstraint.constraintActive = true;
//        transformAuxObject2.transform.parent = rightPalm.transform;
//    }
//    private void PrepareRotate()
//    {
//        transformAuxObject2.transform.LookAt(rightPalm.transform);
//        transformAuxObject1.transform.LookAt(rightPalm.transform);
//        rotationConstraint.constraintActive = true;
//    }
//    private void PrepareScale()
//    {
//        transformAuxObject2.transform.localScale = transformAuxObject1.transform.localScale;
//        scaleConstraint.constraintActive = true;

//        initialScale = transformAuxObject2.transform.localScale.x;
//        initialDistance = Vector3.Distance(transformAuxObject2.transform.position, leftPalm.transform.position);
//    }

//    private void Translate()
//    {
//        //no need to do anything, unity parenting/constraints take care of things.
//    }
//    private void Rotate()
//    {
//        transformAuxObject2.transform.LookAt(rightPalm.transform);
//    }
//    private void Scale()
//    {
//        float distance = Vector3.Distance(this.transformAuxObject2.transform.position, leftPalm.transform.position);
//        float newScale = initialScale + distance - initialDistance;

//        transformAuxObject2.transform.localScale = new Vector3(newScale, newScale, newScale);
//    }

//    // Update is called once per frame
//    void Update()
//    {
        
//    }
//}
