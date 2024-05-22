using System;
using System.Collections;
using System.Collections.Generic;
using ThetanSDK.UI;
using UnityEngine;
using Wolffun.RestAPI;


namespace ThetanSDK.UI.Authen.UIProcess
{
    public class ScreenSendCodeLinkAccount : ScreenSendCodeWithEmail
    {
        protected override void DoSendCodeLogic(string email, Action onSuccessCallback,
            Action<WolffunResponseError> onErrorCallback)
        {
            _wolffunIDAuthenProcess.SendRequestLinkAccount(email, onSuccessCallback, onErrorCallback);
        }

        protected override void DoSubActionLogic(string email, Action onSuccessCallback,
            Action<WolffunResponseError> onErrorCallback)
        {
            return;
        }
    }
}
