using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using ThetanSDK.Utilities;
using UnityEngine;
using UnityEngine.UI;
using Wolffun.Log;
using Wolffun.RestAPI.ThetanWorld;
using Wolffun.StorageResource;

namespace ThetanSDK.UI
{
    public class NFTHeroImg : MonoBehaviour
    {
        [SerializeField] private Image _imgIconHero;
        [SerializeField] private Image _imgPlaceHolder;
        [SerializeField] private Image _imgRarityBackground;
        [SerializeField] private NFTHeroRarityBackgroundConfig _rarityBackgroundConfig;
        [SerializeField] private ThetanSDKUtilities.CustomSize _customSize;
    
        private uint _loadImgId = 0;
        private string _curImgUrl;

        private SpriteCacheManager _spriteCacheManager;

        private void OnEnable()
        {
            StorageResource.RegisterOnClearCacheCallback(HandleOnReleaseResource);
        }

        private void OnDisable()
        {
            StorageResource.UnRegisterOnClearCacheCallback(HandleOnReleaseResource);
        }

        private void HandleOnReleaseResource(string[] releasedUrls)
        {
            foreach (var url in releasedUrls)
            {
                if (url.Equals(_curImgUrl))
                {
                    LoadImgHero(_curImgUrl);
                    return;
                }
            }
        }

        public void SetSpriteCacheManager(SpriteCacheManager spriteCacheManager)
        {
            _spriteCacheManager = spriteCacheManager;
        }
    
        public void ShowUI(NftIngameInfo ingameInfo)
        {
            if (_imgRarityBackground)
                _imgRarityBackground.sprite = _rarityBackgroundConfig.GetHeroRarityBackground(ingameInfo.gameId, ingameInfo.rarity);
        
            var imgUrl = ThetanSDKUtilities.GetUrlPathHeroFullAvatar(ingameInfo.gameId, ingameInfo.type, _customSize);
            LoadImgHero(imgUrl);
        }

        private async void LoadImgHero(string imgUrl)
        {
            _imgPlaceHolder.SetAlphaImg(1);
            _imgIconHero.SetAlphaImg(0);
            
            _loadImgId = _loadImgId + 1;

            var curLoadImgId = _loadImgId;

            var imgTexture = await StorageResource.LoadImg(imgUrl);

            if (curLoadImgId != _loadImgId) // Prevent race-condition when multiple load
                return;

            _curImgUrl = imgUrl;
            
            if (imgTexture == null)
            {
                CommonLog.LogError($"Cannot load img {imgUrl}");
                return;
            }

            imgTexture.ignoreMipmapLimit = true;
            
            if (_spriteCacheManager != null)
            {
                var heroSpr = _spriteCacheManager.CreateAndCacheSprite(imgUrl, imgTexture);
                _imgIconHero.sprite = heroSpr;
            }
            else
            {
                var heroSpr = ThetanSDKUtilities.CreateSpriteFromTexture2D(imgTexture);
                _imgIconHero.sprite = heroSpr;
            }
            
            _imgPlaceHolder.SetAlphaImg(0);
            _imgIconHero.SetAlphaImg(1);
        }
    }
}

