using System;
using Wolffun.RestAPI;
using Wolffun.RestAPI.ThetanAuth;

namespace ThetanSDK.UI.Authen.UIProcess
{
    public class ScreenRegisterConfirmCode : ScreenConfirmCode
    {
        protected override void DoSendCodeLogic(int code, Action onSuccessCallback, Action<WolffunResponseError> onErrorCallback)
        {
            _wolffunIdAuthenProcess.ConfirmCodeRegisterAccount(code, onSuccessCallback, onErrorCallback);
        }

        protected override void DoResendCodeLogic(Action onSuccessCallback, Action<WolffunResponseError> onErrorCallback)
        {
            _wolffunIdAuthenProcess.ResendCodeRegister(onSuccessCallback, onErrorCallback);
        }

        protected override AuthenResultData GetSuccessAuthResult()
        {
            return AuthenResultData.CreateRegistersByEmailResult(_wolffunIdAuthenProcess.EmailRegister);
        }

        protected override string GetEmailSentCode()
        {
            return _wolffunIdAuthenProcess.EmailRegister;
        }
    }
}