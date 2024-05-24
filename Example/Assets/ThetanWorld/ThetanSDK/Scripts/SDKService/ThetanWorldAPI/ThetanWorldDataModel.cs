using System;
using System.Collections.Generic;
using ThetanSDK.UI.LuckySpin;

namespace Wolffun.RestAPI.ThetanWorld
{
    /// <summary>
    /// Request used when fetch list hero nft
    /// </summary>
    [Serializable]
    public struct RequestNFTListRequestModel
    {
        /// <summary>
        /// Number items in page
        /// </summary>
        public int limit;
        
        /// <summary>
        /// Page index to fetch data (start at 0)
        /// </summary>
        public int page;
        
        /// <summary>
        /// Defying sort, see NFTSortType for all sort criteria.
        /// Format Example: 1,-3,-5,-12
        /// </summary>
        public string sorts;
        
        /// <summary>
        /// True to only fetch NFT that has been mounted.
        /// False to ignore mounted status of NFT
        /// </summary>
        public bool mountStatus;
    }

    /// <summary>
    /// Enum defying all sort criteria used when fetch list hero nft
    /// </summary>
    [Serializable]
    public enum NFTSortType
    {
        MountStatus = 1,
        Listing = 2,
        Rarity = -3,
        GrindStage = 4,
        GrindReward = -5, // Số THG nhận được từ grind của nft này cho 1h chơi
        Equipment = 6,
        GrindTime = 7,
        Game = 8,
        EventEligiblility = 9,
        Rental = 10,
        ClaimableReward = 11,
        EquipmentEffect = -12,
    }

    /// <summary>
    /// Enum defying all NFT rarity
    /// </summary>
    public enum NFTRarity
    {
        Common = 1,
        Rare = 2,
        Epic = 3,
        Legend = 4
    }
    
    [Serializable]
    public struct SelectedNftResponseModel
    {
        /// <summary>
        /// Current grinding session id. Will be null or empty when not grinding
        /// </summary>
        public string grindId;
        
        /// <summary>
        /// HeroNFTId of grinding hero NFT. Will be null or empty when not grinding
        /// </summary>
        public string nftGrind;
        
        /// <summary>
        /// HeroNFTId of selected hero NFT. Will be null or empty when not selecting any hero nft
        /// </summary>
        public string selected;
    }
    
    /// <summary>
    /// Response from fetch list hero nft api
    /// </summary>
    public struct ListHeroNftItems
    {
        /// <summary>
        /// List hero nft item data in page
        /// </summary>
        public List<HeroNftItem> data;
        
        /// <summary>
        /// Total nft item user have.
        /// This is NOT data.count. This is actual how many NFT user have
        /// </summary>
        public int total;
    }

    /// <summary>
    /// Struct use for holding hero nft item data
    /// </summary>
    [Serializable]
    public struct HeroNftItem : ICustomDefaultable<HeroNftItem>
    {
        /// <summary>NFT id</summary>
        public string id;
        
        public int gen;
        
        /// <summary>Info use by ingame for nft</summary>
        public NftIngameInfo ingameInfo;
        
        /// <summary>Metadata of this NFT</summary>
        public HeroNftMetaData metaData;
        
        /// <summary>A dictionary that contain info for all equipment slot inside hero NFT item. Key is EquipmentItemType. Value is NFTEquipmentInfo</summary>
        public Dictionary<EquipmentItemType, NFTEquipmentInfo> equipmentSet;
        
        /// <summary>Contain info about this NFT grind ability, rewards, speed, stage, ...</summary>
        public CommonGrindInfo grindInfo;
        
        /// <summary>A MarketInfo of this NFT</summary>
        public MarketInfo marketInfo;

        /// <summary>An OnChainInfo of this NFT</summary>
        public OnChainInfo onchainInfo;
        
        public HeroNftItem SetDefault()
        {
            id = string.Empty;
            gen = 0;
            ingameInfo = default;
            metaData = default;

            return this;
        }

        public bool IsEmpty()
        {
            return string.IsNullOrEmpty(id);
        }

        public bool IsInTransaction()
        {
            return !string.IsNullOrEmpty(marketInfo.status);
        }
    }

    /// <summary>
    /// Contain market info of this NFT
    /// </summary>
    [Serializable]
    public struct MarketInfo
    {
        /// <summary>
        /// A string contain status of this NFT. If this NFT is not in any market's transaction, this value will be null or empty.
        /// </summary>
        public string status;
    }

    /// <summary>
    /// Contain common grind info of hero NFT item
    /// </summary>
    [Serializable]
    public struct CommonGrindInfo
    {
        /// <summary>
        /// Contain grind session info of this NFT. If this NFT is not being grinded, this value is null
        /// </summary>
        public GrindStatus status;
        
        /// <summary>
        /// Total reward this NFT has earned through grinding
        /// </summary>
        public float grindPoint;

