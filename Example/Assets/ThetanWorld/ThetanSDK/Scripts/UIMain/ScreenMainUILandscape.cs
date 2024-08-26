using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using ThetanSDK.Scripts.UITab;
using ThetanSDK.SDKService.UserStatisticService;
using ThetanSDK.Utilities;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Wolffun.RestAPI.ThetanWorld;

namespace ThetanSDK.UI
{
    public class ScreenMainUILandscape : ScreenMainUI
    {
        [Header("Landscape")]
        [SerializeField] private PopupScreenListNFTWrapper _prefabPopupScreenListNft;
        [SerializeField] private PopUpDetailHeroNFT _prefabPopupDetailNFT;
        public override async void ShowScreenListNFT()
        {
            var screenListNft = await _uiHelperContainer.PushPopup(_prefabPopupScreenListNft, new PopupOption()
            {
                IsAllowBackdrop = true,
                IsBackdropCanClosePopup = true
            }) as PopupScreenListNFTWrapper;
            screenListNft.Initialize(_uiHelperContainer);
            screenListNft.ScreenListNft.OnRequestCloseAllScreenCallback = () =>
            {
                _screenContainer.NotifyOnClickCloseScreen();
            };
        }

        protected override async void ShowDetailNFT(HeroNftItem nftData)
        {
            PopUpDetailHeroNFT popup = await _uiHelperContainer.PushPopup(_prefabPopupDetailNFT, new PopupOption()
            {
                IsAllowBackdrop = true,
                IsBackdropCanClosePopup = true
            }) as PopUpDetailHeroNFT;
            
            popup.SetData(nftData, _uiHelperContainer, selectedHeroData =>
            {
                if (!string.IsNullOrEmpty(selectedHeroData.id))
                {
                    _screenContainer.NotifyOnClickCloseScreen();
                }
            });
        }
    }
}