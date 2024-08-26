namespace Wolffun.RestAPI
{
    public enum WSErrorCode
    {
        //in game error code
        UnityHttpRequestNetworkError = -1,
        CantParseError = -2,
        None = -3,
        LimitDailyBattleRentedHero = -4,
        AlreadyResolveFromBase = -5,

        // api result invalid
        ApiResultInvalid = -6,
        ServerStatus500 = -7,

        // Special Event
        Claimed = -8,
        Unlocked = -9,

        //common code 1 - 1000
        Error = 0,
        TokenInvalid = 1,
        TokenExpired = 2,
        UserNotFound = 3,
        UserBanned = 4,
        WrongFormat = 5,
        WrongPrice = 6,
        OutOfLimit = 7,
        CannotClaim = 8,
        NewVersionAvailable = 9,

        GameMaintainServer = 18,
        
        DoNotHavePermission = 403,
        
        ServerMaintenance = 503,

        //wolffunId ERROR_CODE 2001 -> 3000:
        EmailNotExist = 2001,
        EmailIsExist = 2002,
        InvalidCode = 2003,
        InvalidEmail = 2004,
        InCorrectUserId = 2005,
        NotEnoughTicketChangeName = 2006,
        UserNameHasTaken = 2007,
        TooManyAccountLogin = 2008,
        AccountHasBeenLinked = 2009,

        //theta data code 3001 -> 4000
        IdInvalid = 3001,
        HeroNotAvailable = 3011,
        AddressInvalid = 3002,
        TypeInvalid = 3003,
        HeroIdInvalid = 3004,
        Invalidate = 3005,
        NotEnough = 3006,
        NotEnoughTHC = 3007,
        NotEnoughTHG = 3008,
        NotEnoughPP = 3009,
        NotOwnerHero = 3010,

        // Upgrade
        InvalidGTHG = 3017,

        //box
        OutdatedBuying = 3100,
        OutOfBoxes = 3101,
        NotEnoughBox = 3102,

        //friend
        OtherFullRequestReceived = 3103,
        OtherFullFriendList = 3104,
        FullFriendList = 3105,

        // Report player
        ReachLimitReportInWeek = 3107,
        PreviouslyReported = 3108,

        // Rented Hero
        ForRent = 3124,
        MaximumBattleRented = 3125,
        ReturningToOwner = 3127,

        //vesting safe
        MaintenanceVestingSafe = 3140,
        VestingSafeAlreadyClaimed = 3141,

        //lobby guild
        GuildNotInLeaderboard = 4002,

        //shop check
        ErrCosmeticProfileAlreadyExist = 4200,

        //CreatorProgram
        CantFetchYoutubeVideo = 5103,

        //DailyQuest
        DailyQuestFeatureIsNowLock = 4103,
        
        //Special Event
        PersonalEventClaimed = 4701,
        SpecialEventClaimed = 4702,
        
        //SeasonPass
        DontHaveEnoughSSP = 6100,
        AlreadyBoughtPremiumPass = 6101,

        NotEnoughRBuck = 6200,
    }
}