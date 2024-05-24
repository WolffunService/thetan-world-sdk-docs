using Wolffun.RestAPI;

namespace ThetanSDK.SDKService.RemoteConfig
{
    /// <summary>
    /// Thetan World remote config
    /// </summary>
    public struct ThetanWorldRemoteConfigModel : ICustomDefaultable<ThetanWorldRemoteConfigModel>
    {
        /// <summary>
        /// Config for marketplace URL
        /// </summary>
        public MarkePlaceURLConfig mkpUrlConfig;
        
        /// <summary>
        /// If this is true, redirect all endpoint to staging when initialize sdk
        /// </summary>
        public bool isUseTemporaryVersion;
        public ThetanWorldRemoteConfigModel SetDefault()
        {
            return this;
        }

        public bool IsEmpty()
        {
            return false;
        }
    }

    /// <summary>
    /// Contain URL config for all button that need to redirect to marketplace
    /// </summary>
    public struct MarkePlaceURLConfig
    {
        public string urlTabGrind;
        public string urlTabBuyNFT;
        public string urlTabSpin;
        public bool enableBtnBuyNFT;
        public bool enableBtnMore;
        public bool enableBtnSpin;
    }
}