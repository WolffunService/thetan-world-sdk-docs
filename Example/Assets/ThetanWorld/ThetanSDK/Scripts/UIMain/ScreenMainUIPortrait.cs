using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Triggers;
using ThetanSDK.UI;
using ThetanSDK.UI.LuckySpin;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Wolffun.RestAPI.ThetanWorld;

namespace ThetanSDK.UI
{
    public class ScreenMainUIPortrait : ScreenMainUI
    {
        [Header("Portrait Custom Field")]
        [SerializeField] private ScreenListNFTPortrait _prefabScreenListNFT;
        [SerializeField] private ScreenContainer _contentScreenContainer;
        [SerializeField] private ScreenDetailHeroNFT prefabScreenDetailHeroNft;
        
        protected override void Start()
        {
            base.Start();
            
            _contentScreenContainer.EnableButtonCloseScreen();
            _contentScreenContainer.RegisterOnClickCloseScreen(HandleRequestCloseScreen);
        }

        private void HandleRequestCloseScreen()
        {
            _screenContainer.NotifyOnClickCloseScreen();
        }

        protected override async void ShowDetailNFT(HeroNftItem nftData)
        {
            var instance = await _contentScreenContainer.PushScreen(prefabScreenDetailHeroNft) as ScreenDetailHeroNFT;
            
            instance.SetData(nftData, _uiHelperContainer, selectedHeroData =>
            {
                if(!selectedHeroData.IsEmpty())
                    _screenContainer.NotifyOnClickCloseScreen();
            });
        }

        public override void OnAfterPopScreen()
        {
            _contentScreenContainer.PopAllScreen();
            base.OnAfterPopScreen();
        }

        public override async void ShowScreenListNFT()
        {
            ScreenListNFTPortrait screenListNft =
                await _contentScreenContainer.PushScreen(_prefabScreenListNFT) as ScreenListNFTPortrait;
            
            if(screenListNft)
                screenListNft.Initialize(_uiHelperContainer);
        }
    }
}
