using System.Collections.Generic;
using UnityEngine;

public partial class Controller
{
    #region Nested classes - States
    private class RotoScaling : TransformType
    {
        public RotoScaling(Controller controller, Mode mode) : base(controller, mode)
        {
        }

        #region Grab and Pinch events
        public override void GrabLeft_OnDeactivate() => mode.SetTransformType(TransformTypeEnum.Translating);
        #endregion
        public override void Update()
        {
            Rotate();
            Scale();
        }
        private void Scale()
        {
            float distance = Vector3.Distance(controller.transformAuxObject2CT.transform.position, controller.rightPalm.transform.position);
            float newScale = controller.initialScale + distance - controller.initialDistance;

            controller.transformAuxObject2CT.transform.localScale = new Vector3(newScale, newScale, newScale);
        }
        private void Rotate()
        {
            controller.transformAuxObject2CT.transform.LookAt(controller.rightPalm.transform);
        }

        public override void EnableTransform(List<MyObject> objects)
        {
            controller.ReleaseObjects();
            controller.PrepareRotate(objects);
            controller.PrepareScale();
            controller.GrabObjects(objects);
        }
    }
}
