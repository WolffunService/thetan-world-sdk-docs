using System;
using System.Collections.Generic;
using UnityEngine;
using Wolffun.StorageResource;
using Object = UnityEngine.Object;

namespace ThetanSDK.Utilities
{
    public class SpriteCacheManager : IDisposable
    {
        private Dictionary<string, Sprite> _cachedSprites;

        public SpriteCacheManager()
        {
            _cachedSprites = new Dictionary<string, Sprite>();
            
            StorageResource.RegisterOnClearCacheCallback(HandleOnClearCache);
        }

        private void HandleOnClearCache(string[] clearedUrls)
        {
            foreach (var url in clearedUrls)
            {
                if (_cachedSprites.TryGetValue(url, out var cachedSprite))
                {
                    Object.Destroy(cachedSprite);
                    _cachedSprites.Remove(url);
                }
            }
        }

        public Sprite CreateAndCacheSprite(string key, Texture2D texture2D)
        {
            if (string.IsNullOrEmpty(key) ||
                texture2D == null)
                return null;
            
            if(_cachedSprites == null)
                _cachedSprites = new Dictionary<string, Sprite>();

            if (_cachedSprites.TryGetValue(key, out var cachedSprite))
            {
                if (cachedSprite.texture)
                    return cachedSprite;
                else
                {
                    Object.Destroy(cachedSprite);
                    _cachedSprites.Remove(key);
                }
            }

            var newSprite = ThetanSDKUtilities.CreateSpriteFromTexture2D(texture2D);

            _cachedSprites.TryAdd(key, newSprite);

            return newSprite;
        }
        
        public void ClearCache()
        {
            if (_cachedSprites == null)
                return;
            
            foreach (var cached in _cachedSprites)
            {
                Object.Destroy(cached.Value);
            }
            
            _cachedSprites.Clear();
        }
        
        public void Dispose()
        {
            if (_cachedSprites == null)
                return;

            foreach (var cached in _cachedSprites)
            {
                Object.Destroy(cached.Value);
            }
            StorageResource.UnRegisterOnClearCacheCallback(HandleOnClearCache);
            _cachedSprites.Clear();
            _cachedSprites = null;
        }
    }
}