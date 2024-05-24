using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Wolffun.RestAPI.ThetanAuth
{
    /// <summary>
    /// Authen processor used for login as guess, login with WFID, register WFID, link guess account with WFID
    /// </summary>
    public class WolffunIdAuthenProcess : MonoBehaviorAuthenProcess
    {
        [SerializeField] private int _typeGameID = -1;

        [Header("Extension (Optional)")]
        [SerializeField] private DeviceIDContainer _customDeviceIDContainer;

        private string _emailRegister;
        private string _emailLogin;
        private string _emailLinkAccount;

        public string EmailRegister => _emailRegister;
        public string EmailLogin => _emailLogin;
        public string EmailLinkAccount => _emailLinkAccount;
        
        #region Override Base Class

        protected override PostAuthenSuccessMetaData AddCustomDataToMetaData(PostAuthenSuccessMetaData metaData)
        {
            metaData.deviceId = GetDeviceID();

            return metaData;
        }

        #endregion

        #region Register Account;

        public void SendRequestRegisterNewAccount(string email, Action onSuccessCallback, Action<WolffunResponseError> onErrorCallback)
        {
            ThetaAuthAPI.SendCode(new SendCodeRequestModel()
            {
                createAccount = true,
                email = email,
                fromUnity = true
            }, _ =>
            {
                _emailRegister = email;
                onSuccessCallback?.Invoke();
            }, error =>
            {
                _emailRegister = string.Empty;
                onErrorCallback?.Invoke(error);
            });
        }

        public void ConfirmCodeRegisterAccount(int code, Action onSuccessCallback, Action<WolffunResponseError> onErrorCallback)
        {
            if (string.IsNullOrEmpty(_emailRegister))
            {
                onErrorCallback?.Invoke(new WolffunResponseError(
                    (int)WSErrorCode.InvalidEmail,
                    "You must call SendRequestRegisterNewAccount and wait for success before call confirm register with code"));
                return;
            }

            ThetaAuthAPI.CreateAccount(new CreateAccountRequestModel
            {
                email = _emailRegister,
                code = code,
                deviceId = GetDeviceID(),
                typeGame = _typeGameID
            }, result =>
            {
                ProcessAuthenSucceed(LoginType.WolffunId, result.accessToken, result.refreshToken);
                onSuccessCallback?.Invoke();
                _emailRegister = string.Empty;

            }, onErrorCallback);
        }

        public void ResendCodeRegister(Action onSuccessCallback, Action<WolffunResponseError> onErrorCallback)
        {
            if (string.IsNullOrEmpty(_emailRegister))
            {
                onErrorCallback?.Invoke(new WolffunResponseError(
                    (int)WSErrorCode.InvalidEmail,
                    "You must call SendRequestRegisterNewAccount and wait for success before call resend code register"));
                return;
            }

            ThetaAuthAPI.SendCode(new SendCodeRequestModel()
            {
                createAccount = true,
                email = _emailRegister,
                fromUnity = true
            }, _ =>
            {
                onSuccessCallback?.Invoke();
            }, error =>
            {
                onErrorCallback?.Invoke(error);
            });
        }
        #endregion

        #region Login with email

        public void SendRequestLoginWithEmail(string email, Action onSuccessCallback, Action<WolffunResponseError> onErrorCallback)
        {
            ThetaAuthAPI.SendCode(new SendCodeRequestModel()
            {
                createAccount = false,
                email = email,
                fromUnity = true
            }, _ =>
            {
                _emailLogin = email;
                onSuccessCallback?.Invoke();
            }, error =>
            {
                _emailLogin = string.Empty;
                onErrorCallback?.Invoke(error);
            });
        }

        public void ConfirmCodeLoginWithEmail(int code, Action onSuccessCallback, Action<WolffunResponseError> onErrorCallback)
        {
            if (string.IsNullOrEmpty(_emailLogin))
            {
                onErrorCallback?.Invoke(new WolffunResponseError(
                    (int)WSErrorCode.InvalidEmail,
                    "You must call SendRequestLoginWithEmail and wait for success before call confirm login with code"));
                return;
            }

            ThetaAuthAPI.LoginAccount(new LoginAccountRequestModel
            {
                email = _emailLogin,
                code = code,
                deviceId = GetDeviceID(),
                typeGame = _typeGameID,
                fromUnity = true,
            }, result =>
            {
                ProcessAuthenSucceed(LoginType.WolffunId, result.accessToken, result.refreshToken);
                onSuccessCallback?.Invoke();
                _emailLogin = string.Empty;

            }, error =>
            {
                onErrorCallback?.Invoke(error);
            });
        }

        public void ResendCodeLogin(Action onSuccessCallback, Action<WolffunResponseError> onErrorCallback)
        {
            if (string.IsNullOrEmpty(_emailLogin))
            {
                onErrorCallback?.Invoke(new WolffunResponseError(
                    (int)WSErrorCode.InvalidEmail,
                    "You must call SendRequestLoginWithEmail and wait for success before call resend login code"));
                return;
            }

            ThetaAuthAPI.SendCode(new SendCodeRequestModel()
            {
                createAccount = false,
                email = _emailLogin,
                fromUnity = true
            }, _ =>
            {
                onSuccessCallback?.Invoke();
            }, error =>
            {
                onErrorCallback?.Invoke(error);
            });
        }

        #endregion

        #region Login with guess account

        public void LoginWithGuessAcount(Action<AuthenResultData> onSuccessCallback, Action<WolffunResponseError> onErrorCallback)
        {
            ThetaAuthAPI.LoginGuessAccount(new LoginGuessAccountRequestModel()
            {
                deviceId = GetDeviceID(),
                typeGame = _typeGameID,

            }, (result) =>
            {
                ProcessAuthenSucceed(LoginType.GuessAccount, result.accessToken, result.refreshToken);
                onSuccessCallback?.Invoke(AuthenResultData.CreateLoginAsGuestResult());
            }, onErrorCallback);
        }
        #endregion

        #region Link account
        public void SendRequestLinkAccount(string email, Action onSuccessCallback, Action<WolffunResponseError> onErrorCallback)
        {
            ThetaAuthAPI.SendCode(new SendCodeRequestModel()
            {
                createAccount = true,
                email = email,
                fromUnity = true
            }, _ =>
            {
                _emailLinkAccount = email;
                onSuccessCallback?.Invoke();
            }, error =>
            {
                _emailLinkAccount = string.Empty;
                onErrorCallback?.Invoke(error);
            });
        }

        public void ConfirmCodeLinkAccount(int code, Action onSuccessCallback,
            Action<WolffunResponseError> onErrorCallback)
        {
            if (string.IsNullOrEmpty(_emailLinkAccount))
            {
                onErrorCallback?.Invoke(new WolffunResponseError(
                    (int)WSErrorCode.InvalidEmail,
                    "You must call SendRequestLinkAccount and wait for success before call confirm link account with code"));
                return;
            }

            ThetaAuthAPI.LinkAccount(new CreateAccountRequestModel
            {
                email = _emailLinkAccount,
                code = code,
                deviceId = GetDeviceID(),
                typeGame = _typeGameID
            }, result =>
            {
                ProcessPostAuthenSuccess(LoginType.LinkAccount).Forget();
                onSuccessCallback?.Invoke();
                _emailLinkAccount = string.Empty;
            }, error =>
            {
                
                onErrorCallback?.Invoke(error);
            });
        }

        public void ResendCodeLinkAccount(Action onSuccessCallback, Action<WolffunResponseError> onErrorCallback)
        {
            if (string.IsNullOrEmpty(_emailLinkAccount))
            {
                onErrorCallback?.Invoke(new WolffunResponseError(
                    (int)WSErrorCode.InvalidEmail,
                    "You must call SendRequestLinkAccount and wait for success before call resend link account"));
                return;
            }
            
            ThetaAuthAPI.SendCode(new SendCodeRequestModel()
            {
                createAccount = true,
                email = _emailLinkAccount,
                fromUnity = true
            }, _ =>
            {
                onSuccessCallback?.Invoke();
            }, error =>
            {
                onErrorCallback?.Invoke(error);
            });
        }
        #endregion

        #region DeviceID

        protected string GetDeviceID()
        {
            if (_customDeviceIDContainer != null)
                return _customDeviceIDContainer.GetDeviceID();

            return GetDefaultDeviceID();
        }

        private string GetDefaultDeviceID() => Wolffun.RestAPI.Utils.GetDefaultDeviceID();

        #endregion
    }
}

