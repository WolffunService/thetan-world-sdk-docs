using System;
using System.Collections.Generic;
using Wolffun.RestAPI.ThetanWorld;

namespace ThetanSDK.SDKServices.NFTItem
{
    [Serializable]
    public struct FetchListNftItemResponse
    {
        public List<HeroNftItem> listNftHeroItems;
        public bool isNoMoreItem;
    }
}