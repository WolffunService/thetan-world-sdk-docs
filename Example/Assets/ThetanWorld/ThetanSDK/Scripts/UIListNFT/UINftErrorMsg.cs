namespace ThetanSDK.UI
{
    public static class UINftErrorMsg
    {
        public static string ERROR_NO_CONNECTION = "No Internet connection.\nPlease check your network.";
        
        public static string ERROR_FETCH_LIST_NFT_HERO = "Error has occured while getting your items, please refresh";

        public static string ERROR_SELECT_HERO_ANOTHER_IS_GRINDING =
            "Another nft is grinding, please wait for finish grinding before select other nft";

        public static string ERROR_SELECT_HERO_UNKNOWN_ERROR = "Cannot select nft item.\nErrorCode: {0}.\nMessage: {1}";

        public static string ERROR_DESELECT_HERO_ANOTHER_IS_GRINDING =
            "An nft is grinding, please for for finish grinding before deselect nft";
        
        public static string ERROR_DESELECT_HERO_UNKNOWN_ERROR = "Cannot deselect nft item.\nErrorCode: {0}.\nMessage: {1}";

        public static string ERROR_SELECT_HERO_NFT_HERO_IS_GRINDING_IN_ANOTHER_GAME =
            "NFT is grinding in another game, please select other NFT";
        
        public static string ERROR_SELECT_HERO_NFT_HERO_IS_GRINDING_IN_ANOTHER_GAME_NAME =
            "NFT is grinding in {0}, please select other NFT";

        public static string ERROR_SELECT_HERO_NFT_IS_REACH_DAILY_LIMIT =
            "NFT is reach daily limit grind time, please select other NFT";

        public static string ERROR_SELECT_HERO_NFT_IS_NOT_MINTED =
            "NFT is not minted. Please mint your NFT first or contact our technical support if you need help.";

        public static string ERROR_SELECT_HERO_NFT_NOT_OWNED =
            "You are not the owner of this NFT";

        public static string WARNING_CONFIRM_FORCE_STOP_GRINDING =
            "This NFT is grinding in this game, do want to force stop grinding this NFT";
        
        public static string ERROR_SERVER_MAINTAIN = "Server is under maintenance, please comeback later";

        public static string WARNING_SELECT_HERO_MAX_GRIND_TIME =
            "This NFT reached daily grind time limit. Please select another or continue playing without grind rewards.";
        
        public static string WARNING_SELECT_HERO_MAX_LIFE_TIME =
            "This NFT reached its grind limit. Please select another or continue playing without grind rewards.";

        public static string WARNING_OTHER_NFT_IS_GRINDING_FORCE_STOP =
            "Other NFT is grinding in this game that prevent you from selecting other NFT.\nYou can force stop grinding to select your NFT";
    }
}