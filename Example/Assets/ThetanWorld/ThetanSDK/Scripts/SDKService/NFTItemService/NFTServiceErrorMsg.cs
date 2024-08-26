namespace ThetanSDK.SDKServices.NFTItem
{
    public class NFTServiceErrorMsg
    {
        public static string ERROR_PREPARE_MATCH_NFT_GRINDING_IN_ANOTHER_GAME =
            "The selected NFT is in another game. Choose another or continue without rewards.";

        public static string ERROR_PREPARE_MATCH_OTHER_NFT_IS_GRINDING =
            "Another NFT is in this game. Wait for the current match to end or continue without rewards.";

        public static string ERROR_PREPARE_MATCH_NFT_DAILY_LIMIT =
            "The selected NFT reached its daily limit. Choose another or continue without rewards.";
        
        public static string ERROR_PREPARE_MATCH_NFT_MAX_GRIND_STAGE =
            "The selected NFT reached its total limit. Choose another or continue without rewards.";
        
        public static string ERROR_PREPARE_MATCH_NOT_SELECT_NFT =
            "No NFT selected. Did you forget to select one to start the match?";
        
        public static string ERROR_PREPARE_MATCH_SERVER_MAINTAIN =
            "Server is under maintenance. Continuing means no rewards for your NFT in the next match.";
        
        public static string ERROR_PREPARE_MATCH_UNKNOWN_ERROR =
            "Cannot prepare NFT for the match. Continue without rewards?\nErrorCode: {0}.\nErrorMessage: {1}";

        public static string ERROR_PREPARE_MATCH_NFT_NOT_MINT =
            "Your NFT isn't minted yet. Continue without rewards?";

        public static string ERROR_PREPARE_MATCH_NFT_NOT_OWNED =
            "You don't own this NFT. Continue without rewards?";
        
        public static string ERROR_PREPARE_MATCH_FREE_NFT_IN_REST_SESSION =
            "The selected NFT is resting. Choose another or continue without rewards.";
    }
}