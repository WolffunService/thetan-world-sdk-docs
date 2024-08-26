using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Wolffun.RestAPI;
using Wolffun.RestAPI.ThetanAuth;

namespace ThetanSDK.UI
{
    public class AuthenUIControl : MonoBehaviour
    {
        private LoginMainUI _instanceLoginUI;

        public void ShowUISelectLoginMethodWithCloseButton(NetworkClient networkClient,
            AuthenProcessContainer authenProcessContainer,
            Action<AuthenResultData> onAuthenSuccess, Action onCancelCallback)
        {
            _instanceLoginUI = Instantiate(GetPrefabLoginUI(false), this.transform).GetComponent<LoginMainUI>();
            _instanceLoginUI.InitializeUI(networkClient, authenProcessContainer);
            _instanceLoginUI.ShowScreenSelectLoginMethod(onAuthenSuccess, onCancelCallback);
        }

        public void ShowUILinkAccount(NetworkClient networkClient, AuthenProcessContainer authenProcessContainer,
            Action<AuthenResultData> onLinkAccountSuccess, Action onCancelCallback)
        {
            _instanceLoginUI = Instantiate(GetPrefabLoginUI(false), this.transform).GetComponent<LoginMainUI>();
            _instanceLoginUI.InitializeUI(networkClient, authenProcessContainer);
            _instanceLoginUI.ShowScreenLinkAccount(onLinkAccountSuccess, onCancelCallback);
        }

        private GameObject GetPrefabLoginUI(bool isFullscreen)
        {
            var screenType = Utils.GetCurrentScreenType();
            if (screenType == Utils.ScreenType.Portrait)
            {
                if(isFullscreen)
                    return Resources.Load<GameObject>("UILogin_Portrait");
                else
                    return Resources.Load<GameObject>("Popup_UILogin_Portrait");
            }
            else
            {
                if(isFullscreen)
                    return Resources.Load<GameObject>("UILogin_Landscape");
                else
                    return Resources.Load<GameObject>("UILogin_Landscape_Popup");
            }
        }
    }
}
