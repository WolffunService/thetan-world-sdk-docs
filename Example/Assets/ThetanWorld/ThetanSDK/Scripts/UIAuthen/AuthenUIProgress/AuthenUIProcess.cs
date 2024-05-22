using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Wolffun.RestAPI;
using Wolffun.RestAPI.ThetanAuth;

namespace ThetanSDK.UI.Authen.UIProcess
{
    public abstract class AuthenUIProcess : MonoBehaviour
    {
        protected AuthenProcessInfo _authenProcessInfo;
        protected NetworkClient _networkClient;
        protected ScreenContainer _screenContainer;
        protected UIHelperContainer _uiHelperContainer;

        protected Action<int> _onChangeProcessStepIndex;
        protected Action<AuthenResultData> _onAuthenSuccess;
        protected Action _onCancelProcess;

        public virtual void InitializeProcess(ScreenContainer screenContainer, UIHelperContainer uiHelperContainer, 
            NetworkClient networkClient, AuthenProcessContainer authenProcessContainer,
            Action<int> onChangeProcessStepIndex, Action<AuthenResultData> onAuthenSuccess,
            Action onCancelProcess)
        {
            _networkClient = networkClient;
            _screenContainer = screenContainer;
            _onChangeProcessStepIndex = onChangeProcessStepIndex;
            _onAuthenSuccess = onAuthenSuccess;
            _onCancelProcess = onCancelProcess;
            _uiHelperContainer = uiHelperContainer;
        }

        public AuthenProcessInfo GetProcessInfo() => _authenProcessInfo;
    }
}


