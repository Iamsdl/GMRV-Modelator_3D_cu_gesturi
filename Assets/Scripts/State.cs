using Hover.InterfaceModules.Cast;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class State : IState
{
    protected Controller controller;

    protected State(Controller controller)
    {
        this.controller = controller;
    }

    public abstract void GrabLeft_OnActivate();
    public abstract void GrabLeft_OnDeactivate();
    public abstract void GrabRight_OnActivate();
    public abstract void GrabRight_OnDeactivate();
    public abstract void PinchLeft_OnActivate();
    public abstract void PinchLeft_OnDeactivate();
    public abstract void PinchRight_OnActivate();
    public abstract void PinchRight_OnDeactivate();
}