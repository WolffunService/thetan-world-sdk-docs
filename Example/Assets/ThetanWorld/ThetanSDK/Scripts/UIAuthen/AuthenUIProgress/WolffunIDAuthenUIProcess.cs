using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Wolffun.RestAPI;
using Wolffun.RestAPI.ThetanAuth;

namespace ThetanSDK.UI.Authen.UIProcess
{
    public class WolffunIDAuthenUIProcess : AuthenUIProcess
    {
        [SerializeField] private ScreenSendCodeLogin _prefabScreenSendCodeLogin;
        [SerializeField] private ScreenSendCodeRegister _prefabScreenSendCodeRegister;
        [SerializeField] private ScreenSendCodeLinkAccount _prefabScreenSendCodeLinkAccount;
        [SerializeField] private ScreenLoginConfirmCode _prefabScreenConfirmCodeLogin;
        [SerializeField] private ScreenRegisterConfirmCode _prefabScreenConfirmCodeRegister;
        [SerializeField] private ScreenLinkAccountConfirmCode _prefabScreenConfirmCodeLinkAccount;

        private WolffunIdAuthenProcess _authenProcess;
        private Action _onCancelLinkAccountCallback;

        public override void InitializeProcess(ScreenContainer screenContainer, UIHelperContainer uiHelperContainer,
            NetworkClient networkClient, AuthenProcessContainer authenProcessContainer,
            Action<int> onChangeProcessStepIndex, Action<AuthenResultData> onAuthenSuccess, Action onCancelProcess)
        {
            base.InitializeProcess(screenContainer, uiHelperContainer, networkClient, authenProcessContainer, onChangeProcessStepIndex, onAuthenSuccess, onCancelProcess);

            _authenProcess = authenProcessContainer.WFIDAuthenProcess;
        }

        private void OnDestroy()
        {
            _networkClient.UnregisterAuthenProcess(_authenProcess);
        }

        public async UniTask<AuthenProcessInfo> StartLoginProcess(string autoInputEmail = "")
        {
            var screenSendCode = await _screenContainer.ReplaceScreenStackByScreen(_prefabScreenSendCodeLogin, true)
                as ScreenSendCodeLogin;
            
            screenSendCode.Initialize(_authenProcess, _uiHelperContainer,
                SwitchToSceneConfirmCodeLogin, 
                SwitchToSceneConfirmCodeRegister,
                OnExitProcess);
            screenSendCode.OnRequestSwitchToSignUpWithEmail = SwitchToScreenSendCodeRegisterWithEmail;
            
            if(!string.IsNullOrEmpty(autoInputEmail))
                screenSendCode.AutoSendCodeForEmail(autoInputEmail);

            _authenProcessInfo = new AuthenProcessInfo()
            {
                ListStepProcess = new List<AuthenStepInfo>()
                {
                    new AuthenStepInfo() { StepName = "Email" },
                    new AuthenStepInfo() { StepName = "Verify" }
                },
                currentStepIndex = 0
            };

            return _authenProcessInfo;
        }

        private void SwitchToScreenSendCodeRegisterWithEmail(string email)
        {
            StartRegisterProcess(email).Forget();
        }
        
        public async UniTask<AuthenProcessInfo> StartRegisterProcess(string autoInputEmail = "")
        {
            var screenSendCode = await _screenContainer.ReplaceScreenStackByScreen(_prefabScreenSendCodeRegister, true) 
                as ScreenSendCodeRegister;
            
            screenSendCode.Initialize(_authenProcess, _uiHelperContainer,
                SwitchToSceneConfirmCodeRegister, 
                SwitchToSceneConfirmCodeLogin, 
                OnExitProcess);
            screenSendCode.OnRequestSwitchToLoginWithEmail = SwitchToScreenSendCodeLoginWithEmail;
            if(!string.IsNullOrEmpty(autoInputEmail))
                screenSendCode.AutoSendCodeForEmail(autoInputEmail);

            _authenProcessInfo = new AuthenProcessInfo()
            {
                ListStepProcess = new List<AuthenStepInfo>()
                {
                    new AuthenStepInfo() { StepName = "Email" },
                    new AuthenStepInfo() { StepName = "Verify" }
                },
                currentStepIndex = 0
            };
            
            return _authenProcessInfo;
        }

