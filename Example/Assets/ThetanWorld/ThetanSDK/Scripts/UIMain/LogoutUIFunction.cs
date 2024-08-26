using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LogoutUIFunction : MonoBehaviour
{
    [SerializeField] private Button _btnLogout;
    [SerializeField] private ConfirmLogOutPopup _prefabPopupLogOut;

    private UIHelperContainer _uiHelperContainer;
    private Action _onLogOutCallback;
    
    private void Awake()
    {
        _btnLogout.onClick.AddListener(OnClickLogOut);
    }

    public void Initialize(UIHelperContainer uiHelperContainer, Action onConfirmLogOutCallback)
    {
        _uiHelperContainer = uiHelperContainer;
        _onLogOutCallback = onConfirmLogOutCallback;
    }

    private async void OnClickLogOut()
    {
        if (_uiHelperContainer == null)
            return;

        var popup = await _uiHelperContainer.PushPopup(_prefabPopupLogOut, new PopupOption()
        {
            IsAllowBackdrop = true,
            IsBackdropCanClosePopup = false,
        }) as ConfirmLogOutPopup;
        
        popup.SetCallback(() =>
        {
            _onLogOutCallback?.Invoke();
        }, null);
    }
}
