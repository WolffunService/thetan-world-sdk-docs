using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Text;
using UnityEngine;
using Wolffun.RestAPI.ThetanWorld;

namespace ThetanSDK.Utilities
{
    public class NFTHeroRarityBackgroundConfig : ScriptableObject
    {
        [SerializeField] private Sprite _defaultSprite;
        
        public Sprite GetHeroRarityBackground(GameWorldType gameWorldType, NFTRarity rarity)
        {
            var sprite =  Resources.Load<Sprite>(ZString.Format(
                "TexturesRarityBG/NFT_BG_{0}_{1}",
                gameWorldType.ToString(), rarity.ToString()));

            if (sprite != null)
                return sprite;

            return _defaultSprite;
        }
    }
}