        private void SwitchToScreenSendCodeLoginWithEmail(string email)
        {
            StartLoginProcess(email).Forget();
        }

        public async UniTask<AuthenProcessInfo> StartLinkAccountProcess(Action onCancelLinkAccountCallback)
        {
            _onCancelLinkAccountCallback = onCancelLinkAccountCallback;
            
            var screenSendCode =
                await _screenContainer.PushScreen(_prefabScreenSendCodeLinkAccount) as ScreenSendCodeLinkAccount;

            screenSendCode.Initialize(_authenProcess, _uiHelperContainer, SwitchToSceneConfirmCodeLinkAccount, null,
                OnCancelLinkAccount);
            
            _authenProcessInfo = new AuthenProcessInfo()
            {
                ListStepProcess = new List<AuthenStepInfo>()
                {
                    new AuthenStepInfo() { StepName = "Email" },
                    new AuthenStepInfo() { StepName = "Verify" }
                },
                currentStepIndex = 0
            };
            
            return _authenProcessInfo;
        }

        public void StartPlayAsGuestProcess()
        {
            _authenProcess.LoginWithGuessAcount(_onAuthenSuccess, HandleLogInAsGuessError);
        }

        private void HandleLogInAsGuessError(WolffunResponseError error)
        {
            var analticService = ThetanSDKManager.Instance.AnalyticService;
            switch ((WSErrorCode)error.Code)
            {
                case WSErrorCode.AccountHasBeenLinked:
                    _uiHelperContainer.ShowPopUpMsg(AuthenErrorMsg.Error, AuthenErrorMsg.ConnectedAccountContext,
                        AuthenErrorMsg.Okay);
                    break;
                case WSErrorCode.TooManyAccountLogin:
                    _uiHelperContainer.ShowPopUpMsg(AuthenErrorMsg.Error, AuthenErrorMsg.TooManyAccountContext,
                        AuthenErrorMsg.Okay);
                    break;
                case WSErrorCode.UnityHttpRequestNetworkError:
                    analticService.LogErrorOccured("Login", "LoginAsGuest", true, "NetworkError");
                    _uiHelperContainer.ShowPopUpMsg(AuthenErrorMsg.Error, AuthenErrorMsg.LostConnectionContext,
                        AuthenErrorMsg.Okay);
                    break;
                default:
                    analticService.LogErrorOccured("Login", "LoginAsGuest", true, error.Message);
                    _uiHelperContainer.ShowPopUpMsg(AuthenErrorMsg.Error, AuthenErrorMsg.UnknownErrorContext,
                        AuthenErrorMsg.Okay);
                    break;
            }
        }

        private async void SwitchToSceneConfirmCodeLogin()
        {
            _authenProcessInfo.currentStepIndex = 1;
            _onChangeProcessStepIndex?.Invoke(_authenProcessInfo.currentStepIndex);
            
            var screenConfirmCode = await _screenContainer.PushScreen(_prefabScreenConfirmCodeLogin) as ScreenConfirmCode;
            
            screenConfirmCode.Initialize(_authenProcess, _uiHelperContainer, _onAuthenSuccess);
        }

        private async void SwitchToSceneConfirmCodeRegister()
        {
            _authenProcessInfo.currentStepIndex = 1;
            _onChangeProcessStepIndex?.Invoke(_authenProcessInfo.currentStepIndex);
            
            var screenConfirmCode = await _screenContainer.PushScreen(_prefabScreenConfirmCodeRegister) as ScreenConfirmCode;
            
            screenConfirmCode.Initialize(_authenProcess, _uiHelperContainer, _onAuthenSuccess);
        }

        private async void SwitchToSceneConfirmCodeLinkAccount()
        {
            _authenProcessInfo.currentStepIndex = 1;
            _onChangeProcessStepIndex?.Invoke(_authenProcessInfo.currentStepIndex);
            
            var screenConfirmCode =
                await _screenContainer.PushScreen(_prefabScreenConfirmCodeLinkAccount) as ScreenLinkAccountConfirmCode;
            
            screenConfirmCode.Initialize(_authenProcess, _uiHelperContainer, _onAuthenSuccess);
        }

        private void OnExitProcess()
        {
            _onCancelProcess?.Invoke();
        }

        private void OnCancelLinkAccount()
        {
            OnExitProcess();
            _onCancelLinkAccountCallback?.Invoke();
        }
    }
}