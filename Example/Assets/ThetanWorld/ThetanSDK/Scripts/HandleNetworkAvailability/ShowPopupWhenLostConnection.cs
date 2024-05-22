using System;
using System.Collections;
using System.Collections.Generic;
using ThetanSDK.Utilities;
using UnityEngine;
using Wolffun.RestAPI;

namespace ThetanSDK.UI.Connection
{
    public class ShowPopupWhenLostConnection : MonoBehaviour
    {
        private NetworkClient _networkClient;
        private UIHelperContainer _uiHelperContainer;
        private Action _onConfirmLostConnectionCallback;

        private PopupNoInternet _prefabPopupLostConnection;

        private bool _hasShowPopup;
        
        public void Initialize(NetworkClient networkClient, UIHelperContainer uiHelperContainer, Action onConfirmLostConnectionCallback)
        {
            _hasShowPopup = false;
            _networkClient = networkClient;
            _uiHelperContainer = uiHelperContainer;
            _onConfirmLostConnectionCallback = onConfirmLostConnectionCallback;
            _networkClient.SubcribeOnChangeNetworkClientState(HandleOnChangeNetworkClientState);
        }

        private async void HandleOnChangeNetworkClientState(ThetanNetworkClientState newState)
        {
            if (newState == ThetanNetworkClientState.LoggedInNoNetwork)
            {
                if (_hasShowPopup)
                    return;
                
                _hasShowPopup = true;

                ShowPopupNoConnection();
                
                return;
            }

            if (newState == ThetanNetworkClientState.LoggedIn)
                _hasShowPopup = false;
        }

        public async void ShowPopupNoConnection()
        {
            if (_prefabPopupLostConnection == null)
                LoadPrefabPopup();
            
            if(_uiHelperContainer != null && _prefabPopupLostConnection != null)
            {
                // _uiHelperContainer.ShowPopUpMsg("Lost Connection",
                //     "Connection to server was interrupted, please check your network connection.",
                //     "Confirm", _onConfirmLostConnectionCallback);

                var popup = await _uiHelperContainer.PushPopup(_prefabPopupLostConnection, new PopupOption()
                {
                    IsAllowBackdrop = true,
                    IsBackdropCanClosePopup = false,
                }) as PopupNoInternet;
                    
                if(popup)
                    popup.SetCallback(_onConfirmLostConnectionCallback);
            }
        }

        private void LoadPrefabPopup()
        {
            if (Utils.GetCurrentScreenType() == Utils.ScreenType.Portrait)
            {
                _prefabPopupLostConnection = Resources.Load<PopupNoInternet>("PopupLostConnection_Portrait");
            }
            else
            {
                _prefabPopupLostConnection = Resources.Load<PopupNoInternet>("PopupLostConnection");
            }
            
        }
    }
}
