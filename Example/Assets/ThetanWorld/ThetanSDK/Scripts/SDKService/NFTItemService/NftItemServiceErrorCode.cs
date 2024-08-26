namespace ThetanSDK.SDKServices.NFTItem
{
    /// <summary>
    /// Enum defying ErrorCode when interact with NFT ITEM API.
    /// Negative error is client defined error, server will not return those error
    /// </summary>
    public enum NftItemServiceErrorCode
    {
        FAIL_TO_VERIFY_APP = -8,
        
        /// <summary>
        /// This SDK version is not supported by server anymore, please update your SDK
        /// </summary>
        SDK_VERSION_NOT_SUPPORTED = -7, 
        
        /// <summary>
        /// Client is not in logged in state
        /// </summary>
        NOT_LOGGED_IN = -6,
        
        SELECTED_HERO_ID_NOT_THE_SAME = -5,
        NOT_SELECTED_NFT_HERO = -4,
        
        /// <summary>
        /// API Success, there is no error
        /// </summary>
        SUCCESS = -3,
        
        /// <summary>
        /// There is network error when making API request
        /// </summary>
        NETWORK_ERROR = -1,

        /// <summary>
        /// Free NFT is in rest section, wait for rest section end
        /// </summary>
        FREE_HERO_IN_REST_SESSION = -10,
        
        /// <summary>
        /// Unknown Error
        /// </summary>
        UNKNOWN = 99,
        
        /// <summary>
        /// NFT is not minted yet, please go to marketplace to mint NFT
        /// </summary>
        NFT_NOT_MINT = 8101,
        
        /// <summary>
        /// User not the owner of request NFT
        /// </summary>
        USER_NOT_OWN_NFT = 8102,
        
        /// <summary>
        /// Another NFT is grinding in this game that prevent API request
        /// </summary>
        ANOTHER_NFT_IS_GRINDING = 8216,
        
        /// <summary>
        /// Selected NFT is grinding in another game that prevent API request
        /// </summary>
        NFT_IS_GRINDING_IN_ANOTHER_GAME = 8215,
        
        /// <summary>
        /// Selected NFT is reached its daily grind limit
        /// </summary>
        NFT_DAILY_LIMIT_REACH = 8217,
        
        /// <summary>
        /// Selected hero is not grinding
        /// </summary>
        HERO_NOT_GRINDING = 8218,
        
        /// <summary>
        /// Selected hero is at maximum grind stage and cannot be grinded anymore
        /// </summary>
        HERO_MAX_GRIND_STAGE = 8219,
        
        /// <summary>
        /// Free NFT has been claimed before
        /// </summary>
        FREE_NFT_CLAIMED = 8226,
    }
}