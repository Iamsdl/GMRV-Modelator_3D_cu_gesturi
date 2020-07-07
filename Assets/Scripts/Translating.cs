using System.Collections.Generic;

public partial class Controller
{
    #region Nested classes - States
    private class Translating : TransformType
    {
        public Translating(Controller controller, Mode mode) : base(controller, mode)
        {
        }

        #region Grab and Pinch events
        public override void GrabLeft_OnActivate() => mode.SetTransformType(TransformTypeEnum.RotoScaling);
        #endregion

        public override void Update()
        {
            //no need to do anything, unity parenting/constraints take care of things.
            //Translate()
        }

        public override void EnableTransform(List<MyObject> objects)
        {
            controller.ReleaseObjects();
            controller.PrepareTranslate();
            controller.GrabObjects(objects);
            controller.positionConstraint.constraintActive = true;
        }
    }
}
