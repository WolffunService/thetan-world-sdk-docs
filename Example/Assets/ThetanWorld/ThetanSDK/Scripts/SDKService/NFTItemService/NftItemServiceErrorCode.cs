namespace ThetanSDK.SDKServices.NFTItem
{
    // negative error is client defined error, server will not return those error
    public enum NftItemServiceErrorCode
    {
        SDK_VERSION_NOT_SUPPORTED = -7,
        NOT_LOGGED_IN = -6,
        SELECTED_HERO_ID_NOT_THE_SAME = -5,
        NOT_SELECTED_NFT_HERO = -4,
        SUCCESS = -3,
        NETWORK_ERROR = -1,
        UNKNOWN = 99,
        NFT_NOT_MINT = 8101,
        USER_NOT_OWN_NFT = 8102,
        ANOTHER_NFT_IS_GRINDING = 8216,
        NFT_IS_GRINDING_IN_ANOTHER_GAME = 8215,
        NFT_DAILY_LIMIT_REACH = 8217,
        HERO_NOT_GRINDING = 8218,
        HERO_MAX_GRIND_STAGE = 8219,
    }
}