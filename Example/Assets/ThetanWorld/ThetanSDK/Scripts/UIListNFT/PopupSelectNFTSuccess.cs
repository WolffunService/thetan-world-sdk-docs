using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PopupSelectNFTSuccess : Popup
{
    [SerializeField] private Button _btnClose;
    [SerializeField] private Button _btnBackToSDK;
    [SerializeField] private Button _btnContinuePlaying;

    private Action _onBackToSDKCallback;
    private Action _onBackToGameCallback;
    
    private void Awake()
    {
        _btnClose.onClick.AddListener(BackToSDK);
        _btnBackToSDK.onClick.AddListener(BackToSDK);
        _btnContinuePlaying.onClick.AddListener(BackToGame);
    }

    private void BackToGame()
    {
        _popupContainer.Pop();
        _onBackToGameCallback?.Invoke();
    }

    private void BackToSDK()
    {
        _onBackToSDKCallback?.Invoke();
        _popupContainer.Pop();
    }

    public override void OnAfterPopPopup()
    {
        base.OnAfterPopPopup();
        _onBackToSDKCallback = null;
        _onBackToGameCallback = null;
    }

    public void SetCallback(Action onBackToSDKCallback, Action onBackToGameCallback)
    {
        _onBackToSDKCallback = onBackToSDKCallback;
        _onBackToGameCallback = onBackToGameCallback;
    }
}
