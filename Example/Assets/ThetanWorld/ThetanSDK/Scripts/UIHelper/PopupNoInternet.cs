using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ThetanSDK.UI.Connection
{
    public class PopupNoInternet : Popup
    {
        [SerializeField] private Button _btnClose;

        private Action _onCloseCallback;

        private void Awake()
        {
            _btnClose.onClick.AddListener(OnClickClose);
        }

        public override void OnAfterPopPopup()
        {
            base.OnAfterPopPopup();
            _onCloseCallback = null;
        }

        private void OnClickClose()
        {
            if (_popupContainer.CurrentPopup != this)
                return;

            _onCloseCallback?.Invoke();
            _popupContainer.Pop();
        }

        public void SetCallback(Action onCloseCallback)
        {
            _onCloseCallback = onCloseCallback;
        }
    }
}