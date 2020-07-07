using System.Collections.Generic;

public partial class Controller
{
    #region Nested classes - States
    private abstract class Mode
    {
        protected Controller controller;

        protected Dictionary<TransformTypeEnum, TransformType> transformTypes;
        protected TransformType transformType;

        public void SetTransformType(TransformTypeEnum transformType)
        {
            this.transformType = transformTypes[transformType];
        }

        protected Mode(Controller controller)
        {
            this.controller = controller;
            transformTypes = new Dictionary<TransformTypeEnum, TransformType>()
            {
                { TransformTypeEnum.Translating, new Translating(this.controller,this)},
                { TransformTypeEnum.RotoScaling, new RotoScaling(this.controller,this)}
            };
            transformType = transformTypes[TransformTypeEnum.Translating];


        }

        #region Grab and Pinch events
        public virtual void GrabLeft_OnActivate() => transformType.GrabLeft_OnActivate();
        public virtual void GrabLeft_OnDeactivate() => transformType.GrabLeft_OnDeactivate();
        public virtual void GrabRight_OnActivate() { }
        public virtual void GrabRight_OnDeactivate() => transformType.DisableTransform();
        public virtual void PinchLeft_OnActivate() { }
        public virtual void PinchLeft_OnDeactivate() { }
        public virtual void PinchRight_OnActivate() { }
        public virtual void PinchRight_OnDeactivate() => transformType.DisableTransform();
        #endregion

        public virtual void Update() => transformType.Update();

        public virtual void EnableTransform(List<MyObject> objects) => transformType.EnableTransform(objects);

        public virtual void SelectOrDeselect(MyObject obj)
        {
            if (obj.IsSelected)
            {
                controller.Deselect(obj);
            }
            else
            {
                controller.Select(obj);
            }
        }
    }
}
