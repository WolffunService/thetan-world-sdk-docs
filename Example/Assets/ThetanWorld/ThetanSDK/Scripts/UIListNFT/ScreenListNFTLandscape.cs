using System;
using UnityEngine;
using Wolffun.RestAPI.ThetanWorld;

namespace ThetanSDK.UI
{
    public class ScreenListNFTLandscape : ScreenListNFT
    {
        [SerializeField] private PopUpDetailHeroNFT _prefabPopupDetailNFT;

        internal Action OnRequestCloseAllScreenCallback;
        
        protected override async void OnClickItem(HeroNftItem data)
        {
            PopUpDetailHeroNFT popup = await _uiHelperContainer.PushPopup(_prefabPopupDetailNFT, new PopupOption()
            {
                IsAllowBackdrop = true,
                IsBackdropCanClosePopup = true
            }) as PopUpDetailHeroNFT;
            
            popup.SetData(data, _uiHelperContainer, selectedHeroData =>
            {
                if (!string.IsNullOrEmpty(selectedHeroData.id))
                {
                    OnRequestCloseAllScreenCallback?.Invoke();
                }
            });
        }
    }
}