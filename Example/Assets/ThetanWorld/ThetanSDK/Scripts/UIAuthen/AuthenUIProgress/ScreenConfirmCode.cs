using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Wolffun.RestAPI;
using Wolffun.RestAPI.ThetanAuth;

namespace ThetanSDK.UI.Authen.UIProcess
{
    public abstract class ScreenConfirmCode : Screen
    {
        [SerializeField] private TMP_InputField _inputCode;
        [SerializeField] private Button _btnConfirmCode;
        [SerializeField] private TextMeshProUGUI _txtDesc;
        [SerializeField] private TextMeshProUGUI _txtErrorCode;
        [SerializeField] private Button _btnResendCode;

        
        protected WolffunIdAuthenProcess _wolffunIdAuthenProcess;
        private Action<AuthenResultData> _onLoginSuccess;
        protected UIHelperContainer _uiHelperContainer;

        private void Awake()
        {
            _inputCode.keyboardType = TouchScreenKeyboardType.DecimalPad;
            _inputCode.onValueChanged.AddListener(OnCodeChanged);
            _btnConfirmCode.onClick.AddListener(OnClickConfirmCode);
            
            if(_btnResendCode)
                _btnResendCode.onClick.AddListener(OnClickResendCode);
        }

        public override void OnBeforePushScreen()
        {
            base.OnBeforePushScreen();

            _inputCode.text = string.Empty;
            OnCodeChanged(string.Empty);
            
            if(_txtErrorCode)
                _txtErrorCode.gameObject.SetActive(false);
            
            if(_txtDesc)
                _txtDesc.text = string.Empty;
        }

        public override void OnAfterPushScreen()
        {
            base.OnAfterPushScreen();

            FocusOnInputField();
        }

        public override void OnAfterPopScreen()
        {
            base.OnAfterPopScreen();

            _wolffunIdAuthenProcess = null;
            _onLoginSuccess = null;
        }

        private void OnClickConfirmCode()
        {
            SetActiveLoading(true);
            
            var code = int.Parse(_inputCode.text);

            DoSendCodeLogic(code, () =>
            {
                SetActiveLoading(false);
                _onLoginSuccess?.Invoke(GetSuccessAuthResult());
            }, error =>
            {
                SetActiveLoading(false);
                switch ((WSErrorCode)error.Code)
                {
                    case WSErrorCode.UserBanned:
                        _uiHelperContainer.ShowPopUpMsg(AuthenErrorMsg.AccountBanned, AuthenErrorMsg.AccountBannedContactSupport,
                            AuthenErrorMsg.Confirm);
                        break;
                    case WSErrorCode.InvalidCode:
                        _uiHelperContainer.ShowPopUpMsg(AuthenErrorMsg.InvalidCode, AuthenErrorMsg.InvalidCodeContext,
                            AuthenErrorMsg.Okay, FocusOnInputField);
                        break;
                    case WSErrorCode.UnityHttpRequestNetworkError:
                        _uiHelperContainer.ShowPopUpMsg(AuthenErrorMsg.Error, AuthenErrorMsg.LostConnectionContext,
                            AuthenErrorMsg.Okay);
                        break;
                    default:
                        ThetanSDKManager.Instance.AnalyticService.LogErrorOccured("Login", "ConfirmCode", true,
                            error.Message);
                        _uiHelperContainer.ShowPopUpMsg(AuthenErrorMsg.Error, error.Message, AuthenErrorMsg.Okay);
                        break;
                }
                //_txtErrorCode.text = error.Message;
                //_txtErrorCode.gameObject.SetActive(true);
            });
        }

        private void FocusOnInputField()
        {
            EventSystem.current.SetSelectedGameObject(_inputCode.gameObject);
            _inputCode.ActivateInputField();
        }

        private void OnClickResendCode()
        {
            SetActiveLoading(true);
            
            DoResendCodeLogic(() =>
            {
                SetActiveLoading(false);
            }, error =>
            {
                SetActiveLoading(false);
                
                if(_txtErrorCode)
                {
                    _txtErrorCode.text = error.Message;
                    _txtErrorCode.gameObject.SetActive(true);
                }
            });
        }

        protected void SetActiveLoading(bool active)
        {
            if (_uiHelperContainer)
            {
                if(active)
                    _uiHelperContainer.TurnOnLoading();
                else
                    _uiHelperContainer.TurnOffLoading();
            }
        }

        protected abstract void DoSendCodeLogic(int code, Action onSuccessCallback,
            Action<WolffunResponseError> onErrorCallback);

        protected abstract void DoResendCodeLogic(Action onSuccessCallback,
            Action<WolffunResponseError> onErrorCallback);

        protected abstract AuthenResultData GetSuccessAuthResult();

        protected abstract string GetEmailSentCode();

        private void OnCodeChanged(string code)
        {
            int.TryParse(code, out int codeInt);
            bool codeValid = codeInt >= 100000 && codeInt <= 999999;
            _btnConfirmCode.interactable = codeValid;
            
            if(_txtErrorCode)
                _txtErrorCode.gameObject.SetActive(false);
        }

        public virtual void Initialize(WolffunIdAuthenProcess wolffunIdAuthenProcess, UIHelperContainer uiHelperContainer, Action<AuthenResultData> onLoginSuccess)
        {
            _wolffunIdAuthenProcess = wolffunIdAuthenProcess;
            _onLoginSuccess = onLoginSuccess;
            _uiHelperContainer = uiHelperContainer;
            
            if(_txtDesc)
                _txtDesc.text = $"Please enter the OTP sent to\n{GetEmailSentCode()}";
        }
    }
}