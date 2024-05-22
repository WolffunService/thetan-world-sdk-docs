using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using ThetanSDK.Utilities;
using UnityEngine;
using UnityEngine.UI;
using Wolffun.StorageResource;

namespace ThetanSDK.UI
{
    public class UserProfileAvatar : MonoBehaviour
    {
        [SerializeField] private Image _imgAvatar;
        [SerializeField] private Image _imgAvatarFrame;
        [SerializeField] private Image _imgPlaceHolder;

        private SpriteCacheManager _spriteCacheManager;

        private int _curLoadId = 0;
        private string _curAvatarUrl;
        private string _curAvatarFrameUrl;

        public void Awake()
        {
            ShowPlaceHolder();
            
            StorageResource.RegisterOnClearCacheCallback(HandleOnClearCache);
        }

        private void HandleOnClearCache(string[] releasedUrls)
        {
            foreach (var url in releasedUrls)
            {
                if (url.Equals(_curAvatarUrl) || url.Equals(_curAvatarFrameUrl))
                {
                    LoadImg(_curAvatarUrl, _curAvatarFrameUrl);
                    return;
                }
            }
        }

        private void OnDestroy()
        {
            StorageResource.UnRegisterOnClearCacheCallback(HandleOnClearCache);
        }
        
        public void SetSpriteCacheManager(SpriteCacheManager spriteCacheManager)
        {
            _spriteCacheManager = spriteCacheManager;
        }

        public async void SetUI(int avatarId, int avatarFrameId)
        {
            var avatarPath = ThetanSDKUtilities.GetUrlImageProfileCosmetic(avatarId);
            var framePath = ThetanSDKUtilities.GetUrlImageProfileCosmetic(avatarFrameId);

            LoadImg(avatarPath, framePath);
        }

        private async void LoadImg(string avatarPath, string framePath)
        {
            ShowPlaceHolder();

            var loadId = _curLoadId + 1;
            _curLoadId = loadId;
            
            var loadAvatarTask = StorageResource.LoadImg(avatarPath);
            var loadFrameTask = StorageResource.LoadImg(framePath);

            (var avatarTexture, var frameTexture) = await (loadAvatarTask, loadFrameTask);

            if (loadId != _curLoadId)
                return;

            _curAvatarUrl = avatarPath;
            _curAvatarFrameUrl = framePath;
            
            if (_spriteCacheManager != null)
                _imgAvatar.sprite = _spriteCacheManager.CreateAndCacheSprite(avatarPath, avatarTexture);
            else
                _imgAvatar.sprite = ThetanSDKUtilities.CreateSpriteFromTexture2D(avatarTexture);

            if (_spriteCacheManager != null)
                _imgAvatarFrame.sprite = _spriteCacheManager.CreateAndCacheSprite(framePath, frameTexture);
            else
                _imgAvatarFrame.sprite = ThetanSDKUtilities.CreateSpriteFromTexture2D(frameTexture);

            _imgAvatar.SetAlphaImg(1);
            _imgAvatarFrame.SetAlphaImg(1);
            _imgPlaceHolder.SetAlphaImg(0);
        }

        private void ShowPlaceHolder()
        {
            _imgAvatar.SetAlphaImg(0);
            _imgAvatarFrame.SetAlphaImg(0);
            _imgPlaceHolder.SetAlphaImg(1);
        }
    }
}