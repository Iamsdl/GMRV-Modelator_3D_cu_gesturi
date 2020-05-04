using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectModeState : State
{
    public ObjectModeState(Controller controller) : base(controller)
    {
    }

    public override void GrabLeft_OnActivate()
    {
        ActivateRotoScale();
    }

    public override void GrabLeft_OnDeactivate()
    {
        DeactivateRotoScale();
    }

    public override void GrabRight_OnActivate()
    {
        throw new System.NotImplementedException();
    }

    public override void GrabRight_OnDeactivate()
    {
        throw new System.NotImplementedException();
    }

    public override void PinchLeft_OnActivate()
    {
        throw new System.NotImplementedException();
    }

    public override void PinchLeft_OnDeactivate()
    {
        throw new System.NotImplementedException();
    }

    public override void PinchRight_OnActivate()
    {
        throw new System.NotImplementedException();
    }

    public override void PinchRight_OnDeactivate()
    {
        throw new System.NotImplementedException();
    }

    private void ActivateRotoScale()
    {
        controller.TransformingState = TransformingState.RotoScale;
    }
    private void DeactivateRotoScale()
    {
        controller.TransformingState = TransformingState.Translating;
    }
}
