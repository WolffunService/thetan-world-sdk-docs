using System;
using UnityEditor;
using UnityEngine;
using Wolffun.RestAPI.ThetanWorld;

namespace ThetanSDK.UI
{
    public class ScreenDetailHeroNFT : Screen
    {
        [SerializeField] private DetailHeroNFTUI _detailHeroNFTUi;

        private HeroNftItem _heroData;
        private Action<HeroNftItem> _onSelectCallback;

        public override void OnBeforePushScreen()
        {
            base.OnBeforePushScreen();
            ThetanSDKManager.Instance.NftItemService.RegisterOnChangeNftItemData(OnChangeNFTItemData);
        }

        public override void OnAfterPopScreen()
        {
            base.OnAfterPopScreen();

            _heroData = new HeroNftItem();
            _onSelectCallback = null;
            _detailHeroNFTUi.ClearCache();
            
            if(ThetanSDKManager.IsAlive && 
               ThetanSDKManager.Instance.NftItemService != null)
            {
                ThetanSDKManager.Instance.NftItemService.UnRegisterOnChangeNftItemData(OnChangeNFTItemData);
            }
        }
        
        private void OnChangeNFTItemData(HeroNftItem newHeroData)
        {
            if (_heroData.id != newHeroData.id)
                return;

            _heroData = newHeroData;
        }

        public void SetData(HeroNftItem data, UIHelperContainer uiHelperContainer, Action<HeroNftItem> onSelectCallback)
        {
            _onSelectCallback = onSelectCallback;
            _heroData = data;

            _detailHeroNFTUi.SetData(data, uiHelperContainer, OnSelectHeroNFTCallback);
        }

        private void OnSelectHeroNFTCallback(HeroNftItem selectedNFT)
        {
            _screenContainer.PopScreen();
            
            _onSelectCallback?.Invoke(selectedNFT);
        }

        [ContextMenu("AutoGetSizeContent")]
        private void AutoGetSizeContent()
        {
            
#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
#endif
        }
    }
}