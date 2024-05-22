using System.Collections;
using System.Collections.Generic;
using ThetanSDK.Utilities;
using UnityEngine;
using UnityEngine.UI;
using Wolffun.Log;
using Wolffun.RestAPI.ThetanWorld;
using Wolffun.StorageResource;

namespace ThetanSDK.UI
{
    public class GameIconImg : MonoBehaviour
    {
        [SerializeField] private Image _imgIcon;
        [SerializeField] private Image _imgPlaceHolder;

        private uint _loadImgId = 0;

        public async void ShowUI(GameWorldType gameWorldType)
        {
            if (_imgPlaceHolder)
                _imgPlaceHolder.enabled = true;
            _imgIcon.enabled = false;

            _loadImgId = _loadImgId + 1;

            var curLoadImgId = _loadImgId;

            var imgUrl = ThetanSDKUtilities.GetUrlPathGameIcon(gameWorldType);

            var imgTexture = await StorageResource.LoadImg(imgUrl);

            if (curLoadImgId != _loadImgId) // Prevent race-condition when multiple load
                return;

            if (imgTexture == null)
            {
                CommonLog.LogError($"Cannot load img {imgUrl}");
                return;
            }

            var spr = ThetanSDKUtilities.CreateSpriteFromTexture2D(imgTexture);

            if (_imgPlaceHolder)
                _imgPlaceHolder.enabled = false;
            _imgIcon.enabled = true;
            _imgIcon.sprite = spr;
        }
    }
}