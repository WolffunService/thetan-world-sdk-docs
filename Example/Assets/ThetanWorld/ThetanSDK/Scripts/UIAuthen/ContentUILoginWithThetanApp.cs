using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ThetanSDK.UI.Authen
{
    public class ContentUILoginWithThetanApp : MonoBehaviour
    {
        public Action OnUserClickOpenThetanAppCallback;

        private enum ContentType
        {
            TapOnThetanApp,
            QRCode
        }
        
        [SerializeField] private Button _btnSwitchLoginType;
        [SerializeField] private Image _imgLoginType;
        [SerializeField] private Sprite _sprIconTapThetanApp;
        [SerializeField] private Sprite _spriteIconQRCode;
        
        [Header("Tap on Thetan App")]
        [SerializeField] private GameObject _contentTapOnThetanApp;
        [SerializeField] private Button _btnTapOnThetanApp;

        [Header("QR Code")]
        [SerializeField] private GameObject _contentQRCode;
        [SerializeField] private Image _imgQRCode;
        [SerializeField] private Image _imgQRCodePlaceHolder;
        [SerializeField] private GameObject _contentQRCodeError;
        [SerializeField] private Button _btnRetryQRCode;
        [SerializeField] private GameObject _contentLoadingQR;

        private ContentType _currentContentType;
        private Action _onClickRetryQRCode;
        private bool _disableLoginTapOnThetaApp;
        
        private void Awake()
        {
            _btnRetryQRCode.onClick.AddListener(() =>
            {
                _contentQRCodeError.gameObject.SetActive(false);
                _contentLoadingQR.SetActive(true);
                _onClickRetryQRCode?.Invoke();
                _onClickRetryQRCode = null;
            });
            
            _btnTapOnThetanApp.onClick.AddListener(OnUserClickOnThetanApp);
            _btnSwitchLoginType.onClick.AddListener(OnClickSwitchLoginType);
            ChangeContentType(ContentType.TapOnThetanApp);
        }

        public void DisableTapOnThetanApp()
        {
            _disableLoginTapOnThetaApp = true;
            if (_currentContentType == ContentType.TapOnThetanApp)
            {
                ChangeContentType(ContentType.QRCode);
            }
        }

        private void OnClickSwitchLoginType()
        {
            if (_currentContentType == ContentType.QRCode && 
                !_disableLoginTapOnThetaApp)
            {
                ChangeContentType(ContentType.TapOnThetanApp);
            }
            else if (_currentContentType == ContentType.TapOnThetanApp)
            {
                ChangeContentType(ContentType.QRCode);
            }
        }

        public void SetQRCode(Sprite qrCode)
        {
            _imgQRCode.sprite = qrCode;
            _imgQRCode.enabled = true;
            _imgQRCodePlaceHolder.enabled = false;
            _contentQRCodeError.SetActive(false);
            
            _contentLoadingQR.SetActive(false);
        }

        public void ShowQRPlaceHolder()
        {
            _contentLoadingQR.SetActive(false);
            _imgQRCode.enabled = false;
            _imgQRCodePlaceHolder.enabled = true;
            _contentQRCodeError.SetActive(false);
        }

        public void ShowErrorQRCode(Sprite defaultQRCode, Action onClickRetryCallback)
        {
            SetQRCode(defaultQRCode);
            _contentQRCodeError.SetActive(true);
            _onClickRetryQRCode = onClickRetryCallback;
        }

        private void OnUserClickOnThetanApp()
        {
            OnUserClickOpenThetanAppCallback?.Invoke();
        }

        private void ChangeContentType(ContentType newContentType)
        {
            if (newContentType == ContentType.TapOnThetanApp &&
                _disableLoginTapOnThetaApp)
                return;
            
            _currentContentType = newContentType;

            if (newContentType == ContentType.TapOnThetanApp)
            {
                _imgLoginType.sprite = _spriteIconQRCode;
            }
            else if(newContentType == ContentType.QRCode)
            {
                _imgLoginType.sprite = _sprIconTapThetanApp;
            }
            
            _contentQRCode.SetActive(newContentType == ContentType.QRCode);
            _contentTapOnThetanApp.SetActive(newContentType == ContentType.TapOnThetanApp);
        }
    }
}
