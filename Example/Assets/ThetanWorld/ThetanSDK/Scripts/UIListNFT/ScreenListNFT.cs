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
       
        private string _selectedHeroId;

        private SpriteCacheManager _spriteCacheManager;
        
        private void Awake()
        {
            _listNftAdapter.RegisterOnSelectItem(OnSelectItem);
            _spriteCacheManager = new SpriteCacheManager();
        }

        public override void OnBeforePushScreen()
        {
            base.OnBeforePushScreen();
            
            _listNftAdapter.SetSpriteCacheManager(_spriteCacheManager);
            
            if(ThetanSDKManager.Instance.NftItemService.ListHeroNftItems == null ||
                ThetanSDKManager.Instance.NftItemService.ListHeroNftItems.Count == 0)
            {
                _listNftAdapter.RefetchNewData();
            }
            else
            {
                _listNftAdapter.InitializeWithCacheData();
            }
            
            ThetanSDKManager.Instance.NftItemService.RegisterOnChangeNftItemData(HandleOnChangeNftItemData);
            
            ThetanSDKManager.Instance.NftItemService.RegisterOnChangeSelectedNftHeroCallback(HandleOnChangeSelectedNftItem);
        }

        public override void OnAfterPopScreen()
        {
            base.OnAfterPopScreen();
            
            _listNftAdapter.ClearData();
            
            if(ThetanSDKManager.IsAlive)
            {
                ThetanSDKManager.Instance.NftItemService.UnRegisterOnChangeNftItemData(HandleOnChangeNftItemData);

                ThetanSDKManager.Instance.NftItemService.UnRegisterOnChangeSelectedNftHeroCallback(
                    HandleOnChangeSelectedNftItem);
            }
        }

        private void OnDestroy()
        {
            _listNftAdapter.UnregisterOnSelectItem(OnSelectItem);
        }

        private void HandleOnChangeNftItemData(HeroNftItem newData)
        {
            _listNftAdapter.ChangeHeroNftData(newData);
        }

        protected abstract void OnSelectItem(HeroNftItem data);

        private void HandleOnChangeSelectedNftItem(string selectedItemId)
        {
            _selectedHeroId = selectedItemId;
            _listNftAdapter.UpdateSelectedItemId();
        }
    }
}
