using System.Collections;
using System.Collections.Generic;
using ThetanSDK.UI;
using UnityEngine;

public class FloatingWindow : MonoBehaviour
{
    protected FloatingWindowContainer _container;

    public void Initialize(FloatingWindowContainer container)
    {
        _container = container;
    }

    public virtual void OnBeforeShow() {}
    
    public virtual void OnAfterShow() {}

    public virtual void OnBeforeClose() {}
    
    public virtual void OnAfterClose() {}
}
