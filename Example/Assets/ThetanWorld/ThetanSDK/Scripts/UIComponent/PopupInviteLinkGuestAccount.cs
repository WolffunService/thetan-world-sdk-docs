using System;
using System.Collections;
using System.Collections.Generic;
using ThetanSDK;
using UnityEngine;
using UnityEngine.UI;

public class PopupInviteLinkGuestAccount : Popup
{
    [SerializeField] private Button _btnClose;
    [SerializeField] private Button _btnConnectThetanID;
    [SerializeField] private Button _btnLoginWithExistingAccount;

    private Action _onLinkAccountSuccessCallback;
    private Action _onLoginAnotherAccountSuccessCallback;

    private void Awake()
    {
        _btnClose.onClick.AddListener(ClosePopup);
        _btnConnectThetanID.onClick.AddListener(OnClickConnectThetanID);
        _btnLoginWithExistingAccount.onClick.AddListener(OnClickLoginWithExistingAccount);
    }

    private void OnClickLoginWithExistingAccount()
    {
        ClosePopup();
        ThetanSDKManager.Instance.ShowLogin(loginData =>
        {
            _onLoginAnotherAccountSuccessCallback?.Invoke();
        }, null);
    }

    public override void OnAfterPopPopup()
    {
        base.OnAfterPopPopup();

        _onLinkAccountSuccessCallback = null;
        _onLoginAnotherAccountSuccessCallback = null;
    }

    public void Initialize(Action onLinkAccountSuccessCallback, Action onLoginAnotherAccountSuccessCallback)
    {
        _onLinkAccountSuccessCallback = onLinkAccountSuccessCallback;
        _onLoginAnotherAccountSuccessCallback = onLoginAnotherAccountSuccessCallback;
    }

    private void OnClickConnectThetanID()
    {
        ClosePopup();
        ThetanSDKManager.Instance.ShowLinkAccount(authenData =>
        {
            _onLinkAccountSuccessCallback?.Invoke();
        }, null);
    }

    private void ClosePopup()
    {
        _popupContainer.Pop();
    }
    
    
}
