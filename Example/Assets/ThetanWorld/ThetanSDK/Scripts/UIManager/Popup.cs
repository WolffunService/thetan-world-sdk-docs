using System.Collections;
using System.Collections.Generic;
using Wolffun.Tweening;
using UnityEngine;

public class Popup : MonoBehaviour
{
    protected PopupContainer _popupContainer;

    public void SetPopupContainer(PopupContainer popupContainer)
    {
        _popupContainer = popupContainer;
    }
    
    public virtual void OnBeforePushPopup()
    {}
    
    public virtual void OnAfterPushPopup()
    {}
    
    public virtual void OnBeforePopPopup()
    {}

    public virtual void OnAfterPopPopup()
    {
        _popupContainer = null;
    }
}
