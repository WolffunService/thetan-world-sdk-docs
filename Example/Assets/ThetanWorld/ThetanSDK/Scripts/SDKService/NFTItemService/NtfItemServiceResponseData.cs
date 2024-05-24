using System;
using System.Collections.Generic;
using Wolffun.RestAPI.ThetanWorld;

namespace ThetanSDK.SDKServices.NFTItem
{
    [Serializable]
    public struct FetchListNftItemResponse
    {
        /// <summary>
        /// List Hero NFT Item of this response
        /// </summary>
        public List<HeroNftItem> listNftHeroItems;
        
        /// <summary>
        /// Is there anymore unfetched NFT
        /// </summary>
        public bool isNoMoreItem;
    }
}