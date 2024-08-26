using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using ThetanSDK.Utilities;
using TMPro;
using UnityEngine;
using Wolffun.RestAPI.ThetanWorld;
using Screen = ThetanSDK.UI.Screen;

namespace ThetanSDK.UI
{
    public abstract class ScreenListNFT : ScreenContentMainUI
    {
        [SerializeField] private ListNftAdapter _listNftAdapter;
        [SerializeField] private TextMeshProUGUI _txtListNftCount;
       
        private string _selectedHeroId;

        private SpriteCacheManager _spriteCacheManager;
        
        private void Awake()
        {
            _listNftAdapter.RegisterOnClickItem(OnClickItem);
            _listNftAdapter.RegisterOnSelectItem(OnSelectItem);
            _spriteCacheManager = new SpriteCacheManager();
        }

        public override void OnBeforePushScreen()
        {
            base.OnBeforePushScreen();
            
            _listNftAdapter.SetSpriteCacheManager(_spriteCacheManager);

            var nftService = ThetanSDKManager.Instance.NftItemService;
            if(nftService.ListHeroNftItems == null ||
               nftService.ListHeroNftItems.Count == 0)
            {
                _listNftAdapter.RefetchNewData();
            }
            else
            {
                _listNftAdapter.InitializeWithCacheData();
                
            }
            
            _txtListNftCount.SetText("NFT List<size=122%><color=#8C90A8> ({0})</color></size>", nftService.CountTotalNFT);
            nftService._onListNFTFetchSuccessCallback += HandleOnListNFTFetchSuccess;
            
            ThetanSDKManager.Instance.NftItemService.RegisterOnChangeNftItemData(HandleOnChangeNftItemData);
            
            ThetanSDKManager.Instance.NftItemService.RegisterOnChangeSelectedNftHeroCallback(HandleOnChangeSelectedNftItem);
        }
        
        public override void OnAfterPopScreen()
        {
            base.OnAfterPopScreen();
            
            _listNftAdapter.ClearData();
            
            if(ThetanSDKManager.IsAlive && ThetanSDKManager.Instance.NftItemService)
            {
                var nftService = ThetanSDKManager.Instance.NftItemService;
                nftService.UnRegisterOnChangeNftItemData(HandleOnChangeNftItemData);
                nftService.UnRegisterOnChangeSelectedNftHeroCallback(HandleOnChangeSelectedNftItem);
                nftService._onListNFTFetchSuccessCallback -= HandleOnListNFTFetchSuccess;
            }
        }

        private void OnDestroy()
        {
            _listNftAdapter.UnregisterOnClickItem(OnClickItem);
            _listNftAdapter.UnregisterOnSelectItem(OnSelectItem);
        }
        
        private void HandleOnListNFTFetchSuccess()
        {
            if(_txtListNftCount)
                _txtListNftCount.SetText("NFT List<size=122%><color=#8C90A8> ({0})</color></size>", ThetanSDKManager.Instance.NftItemService.CountTotalNFT);
        }
        
        private void HandleOnChangeNftItemData(HeroNftItem newData)
        {
            _listNftAdapter.ChangeHeroNftData(newData);
        }

        private void OnSelectItem(HeroNftItem item)
        {
            _uiHelperContainer.TurnOnLoading();
            var nftService = ThetanSDKManager.Instance.NftItemService;
            nftService.SelectHeroNft(item, _ =>
            {
                _uiHelperContainer.TurnOffLoading();
                _uiHelperContainer.ShowToast("Select success");
            }, error =>
            {
                _uiHelperContainer.TurnOffLoading();
                ThetanSDKUtilities.HandleSelectNFTError(error, _uiHelperContainer);
            });
        }
        
        protected abstract void OnClickItem(HeroNftItem data);

        private void HandleOnChangeSelectedNftItem(string selectedItemId)
        {
            _selectedHeroId = selectedItemId;
            _listNftAdapter.UpdateSelectedItemId();
        }
    }
}
