using System;
using System.Collections.Generic;
using Cysharp.Text;
using ThetanSDK.Utilities;
using UnityEngine;
using Wolffun.Log;
using Wolffun.RestAPI;
using Wolffun.RestAPI.ThetanAuth;

namespace ThetanSDK.UI.Authen.UIProcess
{
    public class ThetanAppAuthenUIProcess : AuthenUIProcess
    {
        private ThetanAppAuthenProcess _thetanAppAuthenProcess;
        private ContentUILoginWithThetanApp _uiLoginWithThetanApp;

        public override void InitializeProcess(ScreenContainer screenContainer, UIHelperContainer uiHelperContainer,
            NetworkClient networkClient, AuthenProcessContainer authenProcessContainer, Action<int> onChangeProcessStepIndex,
            Action<AuthenResultData> onAuthenSuccess, Action onCancelProcess)
        {
            base.InitializeProcess(screenContainer, uiHelperContainer, networkClient, authenProcessContainer, onChangeProcessStepIndex, onAuthenSuccess, onCancelProcess);

            _thetanAppAuthenProcess = authenProcessContainer.ThetanAppAuthenProcess;
        }

        public void SetUpContentUILoginWithThetanApp(ContentUILoginWithThetanApp uiLoginWithThetanApp)
        {
            _uiLoginWithThetanApp = uiLoginWithThetanApp;
            _uiLoginWithThetanApp.ShowQRPlaceHolder();
            _uiLoginWithThetanApp.OnUserClickOpenThetanAppCallback -= StartLoginUsingThetanApp;
            _uiLoginWithThetanApp.OnUserClickOpenThetanAppCallback += StartLoginUsingThetanApp;

            PrepareQRCode();
        }

        private void PrepareQRCode()
        {
            /* Bring back QRCode later
            _thetanAppAuthenProcess.PrepareForLoginWithQRCode(_networkClient, ShowQRCode, error =>
                {
                    QRCodeGenerator qrGenerator = new QRCodeGenerator();
                    QRCodeData qrCodeData = qrGenerator.CreateQrCode("https://thetanarena.com", QRCodeGenerator.ECCLevel.Q);
                    var qrCode = new Texture2DQRCode(qrCodeData);
                    var textureQRCode = qrCode.GetGraphic(10);
                    _uiLoginWithThetanApp.ShowErrorQRCode(ThetanSDKUtilities.CreateSpriteFromTexture2D(textureQRCode), PrepareQRCode);
                    CommonLog.LogError(error.Message);
                }, ShowQRCode, 
                () =>
                {
                    _uiHelperContainer.TurnOffLoading();
                    _onAuthenSuccess?.Invoke(new AuthenResultData()
                    {
                    });
                    _thetanAppAuthenProcess.ClearCache();
                });
                */
        }

        private void ShowQRCode(string loginUrl)
        {
            /*
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(loginUrl, QRCodeGenerator.ECCLevel.Q);
            var qrCode = new Texture2DQRCode(qrCodeData);
            var textureQRCode = qrCode.GetGraphic(10);
            
            _uiLoginWithThetanApp.SetQRCode(ThetanSDKUtilities.CreateSpriteFromTexture2D(textureQRCode));
            */
        }
        
        private void OnApplicationPause(bool pauseStatus)
        {
            if(!pauseStatus && _uiHelperContainer)
                _uiHelperContainer.TurnOffLoading();
        }
        
        private void OnApplicationFocus(bool hasFocus)
        {
            if(hasFocus && _uiHelperContainer)
                _uiHelperContainer.TurnOffLoading();
        }

        private void StartLoginUsingThetanApp()
        {
            _uiHelperContainer.TurnOnLoading();

            _thetanAppAuthenProcess.StartProcessLoginWithThetanApp(_networkClient, () =>
            {
                _uiHelperContainer.TurnOffLoading();
            }, error =>
            {
                _uiHelperContainer.TurnOffLoading();
                if (error.Code == -99)
                {
                    _uiHelperContainer.ShowPopUpMsg("Error", AuthenErrorMsg.TIME_OUT_OPEN_WS, "Confirm", null);
                }
                else if (error.Code == -98)
                {
                    _uiHelperContainer.ShowPopUpMsg("Error", AuthenErrorMsg.TIME_OUT_WAIT_LOGIN_CODE_DATA,
                        "Confirm", null);
                }
                else
                {
                    _uiHelperContainer.ShowPopUpMsg("Error",
                        ZString.Format(AuthenErrorMsg.UnknownErrorContext, error.Code, error.Message),
                        "Confirm", null);
                }

            });
        }

        public void ClearCacheData()
        {
            // Todo: uncomment me later
            //_thetanAppAuthenProcess.ClearCache();
            
            if(_uiLoginWithThetanApp != null)
                _uiLoginWithThetanApp.OnUserClickOpenThetanAppCallback -= StartLoginUsingThetanApp;
            
            _uiLoginWithThetanApp = null;
        }
    }
}