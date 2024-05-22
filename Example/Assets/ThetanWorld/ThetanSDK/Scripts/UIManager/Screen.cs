using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks.Triggers;
using UnityEngine;
using UnityEngine.UI;

namespace ThetanSDK.UI
{
    public class Screen : MonoBehaviour
    {
        [SerializeField] private Button _btnBack;

        [SerializeField] private Button _btnClose;
        
        protected ScreenContainer _screenContainer;

        public ScreenContainer ScreenContainer => _screenContainer;

        protected virtual void Start()
        {
            if(_btnBack)
            {
                _btnBack.onClick.AddListener(PopScreen);
            }

            if (_btnClose)
            {
                _btnClose.onClick.AddListener(OnClickClose);
            }
        }

        internal void SetScreenContainer(ScreenContainer screenContainer)
        {
            _screenContainer = screenContainer;
        }
        
        public virtual void OnBeforePushScreen()
        {}

        public virtual void OnAfterPushScreen()
        {}
        
        public virtual void OnBeforePopScreen()
        {}

        public virtual void OnAfterPopScreen()
        {
            _screenContainer = null;
        }

        private void PopScreen()
        {
            _screenContainer.PopScreen();
        }

        private void OnClickClose()
        {
            _screenContainer.NotifyOnClickCloseScreen();
        }

        public void DisableButtonCloseScreen()
        {
            if(_btnClose)
                _btnClose.gameObject.SetActive(false);
        }
        
        public void EnableButtonCloseScreen()
        {
            if(_btnClose)
                _btnClose.gameObject.SetActive(true);
        }
    }
}

