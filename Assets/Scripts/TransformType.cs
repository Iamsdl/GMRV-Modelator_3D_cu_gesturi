using System.Collections.Generic;

public partial class Controller
{
    #region Nested classes - States

    private abstract class TransformType
    {
        protected Controller controller;
        protected Mode mode;

        protected TransformType(Controller controller, Mode mode)
        {
            this.controller = controller;
            this.mode = mode;
        }

        public virtual void Update() { }
        public virtual void GrabLeft_OnActivate() { }
        public virtual void GrabLeft_OnDeactivate() { }
        public virtual void DisableTransform()
        {
            controller.positionConstraint.constraintActive = false;
            controller.rotationConstraint.constraintActive = false;
            controller.scaleConstraint.constraintActive = false;

            controller.transformAuxObject2CT.transform.parent = null;
        }
        public virtual void EnableTransform(List<MyObject> objects) { }
    }
}
