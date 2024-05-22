using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Wolffun.RestAPI.ThetanWorld;

namespace ThetanSDK.Utilities
{
    [CreateAssetMenu(fileName = "NFTRarityColorConfig", menuName = "Wolffun/Configs/NFTRarityColorConfig")]
    internal class NFTRarityColorConfig : ScriptableObject
    {
        [Serializable]
        private struct Config
        {
            public NFTRarity rarity;
            public Color color;
        }

        [SerializeField] private Config[] _configs;

        public Color GetColor(NFTRarity rarity)
        {
            foreach (var config in _configs)
            {
                if (config.rarity == rarity)
                    return config.color;
            }

            return Color.white;
        }
    }
}
