using System;
using System.Collections;
using System.Collections.Generic;
using ThetanSDK.UI;
using ThetanSDK.UI.Authen.UIProcess;
using UnityEngine;
using UnityEngine.UI;
using Wolffun.RestAPI;
using Wolffun.RestAPI.ThetanAuth;

namespace ThetanSDK.UI
{
    public class LoginMainUI : MonoBehaviour
    {
        [SerializeField] protected ScreenContainer _screenContainer;

        [SerializeField] private ScreenSelectLoginMethod _prefabScreenSelectLoginMethod;

        [SerializeField] private WolffunIDAuthenUIProcess _wfidUIProcess;

        [SerializeField] private ThetanAppAuthenUIProcess _thetanAppAuthenUIProcess;

        [SerializeField] private UIHelperContainer _uiHelperContainer;

        private NetworkClient _networkClient;
        private Action<AuthenResultData> _onAuthenSuccess;
        private Action _onCancelCallback;
        private AuthenProcessContainer _authenProcessContainer;

        private void Start()
        {
            _screenContainer.RegisterOnClickCloseScreen(OnClickCloseScreen);
        }

        private void OnClickCloseScreen()
        {
            if(_screenContainer != null)
                _screenContainer.PopAllScreen();
            
            _thetanAppAuthenUIProcess.ClearCacheData();
            
            _onCancelCallback?.Invoke();
        }

        public void InitializeUI(NetworkClient networkClient, AuthenProcessContainer authenProcessContainer)
        {
            _authenProcessContainer = authenProcessContainer;
            
            _wfidUIProcess.InitializeProcess(this._screenContainer, _uiHelperContainer, networkClient, authenProcessContainer,
                OnChangeProgressStepIndex, OnAuthenSuccess, OnProcessCancel);
            
            // Todo: uncomment me later
            //_thetanAppAuthenUIProcess.InitializeProcess(_screenContainer, _uiHelperContainer, networkClient, authenProcessContainer,
            //    OnChangeProgressStepIndex, OnAuthenSuccess, OnProcessCancel);
            
            _networkClient = networkClient;
        }

        public async void ShowScreenSelectLoginMethod(Action<AuthenResultData> onAuthenSuccess, Action onUserCancelCallback)
        {
            _onAuthenSuccess = onAuthenSuccess;
            _onCancelCallback = onUserCancelCallback;
            
            _screenContainer.EnableButtonCloseScreen();

            var screenLoginMethod = await _screenContainer.PushScreen(_prefabScreenSelectLoginMethod) as ScreenSelectLoginMethod;
            
            screenLoginMethod.Initialize(OnClickLoginMethod, OnClickRegisterMethod);
            
            // Todo: uncomment me later
            //_thetanAppAuthenUIProcess.SetUpContentUILoginWithThetanApp(screenLoginMethod.UILoginWithThetanApp);
            
            OnAfterPushScreen();
        }

        public async void ShowScreenLinkAccount(Action<AuthenResultData> onLinkSuccess, Action onCancelCallback)
        {
            _screenContainer.EnableButtonCloseScreen();
            _onAuthenSuccess = onLinkSuccess;
            var processInfo = await _wfidUIProcess.StartLinkAccountProcess(onCancelCallback);
            OnStartProgress(processInfo);

            OnAfterPushScreen();
        }

        protected virtual void OnAfterPushScreen(){}
        
        private void OnChangeProgressStepIndex(int newIndex)
        {
            
        }

        private async void OnClickLoginMethod()
        {
            var processInfo = await _wfidUIProcess.StartLoginProcess();
            
            OnStartProgress(processInfo);
        }

        private async void OnClickRegisterMethod()
        {
            var processInfo = await _wfidUIProcess.StartRegisterProcess();

            OnStartProgress(processInfo);
        }

        private void OnAuthenSuccess(AuthenResultData result)
        {
            Debug.Log("OnAuthenSuccess");
            _onAuthenSuccess?.Invoke(result);
            
            _thetanAppAuthenUIProcess.ClearCacheData();
        }

        private void OnStartProgress(AuthenProcessInfo progressInfo)
        {
        }
        
        private void OnProcessCancel()
        {
        }
    }
}

