namespace ThetanSDK.SDKServices.NFTItem
{
    public class NFTServiceErrorMsg
    {
        public static string ERROR_PREPARE_MATCH_NFT_GRINDING_IN_ANOTHER_GAME =
            "The chosen NFT is grinding in another game. Please select another or continue playing without grind rewards.";

        public static string ERROR_PREPARE_MATCH_OTHER_NFT_IS_GRINDING =
            "Another NFT is grinding in this game. Please wait for previous match end before start another match or continue playing without grind rewards.";

        public static string ERROR_PREPARE_MATCH_NFT_DAILY_LIMIT =
            "The chosen NFT has hit its daily grind limit. Please select another or continue playing without grind rewards.";
        
        public static string ERROR_PREPARE_MATCH_NFT_MAX_GRIND_STAGE =
            "The chosen NFT has hit its grind limit. Please select another or continue playing without grind rewards.";
        
        public static string ERROR_PREPARE_MATCH_NOT_SELECT_NFT =
            "You are not select any NFT, are you forget to select NFT to start match?";
        
        public static string ERROR_PREPARE_MATCH_SERVER_MAINTAIN =
            "Server is under maintenance. If you continue, your NFT will not be grind in next match.";
        
        public static string ERROR_PREPARE_MATCH_UNKNOWN_ERROR =
            "Cannot prepare NFT for the match. Do you want to continue playing without grind rewards.\nErrorCode: {0}.\nErrorMessage: {1}";

        public static string ERROR_PREPARE_MATCH_NFT_NOT_MINT =
            "Your NFT is not minted yet. Do you want to continue playing without grind rewards.";

        public static string ERROR_PREPARE_MATCH_NFT_NOT_OWNED =
            "You are not the owner of the NFT. Do you want to continue playing without grind rewards.";
    }
}