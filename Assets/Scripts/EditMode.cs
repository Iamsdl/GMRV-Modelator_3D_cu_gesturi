using System;
using UnityEngine;

public partial class Controller
{
    #region Nested classes - States
    private class EditMode : Mode
    {
        public EditMode(Controller controller) : base(controller)
        {
        }
        #region Grab and Pinch events
        public override void GrabRight_OnActivate() => EnableTransform(controller.AllVertices);
        public override void PinchRight_OnActivate() => EnableTransform(controller.SelectedVertices);
        #endregion
        public override void Update()
        {
            base.Update();
            if (controller.AllVertices.Count != 0)
            {
                try
                {
                    for (int i = 0; i < controller.AllVertices.Count; i++)
                    {
                        controller.vertices[i] = controller.LastSelectedObject.transform.InverseTransformPoint(controller.AllVertices[i].transform.position);
                    }
                    controller.LastSelectedObject.Mesh.vertices = controller.vertices;

                }
                catch (Exception e)
                {
                    //handles[i] might be deleted midframe, this is expected so continue
                    Debug.Log(e.Message);
                }
            }

        }

        public override void SelectOrDeselect(MyObject obj)
        {
            if (controller.AllObjects.Contains(obj))
            {
                return;
            }

            base.SelectOrDeselect(obj);
        }
    }
}
