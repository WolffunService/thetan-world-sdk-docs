using Wolffun.RestAPI;

namespace ThetanSDK.SDKService.RemoteConfig
{
    public struct ThetanWorldRemoteConfigModel : ICustomDefaultable<ThetanWorldRemoteConfigModel>
    {
        public MarkePlaceURLConfig mkpUrlConfig;
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