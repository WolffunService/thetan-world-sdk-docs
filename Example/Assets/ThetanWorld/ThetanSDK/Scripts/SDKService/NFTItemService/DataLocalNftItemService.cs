using System;

namespace ThetanSDK.SDKServices.NFTItem
{
    [Serializable]
    public struct DataLocalNftItemService
    {
        /// <summary>
        /// Check if we have checked to unselect selected NFT when free NFT is not claimed.
        /// Only perform check once per app client.
        /// </summary>
        public bool hasCheckUnselectNftWhenNotClaimFreeNft;
    }
}