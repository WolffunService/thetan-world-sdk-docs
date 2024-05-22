using System;
using TMPro;
using UnityEngine;
using Wolffun.RestAPI;
using Wolffun.RestAPI.ThetanAuth;

namespace ThetanSDK.UI.Authen.UIProcess
{
    public class ScreenLoginConfirmCode : ScreenConfirmCode
    {
        public override void Initialize(WolffunIdAuthenProcess wolffunIdAuthenProcess, UIHelperContainer uiHelperContainer,
            Action<AuthenResultData> onLoginSuccess)
        {
            base.Initialize(wolffunIdAuthenProcess, uiHelperContainer, onLoginSuccess);
        }

        protected override void DoSendCodeLogic(int code, Action onSuccessCallback, Action<WolffunResponseError> onErrorCallback)
        {
            _wolffunIdAuthenProcess.ConfirmCodeLoginWithEmail(code, onSuccessCallback, onErrorCallback);
        }

        protected override void DoResendCodeLogic(Action onSuccessCallback, Action<WolffunResponseError> onErrorCallback)
        {
            _wolffunIdAuthenProcess.ResendCodeLogin(onSuccessCallback, onErrorCallback);
        }

        protected override AuthenResultData GetSuccessAuthResult()
        {
            return AuthenResultData.CreateLoginByEmailResult(_wolffunIdAuthenProcess.EmailLogin);
        }

        protected override string GetEmailSentCode()
        {
            return _wolffunIdAuthenProcess.EmailLogin;
        }
    }
}