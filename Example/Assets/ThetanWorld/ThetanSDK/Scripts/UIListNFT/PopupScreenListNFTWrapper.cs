using System;
using System.Collections;
using System.Collections.Generic;
using ThetanSDK.UI;
using UnityEngine;
using UnityEngine.UI;

public class PopupScreenListNFTWrapper : Popup
{
    [SerializeField] private Button _btnClose;
    [SerializeField] private ScreenListNFTLandscape _screenListNft;
    public ScreenListNFTLandscape ScreenListNft => _screenListNft;

    private void Awake()
    {
        _btnClose.onClick.AddListener(OnClickClose);
    }

    private void OnClickClose()
    {
        if (_popupContainer != null &&
            _popupContainer.CurrentPopup == this)
        {
            _popupContainer.Pop();
        }
    }

    public void Initialize(UIHelperContainer uiHelperContainer)
    {
        _screenListNft.Initialize(uiHelperContainer);
    }

    public override void OnBeforePushPopup()
    {
        base.OnBeforePushPopup();
        
        _screenListNft.OnBeforePushScreen();
    }

    public override void OnAfterPushPopup()
    {
        base.OnAfterPushPopup();
        
        _screenListNft.OnAfterPushScreen();
    }

    public override void OnBeforePopPopup()
    {
        base.OnBeforePopPopup();
        
        _screenListNft.OnBeforePopScreen();
    }

    public override void OnAfterPopPopup()
    {
        base.OnAfterPopPopup();
        
        _screenListNft.OnAfterPopScreen();
    }
}
