using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using ThetanSDK.UI;
using UnityEngine;
using UnityEngine.Diagnostics;
using Utils = Wolffun.RestAPI.Utils;

public class UIHelperContainer : MonoBehaviour
{
    [SerializeField] private GameObject _goLoading;
    [SerializeField] private PopupContainer _popupContainer;
    [SerializeField] private ShowToastBehavior _toastBehavior;
    [SerializeField] private TooltipText _prefabPopupTooltip;

    private PopupMsg _prefabPopupMsg;
    
    private void Awake()
    {
        TurnOffLoading();
    }

    public void TurnOnLoading()
    {
        if(_goLoading)
            _goLoading.SetActive(true);
    }

    public void TurnOffLoading()
    {
        if(_goLoading)
            _goLoading.SetActive(false);
    }

    public async void ShowPopUpMsg(string title, string msg, string buttonText, Action onConfirmCallback = null)
    {
        if(_prefabPopupMsg == null)
            LoadPopupMessage();

        PopupMsg popupMsg = await PushPopup(_prefabPopupMsg, new PopupOption()
        {
            IsAllowBackdrop = false,
            SkipAnimation = true
        }) as PopupMsg;
        
        if(popupMsg)
            popupMsg.ShowPopUp(title, msg, buttonText, onConfirmCallback);
    }

    public async void ShowPopUpMsg(string title, string msg, string buttonTextLeft,
        string buttonTextRight, Action onLeftConfirmCallback = null, Action onRightConfirmCallback = null)
    {
        if(_prefabPopupMsg == null)
            LoadPopupMessage();

        PopupMsg popupMsg = await PushPopup(_prefabPopupMsg, new PopupOption()
        {
            IsAllowBackdrop = false,
            SkipAnimation = true
        }) as PopupMsg;
        
        if(popupMsg)
            popupMsg.ShowPopUp(title, msg, buttonTextLeft, buttonTextRight, onLeftConfirmCallback, onRightConfirmCallback);
    }

    public async UniTask<Popup> PushPopup(Popup prefab, PopupOption options)
    {
        if (_popupContainer == null)
            return null;

        return await _popupContainer.Push(prefab, options);
    }

    public void CloseAllPopup()
    {
        if(_popupContainer != null)
            _popupContainer.PopAll();
    }

    public void ShowToast(string msg)
    {
        if (_toastBehavior == null)
            return;
        
        _toastBehavior.ShowText(msg);
    }

    public async void ShowTextTooltip(string msg, RectTransform targetTransform, TooltipAlignment tooltipAlignment)
    {
        if (_prefabPopupTooltip == null ||
            _popupContainer == null)
            return;

        TooltipText tooltipText = await _popupContainer.Push(_prefabPopupTooltip, new PopupOption()
        {
            IsAllowBackdrop = false,
            IsBackdropCanClosePopup = false,
            SkipAnimation = true
        }) as TooltipText;
        
        tooltipText.ShowTooltip(msg, targetTransform, tooltipAlignment);
    }

    private void LoadPopupMessage()
    {
        if (_prefabPopupMsg != null)
            return;

        if (Utils.GetCurrentScreenType() == Utils.ScreenType.Portrait)
            _prefabPopupMsg = Resources.Load<PopupMsg>("PopupErrorMsg_Portrait");
        else
            _prefabPopupMsg = Resources.Load<PopupMsg>("PopupErrorMsg");
    }
}
