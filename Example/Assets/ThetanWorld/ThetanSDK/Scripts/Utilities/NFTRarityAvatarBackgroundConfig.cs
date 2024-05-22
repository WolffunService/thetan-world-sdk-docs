using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Wolffun.RestAPI.ThetanWorld;

namespace ThetanSDK.Utilities
{
    public class NFTRarityAvatarBackgroundConfig : ScriptableObject
    {
        [Serializable]
        private struct ImgConfig
        {
            public NFTRarity rarity;
            public Sprite sprite;
        }

        [SerializeField] private List<ImgConfig> _listConfig;

        public Sprite GetSpriteForRarity(NFTRarity rarirty)
        {
            if (_listConfig == null)
                return default;
            
            foreach(var config in _listConfig)
                if (config.rarity == rarirty)
                    return config.sprite;

            return default;
        }
    }
}
