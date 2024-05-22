using System;
using UnityEngine;
using UnityEngine.UI;

namespace ThetanSDK.UI
{
    public class PopupBackdrop : MonoBehaviour
    {
        [SerializeField] private Button _btnBgClose;

        private Popup _popup;
        private PopupContainer _popupContainer;
        private bool _isAllowClosePopup;

        private void Start()
        {
            _btnBgClose.onClick.AddListener(OnClickClose);
        }

        private void OnClickClose()
        {
            if (_popupContainer == null || _popup == null || !_isAllowClosePopup)
                return;
            
            if(_popupContainer.CurrentPopup == _popup)
                _popupContainer.Pop();
        }

        public void SetPopup(PopupContainer popupContainer, Popup popup, bool isAllowClosePopup)
        {
            _popup = popup;
            _popupContainer = popupContainer;
            _isAllowClosePopup = isAllowClosePopup;
        }

        public void ClearData()
        {
            _popup = null;
            _popupContainer = null;
            _isAllowClosePopup = false;
        }

    }
}