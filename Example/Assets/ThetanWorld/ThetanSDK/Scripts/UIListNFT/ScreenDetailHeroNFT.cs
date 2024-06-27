using System;
using Cysharp.Threading.Tasks;
using ThetanSDK.SDKServices.NFTItem;
using ThetanSDK.Utilities;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Wolffun.Log;
using Wolffun.RestAPI;
using Wolffun.RestAPI.ThetanWorld;
using Wolffun.StorageResource;

namespace ThetanSDK.UI
{
    public class ScreenDetailHeroNFT : Screen
    {
        [SerializeField] private DetailHeroNFTUI _detailHeroNFTUi;
        [SerializeField] private ScrollRect _scrollContent;

        [SerializeField] private float _contentScrollRectDefaultSize;

        private HeroNftItem _heroData;
        private Action<HeroNftItem> _onSelectCallback;
        private UIHelperContainer _uiHelperContainer;

        public override void OnBeforePushScreen()
        {
            base.OnBeforePushScreen();
            
            if(_scrollContent)
                _scrollContent.content.anchoredPosition = Vector2.zero;
            
            ThetanSDKManager.Instance.NftItemService.RegisterOnChangeNftItemData(OnChangeNFTItemData);
        }

        public override void OnAfterPopScreen()
        {
            base.OnAfterPopScreen();

            _heroData = new HeroNftItem();
            _onSelectCallback = null;
            _uiHelperContainer = null;
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

            SetContentScrollRectSize(newHeroData);
        }

        private void SetContentScrollRectSize(HeroNftItem heroData)
        {
            if (heroData.grindInfo.IsMaxLifeTime())
            {
                _scrollContent.content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 
                    _contentScrollRectDefaultSize + 
                    _detailHeroNFTUi.MaxLifeTimeContentBonusSize + _detailHeroNFTUi.BonusContentPaddingSize);
            }
            else if (heroData.grindInfo.grindTime >= heroData.grindInfo.maxGrindTime)
            {
                _scrollContent.content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 
                    _contentScrollRectDefaultSize + 
                    _detailHeroNFTUi.MaxGrindTimeContentBonusSize + _detailHeroNFTUi.BonusContentPaddingSize);
            }
            else
            {
                _scrollContent.content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 
                    _contentScrollRectDefaultSize);
            }
        }

        public void SetData(HeroNftItem data, UIHelperContainer uiHelperContainer, Action<HeroNftItem> onSelectCallback)
        {
            _onSelectCallback = onSelectCallback;
            _uiHelperContainer = uiHelperContainer;
            _heroData = data;

            _detailHeroNFTUi.SetData(data, uiHelperContainer, OnSelectHeroNFTCallback);
            SetContentScrollRectSize(data);
        }

        private void OnSelectHeroNFTCallback(HeroNftItem selectedNFT)
        {
            if(selectedNFT.IsEmpty())
                _screenContainer.PopScreen();
            
            _onSelectCallback?.Invoke(selectedNFT);
        }

        [ContextMenu("AutoGetSizeContent")]
        private void AutoGetSizeContent()
        {
            _contentScrollRectDefaultSize = _scrollContent.content.sizeDelta.y;
            
#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
#endif
        }
    }
}