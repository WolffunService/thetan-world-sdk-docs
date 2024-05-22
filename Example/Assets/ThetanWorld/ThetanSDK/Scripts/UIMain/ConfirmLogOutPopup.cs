using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ConfirmLogOutPopup : Popup
{
    [SerializeField] private Button _btnClose;
    [SerializeField] private Button _btnCancel;
    [SerializeField] private Button _btnLogOut;

    private Action _confirmCallback;
    private Action _cancelCallback;

    private bool _isConfirm = false;
    
    private void Awake()
    {
        _btnClose.onClick.AddListener(OnClickCancel);
        _btnCancel.onClick.AddListener(OnClickCancel);
        _btnLogOut.onClick.AddListener(OnClickConfirm);
    }

    public override void OnBeforePushPopup()
    {
        base.OnBeforePushPopup();

        _isConfirm = false;
    }

    public override void OnAfterPopPopup()
    {
        base.OnAfterPopPopup();
        
        if(_isConfirm)
            _confirmCallback?.Invoke();
        else
            _cancelCallback?.Invoke();
    }

    public void SetCallback(Action confirmCallback, Action cancelCallback)
    {
        _confirmCallback = confirmCallback;
        _cancelCallback = cancelCallback;
    }
    
    private void OnClickConfirm()
    {
        _isConfirm = true;
        _popupContainer.Pop();
    }

    private void OnClickCancel()
    {
        _isConfirm = false;
        _popupContainer.Pop();
    }
}
