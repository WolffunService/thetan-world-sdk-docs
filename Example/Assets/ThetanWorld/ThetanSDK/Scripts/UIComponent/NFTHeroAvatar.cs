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
    public class NFTHeroAvatar : MonoBehaviour
    {
        [SerializeField] private Image _imgAvatar;
        [SerializeField] private Image _imgPlaceHolder;
        [SerializeField] private Image _imgRarityBackground;
        [SerializeField] private NFTRarityAvatarBackgroundConfig _nftRarityConfig;
        
        private uint _loadImgId = 0;
        private string _curImgUrl;

        private void Awake()
        {
            StorageResource.RegisterOnClearCacheCallback(HandleOnClearCache);
        }

        private void OnDestroy()
        {
            StorageResource.UnRegisterOnClearCacheCallback(HandleOnClearCache);
        }

        private void HandleOnClearCache(string[] releasedUrls)
        {
            foreach (var url in releasedUrls)
            {
                if (url.Equals(_curImgUrl))
                {
                    ShowUI(_curImgUrl);
                    return;
                }
            }
        }

        public async void ShowUI(NftIngameInfo ingameInfo)
        {
            if (_imgRarityBackground)
            {
                _imgRarityBackground.sprite = _nftRarityConfig.GetSpriteForRarity(ingameInfo.rarity);
            }
            
            var imgUrl = ThetanSDKUtilities.GetUrlPathHeroAvatar(ingameInfo.gameId, ingameInfo.type);
            ShowUI(imgUrl);
        }

        private async void ShowUI(string imgUrl)
        {
            if(_imgPlaceHolder)
                _imgPlaceHolder.enabled = true;
            _imgAvatar.enabled = false;

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
#if UNITY_2021_3_OR_NEWER
            imgTexture.ignoreMipmapLimit = true;
#endif
            var heroSpr = ThetanSDKUtilities.CreateSpriteFromTexture2D(imgTexture);

            _imgAvatar.sprite = heroSpr;
            
            if(_imgPlaceHolder)
                _imgPlaceHolder.enabled = false;
            _imgAvatar.enabled = true;
        }

    }
}

