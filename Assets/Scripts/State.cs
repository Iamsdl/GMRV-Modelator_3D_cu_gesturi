using Hover.InterfaceModules.Cast;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class State : MonoBehaviour
{
    public delegate void StateChanged();
    public event StateChanged OnStateChanged;

    public bool transforming;
    public bool editing;
    public bool selecting;

    // Start is called before the first frame update
    void Start()
    {
        transforming = false;
        editing = false;
        selecting = false;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void EnableSelecting()
    {
        selecting = true;
        OnStateChanged?.Invoke();
    }
    public void EnableTransforming()
    {
        transforming = true;
        OnStateChanged?.Invoke();
    }
    public void EnableEditing()
    {
        editing = true;
        OnStateChanged?.Invoke();
    }
    public void DisableStates(HovercastRowSwitchingInfo.RowEntryType rowEntryType)
    {
        if (rowEntryType == HovercastRowSwitchingInfo.RowEntryType.FromInside)
        {
            if (transforming)
            {
                transforming = false;
                OnStateChanged?.Invoke();
            }
            else if (editing)
            {
                editing = false;
                OnStateChanged?.Invoke();
            }
        }
    }
    public void DisableSelecting(HovercastRowSwitchingInfo.RowEntryType rowEntryType)
    {
        if (rowEntryType == HovercastRowSwitchingInfo.RowEntryType.FromInside)
        {
            selecting = false;
            OnStateChanged?.Invoke();
        }
    }
}
