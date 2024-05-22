using System;
using UnityEngine;

namespace ThetanSDK.UI
{
    public class FloatingWindowMainUI : FloatingWindow
    {
        [SerializeField] private ScreenContainer _screenContainer;
        [SerializeField] private ScreenListNFT _prefabScreenListNFT;
        [SerializeField] private UIHelperContainer _uiHelperContainer;

        public override async void OnBeforeShow()
        {
            base.OnBeforeShow();
            
            ScreenListNFT instanceScreenListNFT = await _screenContainer.PushScreen(_prefabScreenListNFT) as ScreenListNFT;
            
            instanceScreenListNFT.Initialize(_uiHelperContainer);
        }

        public override void OnAfterClose()
        {
            _screenContainer.PopAllScreen();
            base.OnAfterClose();
        }
    }
}