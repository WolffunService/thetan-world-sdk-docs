using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using Wolffun.RestAPI;

namespace ThetanSDK.UI.Authen.UIProcess
{
    public class ScreenSendCodeRegister : ScreenSendCodeWithEmail, IPointerClickHandler
    {
        [SerializeField] private TextMeshProUGUI _txtTermAndPrivacy;

        public Action<string> OnRequestSwitchToLoginWithEmail;
        
        protected override void DoSendCodeLogic(string email, Action onSuccessCallback, Action<WolffunResponseError> onErrorCallback)
        {
            _wolffunIDAuthenProcess.SendRequestRegisterNewAccount(email, onSuccessCallback, onErrorCallback);
        }

        protected override void DoSubActionLogic(string email, Action onSuccessCallback, Action<WolffunResponseError> onErrorCallback)
        {
            _wolffunIDAuthenProcess.SendRequestLoginWithEmail(email, onSuccessCallback, onErrorCallback);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            /*
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                var index = TMP_TextUtilities.FindIntersectingLink(_txtTermAndPrivacy, Input.mousePosition, null);

                if (index > -1)
                {
                    Application.OpenURL(_txtTermAndPrivacy.textInfo.linkInfo[index].GetLinkID());
                }
            }
            */
        }

        protected override bool HandleSpecificSendCodeError(string email, WolffunResponseError sendCodeError)
        {
            if (sendCodeError.Code == (int)WSErrorCode.EmailIsExist)
            {
                _uiHelperContainer.ShowPopUpMsg(AuthenErrorMsg.InvalidEmail, AuthenErrorMsg.EmailExistedContext,
                    AuthenErrorMsg.Okay, () => SwitchToScreenLoginWithEmail(email));
                return true;
            }
            
            return false;
        }

        private void SwitchToScreenLoginWithEmail(string email)
        {
            OnRequestSwitchToLoginWithEmail?.Invoke(email);
        }
    }
}