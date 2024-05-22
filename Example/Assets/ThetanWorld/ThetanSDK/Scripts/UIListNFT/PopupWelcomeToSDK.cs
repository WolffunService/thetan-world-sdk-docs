using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PopupWelcomeToSDK : Popup
{
    [SerializeField] private Button _btnClose;
    [SerializeField] private Button _btnSelectNow;

    private Action _onClosePopupCallback;
    
    private void Awake()
    {
        _btnClose.onClick.AddListener(ClosePopup);
        _btnSelectNow.onClick.AddListener(ClosePopup);
    }

    public void SetCallback(Action onClosePopupCallback)
    {
        _onClosePopupCallback = onClosePopupCallback;
    }
    
    public override void OnBeforePopPopup()
    {
        base.OnBeforePopPopup();
        
        _onClosePopupCallback?.Invoke();
        _onClosePopupCallback = null;
    }

    private void ClosePopup()
    {
        _popupContainer.Pop();
    }
}