        /// <summary>
        /// Current grind speed of this NFT (THG/s)
        /// </summary>
        public float currentGrindSpeed;

        /// <summary>
        /// Maximum grind speed this NFT can reach (THG/s)
        /// </summary>
        public float maxGrindSpeed;
        
        /// <summary>
        /// The grind ability of this NFT (value range [0, 1])
        /// </summary>
        public float grindAbility; // Range [0,1]
        
        /// <summary>
        /// Current stage of this NFT
        /// </summary>
        public int stage;
        
        /// <summary>
        /// Maximum stage of this NFT
        /// </summary>
        public int maxStage;
        
        /// <summary>
        /// Is this NFT reached its maximum stage and cannot be grinded anymore
        /// </summary>
        public bool reachMaxStage;
        
        /// <summary>
        /// How many second nft have been grinded today
        /// </summary>
        public float grindTime;
        
        /// <summary>
        /// Maximum second nft can be grinded today
        /// </summary>
        public float maxGrindTime;
        public DateTime nextReset;
        public float equipmentEffect; // Range [0, 1]

        public bool IsGrinding() => status != null && !string.IsNullOrEmpty(status.grindId);
        
        public bool IsMaxLifeTime() => reachMaxStage; // implement later
    }
    
    [Serializable]
    public struct DetailHeroGrindInfo
    {
        public float grindPoint;
        public float grindTime;
        public float currentGrindSpeed;
        public float maxGrindSpeed;
        public float maxGrindTime;
        public int stage;
        public int maxStage;
        public float grindRewardBonus;
        public float maxGrindRewarBonus;
        public GrindStatus status;
    }

    /// <summary>
    /// Contain grind session info
    /// </summary>
    [Serializable]
    public class GrindStatus
    {
        /// <summary>
        /// AppId of app this NFT is grinding
        /// </summary>
        public string appId;
        
        /// <summary>
        /// Grind session ID
        /// </summary>
        public string grindId;
        
        /// <summary>
        /// DateTime of when this NFT is started grind.
        /// </summary>
        public DateTime startGrindAt;
        
        /// <summary>
        /// DateTime of when this NFT is timeout grind when server cannot receive grind signal.
        /// </summary>
        public DateTime timeOut;
    }

    /// <summary>
    /// Struct use for holding hero nft item data
    /// </summary>
    public struct NftIngameInfo
    {
        /// <summary>Game World Type of NFT</summary>
        public GameWorldType gameId;

        /// <summary>Ingame item id (Only used in specific game world that NFT is belong to)</summary>
        public string ingameID;

        /// <summary>Rarity of this NFT</summary>
        public NFTRarity rarity;
        
        /// <summary>Item kind of this item</summary>
        public ItemKind kind;
        
        /// <summary>typeId of this item</summary>
        public int type;
    }

    /// <summary>
    /// Contain Chain info of NFT
    /// </summary>
    public struct OnChainInfo
    {
        /// <summary>
        /// A string contain token id of NFT on chain
        /// </summary>
        public string tokenId;
    }

    /// <summary>
    /// Enum defying item kind of NFT (Hero, Equipment, Ticket, ....)
    /// </summary>
    public enum ItemKind
    {
        Hero = 5,
    }
    
    /// <summary>
    /// Meta data for descript Hero NFT
    /// </summary>
    public struct HeroNftMetaData
    {
        /// <summary>
        /// Display name of this NFT
        /// </summary>
        public string name;
    }

    /// <summary>
    /// Enum defying all Thetan Games
    /// </summary>
    public enum GameWorldType
    {
        ThetanArena = 0,
        ThetanRivals = 1,
        ThetanUGC = 2,
        ThetanImmortal = 3,
        ThetanMarket = 4
    }

    public struct NFTItemDailySummaryDataReponse
    {
        public List<NFTItemDailySummaryData> data;
    }
    
    [Serializable]
    public struct NFTItemDailySummaryData
    {
        public NftIngameInfo ingameInfo;
        public HeroNftMetaData metaData;
        public CommonGrindInfo grindInfo;
    }

    public struct GameDailySummaryDataReponse
    {
        public List<GameDailySummaryData> data;
    }
    
    [Serializable]
    public struct GameDailySummaryData
    {
        public GameWorldType GameWorldType;
        public CommonGrindInfo grindInfo;
    }

    [Serializable]
    public struct GrindNFTStatisticData
    {
        public int grindTimeToday;
        public float grindRewardToday;
        public int incSpinChanceToday;
        public int nftUsed;
    }

    /// <summary>
    /// Contain information about an equipment slot inside hero nft item
    /// </summary>
    public struct NFTEquipmentInfo
    {
        /// <summary>
        /// Require Equipment Type Id
        /// </summary>
        public string equippedId;
        
        /// <summary>
        /// ItemId of equipment item that been equipped in this slot. If this slot is not equipped yet, this field is null or empty
        /// </summary>
        public int requiredTypeId;
    }
}