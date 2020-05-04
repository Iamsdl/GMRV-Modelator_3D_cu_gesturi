public interface IState
{
    void GrabLeft_OnActivate();
    void GrabLeft_OnDeactivate();

    void GrabRight_OnActivate();
    void GrabRight_OnDeactivate();


    void PinchLeft_OnActivate();
    void PinchLeft_OnDeactivate();

    void PinchRight_OnActivate();
    void PinchRight_OnDeactivate();
}
