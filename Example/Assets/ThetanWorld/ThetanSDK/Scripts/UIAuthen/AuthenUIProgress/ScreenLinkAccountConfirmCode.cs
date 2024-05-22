using System;
using System.Collections;
using System.Collections.Generic;
using ThetanSDK.UI;
using ThetanSDK.UI.Authen.UIProcess;
using UnityEngine;
using Wolffun.RestAPI;
using Wolffun.RestAPI.ThetanAuth;

namespace ThetanSDK.UI.Authen.UIProcess
{
    public class ScreenLinkAccountConfirmCode : ScreenConfirmCode
    {
        protected override void DoSendCodeLogic(int code, Action onSuccessCallback, Action<WolffunResponseError> onErrorCallback)
        {
            _wolffunIdAuthenProcess.ConfirmCodeLinkAccount(code, onSuccessCallback, onErrorCallback);
        }

        protected override void DoResendCodeLogic(Action onSuccessCallback, Action<WolffunResponseError> onErrorCallback)
        {
            _wolffunIdAuthenProcess.ResendCodeLinkAccount(onSuccessCallback, onErrorCallback);
        }

        protected override AuthenResultData GetSuccessAuthResult()
        {
            return AuthenResultData.CreateLinkAccountResult(_wolffunIdAuthenProcess.EmailLinkAccount);
        }

        protected override string GetEmailSentCode()
        {
            return _wolffunIdAuthenProcess.EmailLinkAccount;
        }
    }
}