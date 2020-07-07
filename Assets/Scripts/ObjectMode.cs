public partial class Controller
{
    #region Nested classes - States
    private class ObjectMode : Mode
    {
        public ObjectMode(Controller controller) : base(controller)
        {
        }
        #region Grab and Pinch events
        public override void GrabRight_OnActivate() => EnableTransform(controller.AllObjects);
        public override void PinchRight_OnActivate() => EnableTransform(controller.SelectedObjects);
        #endregion
    }
}
