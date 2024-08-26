using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks.Triggers;
using ThetanSDK.UI.Authen;
using UnityEngine;
using UnityEngine.UI;

namespace ThetanSDK.UI
{
    public class ScreenSelectLoginMethod : Screen
    {
        [SerializeField] private Button _btnLoginWFID;
        [SerializeField] private Button _btnRegisterWFID;
        [SerializeField] private Button _btnGuestAccount;
        [SerializeField] private ContentUILoginWithThetanApp _contentUILoginWithThetanApp;
        [SerializeField] private Button _btnFAQ;
        [SerializeField] private Screen _prefabScreenFAQ;

        public ContentUILoginWithThetanApp UILoginWithThetanApp => _contentUILoginWithThetanApp;

        private Action _onClickLoginMethod;
        private Action _onClickRegisterMethod;
        private Action _onClickLoginAsGuest;
        private Action _onClickLoginWithThetanApp;
        
        private void Awake()
        {
            _btnLoginWFID.onClick.AddListener(() => _onClickLoginMethod?.Invoke());
            _btnRegisterWFID.onClick.AddListener(() => _onClickRegisterMethod?.Invoke());
            _btnGuestAccount.onClick.AddListener(() => _onClickLoginAsGuest?.Invoke());
            _btnFAQ.onClick.AddListener(ShowScreenFAQ);
        }

        public void Initialize(Action onClickLoginMethod, Action onClickRegisterMethod, Action onClickPlayAsGuest)
        {
            _onClickLoginMethod = onClickLoginMethod;
            _onClickRegisterMethod = onClickRegisterMethod;
            _onClickLoginAsGuest = onClickPlayAsGuest;
            
            _contentUILoginWithThetanApp.ShowQRPlaceHolder();
        }

        private void ShowScreenFAQ()
        {
            Application.OpenURL("https://wolffun.helpshift.com/hc/en/7-thetan-world/");
            //_screenContainer.PushScreen(_prefabScreenFAQ);
        }
    }
}
