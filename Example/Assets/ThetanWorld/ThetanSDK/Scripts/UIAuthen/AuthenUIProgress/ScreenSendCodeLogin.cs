using System;
using Wolffun.RestAPI;

namespace ThetanSDK.UI.Authen.UIProcess
{
    public class ScreenSendCodeLogin : ScreenSendCodeWithEmail
    {
        public Action<string> OnRequestSwitchToSignUpWithEmail;
        
        protected override void DoSendCodeLogic(string email, Action onSuccessCallback, Action<WolffunResponseError> onErrorCallback)
        {
            _wolffunIDAuthenProcess.SendRequestLoginWithEmail(email, onSuccessCallback, onErrorCallback);
        }

        protected override void DoSubActionLogic(string email, Action onSuccessCallback, Action<WolffunResponseError> onErrorCallback)
        {
            _wolffunIDAuthenProcess.SendRequestRegisterNewAccount(email, onSuccessCallback, onErrorCallback);
        }

        protected override bool HandleSpecificSendCodeError(string email, WolffunResponseError sendCodeError)
        {
            if (sendCodeError.Code == (int)WSErrorCode.EmailNotExist)
            {
                _uiHelperContainer.ShowPopUpMsg("Invalid Email",
                    "That email doesn't exist. Please enter another or sign up for a new account.", 
                    "Enter Another", "Sign Up",
                    null, () => SwitchToSignUp(email));
                
                return true;
            }

            return false;
        }

        private void SwitchToSignUp(string email)
        {
            OnRequestSwitchToSignUpWithEmail?.Invoke(email);
        }
    }
}