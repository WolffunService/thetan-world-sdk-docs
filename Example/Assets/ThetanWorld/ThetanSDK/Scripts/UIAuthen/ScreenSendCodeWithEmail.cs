using System;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Wolffun.RestAPI;
using Wolffun.RestAPI.ThetanAuth;

namespace ThetanSDK.UI
{
    public abstract class ScreenSendCodeWithEmail : Screen
    {
        [SerializeField] private TMP_InputField _inputEmail;
        [SerializeField] private Button _btnSendCode;
        [SerializeField] private TextMeshProUGUI _txtErrorMessage;
        [SerializeField] private TextMeshProUGUI _txtPromptMessage;
        [SerializeField] private Button _btnSubAction;
        
        protected WolffunIdAuthenProcess _wolffunIDAuthenProcess;
        private Action _onSendEmailSuccess;
        private Action _onSubActionSuccess;
        private Action _onPopScreenCallback;
        protected UIHelperContainer _uiHelperContainer;

        private void Awake()
        {
            _btnSendCode.onClick.AddListener(OnClickSendCode);
            _inputEmail.onValueChanged.AddListener(OnEmailChanged);
            
            if(_btnSubAction)
                _btnSubAction.onClick.AddListener(OnClickSubAction);
        }

        private void OnEmailChanged(string email)
        {
            bool emailValid = Utils.IsEmailValid(email);
            _btnSendCode.interactable = emailValid;

            if (_btnSubAction)
                _btnSubAction.interactable = emailValid;
        }

        public override void OnAfterPushScreen()
        {
            base.OnAfterPushScreen();

            if(string.IsNullOrEmpty(_inputEmail.text))
            {
                EventSystem.current.SetSelectedGameObject(_inputEmail.gameObject);
                _inputEmail.ActivateInputField();
            }
        }

        public override void OnBeforePushScreen()
        {
            base.OnBeforePushScreen();

            _inputEmail.text = string.Empty;
            _inputEmail.interactable = true;
            OnEmailChanged(string.Empty);
            
            if(_txtErrorMessage)
                _txtErrorMessage.text = string.Empty;
            
            if(_btnSubAction)
                _btnSubAction.gameObject.SetActive(false);
            ActiveTextError(false);
        }

        public override void OnAfterPopScreen()
        {
            base.OnAfterPopScreen();

            _wolffunIDAuthenProcess = null;
            _onSendEmailSuccess = null;
            _onSubActionSuccess = null;
            
            _onPopScreenCallback = null;
        }

        public override void OnBeforePopScreen()
        {
            base.OnBeforePopScreen();
            
            _onPopScreenCallback?.Invoke();
        }

        public void Initialize(WolffunIdAuthenProcess wolffunIdAuthenProcess, UIHelperContainer uiHelperContainer, Action onSendEmailSuccess, Action onSubActionSuccess, Action onPopScreenCallback)
        {
            _wolffunIDAuthenProcess = wolffunIdAuthenProcess;
            _onSendEmailSuccess = onSendEmailSuccess;
            _onSubActionSuccess = onSubActionSuccess;
            _onPopScreenCallback = onPopScreenCallback;
            _uiHelperContainer = uiHelperContainer;
        }

        public async void AutoSendCodeForEmail(string email)
        {
            _inputEmail.text = email;
            
            SetActiveBtnProcess(true);

            await UniTask.Delay(500, ignoreTimeScale: true);

            OnClickSendCode();
        }
        
        private void OnClickSendCode()
        {
            _inputEmail.interactable = false;
            SetActiveBtnProcess(true);
            
            DoSendCodeLogic(_inputEmail.text, () =>
            {
                OnSetTextError(string.Empty);
                SetActiveBtnProcess(false);
                _inputEmail.interactable = true;
                _onSendEmailSuccess?.Invoke();
            }, error =>
            {
                SetActiveBtnProcess(false);
                _inputEmail.interactable = true;
                
                var isHandledSpecificError = HandleSpecificSendCodeError(_inputEmail.text, error);

                if (isHandledSpecificError)
                    return;
                
                string errorText = error.Message;

                switch (error.Code)
                {
                    case (int)WSErrorCode.EmailNotExist:
                    {
                        _uiHelperContainer.ShowPopUpMsg(AuthenErrorMsg.InvalidEmail, AuthenErrorMsg.EmailNotExistContext,
                            AuthenErrorMsg.Okay);
                        if (_btnSubAction)
                        {
                            _btnSubAction.interactable = true;
                            _btnSubAction.gameObject.SetActive(true);
                        }
                        break;
                    }
                    case (int)WSErrorCode.EmailIsExist:
                    {
                        _uiHelperContainer.ShowPopUpMsg(AuthenErrorMsg.InvalidEmail, AuthenErrorMsg.EmailExistedContext,
                            AuthenErrorMsg.Okay);
                        if (_btnSubAction)
                        {
                            _btnSubAction.interactable = true;
                            _btnSubAction.gameObject.SetActive(true);
                        }
                        break;
                    }
                    default:
                        ThetanSDKManager.Instance.AnalyticService.LogErrorOccured("Login", "SendCode", true,
                            error.Message);
                        _uiHelperContainer.ShowPopUpMsg(AuthenErrorMsg.Error, error.Message,
                            AuthenErrorMsg.Okay);
                        break;
                }
                
                //OnSetTextError(errorText);
            });
        }
        
        private void OnSetTextError(string message)
        {
            if (_txtErrorMessage == null) return;
            if (string.IsNullOrEmpty(message))
            {
                ActiveTextError(false);
                return;

            }
            else
            {
                _txtErrorMessage.text = message;
            }
            ActivePromptText(false);
            ActiveTextError(true);
        }
        
        private void ActiveTextError(bool isActive)
        {
            if (_txtErrorMessage)
            {
                _txtErrorMessage.gameObject.SetActive(isActive);
            }
        }
        
        private void ActivePromptText(bool isActive)
        {
            if (_txtPromptMessage)
            {
                _txtPromptMessage.gameObject.SetActive(isActive);
            }
        }

        private void SetActiveBtnProcess(bool active)
        {
            if(_uiHelperContainer)
            {
                if(active)
                    _uiHelperContainer.TurnOnLoading();
                else
                    _uiHelperContainer.TurnOffLoading();
            }
        }

        protected abstract void DoSendCodeLogic(string email, Action onSuccessCallback, Action<WolffunResponseError> onErrorCallback);

        protected abstract void DoSubActionLogic(string email, Action onSuccessCallback,
            Action<WolffunResponseError> onErrorCallback);

        protected virtual bool HandleSpecificSendCodeError(string email, WolffunResponseError sendCodeError) => false;
        
        private void OnClickSubAction()
        {
            _inputEmail.interactable = false;
            SetActiveBtnProcess(true);
            _btnSubAction.interactable = false;
            DoSubActionLogic(_inputEmail.text, () =>
            {
                OnSetTextError(string.Empty);
                _btnSubAction.gameObject.SetActive(false);
                SetActiveBtnProcess(false);
                _inputEmail.interactable = true;
                _onSubActionSuccess?.Invoke();
            }, async error =>
            {
                string errorText = error.Message;

                _btnSubAction.gameObject.SetActive(false);
                OnSetTextError(errorText);
                SetActiveBtnProcess(false);
                _inputEmail.interactable = true;
            });
        }
    }
}