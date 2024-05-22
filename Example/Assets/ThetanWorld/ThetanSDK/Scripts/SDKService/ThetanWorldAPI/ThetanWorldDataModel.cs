using System;
using System.Collections.Generic;
using ThetanSDK.UI.LuckySpin;

namespace Wolffun.RestAPI.ThetanWorld
{
    [Serializable]
    public struct RequestNFTListRequestModel
    {
        public int limit;
        public int page;
        public string sorts;
        public bool mountStatus;
    }

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
        public string grindId;
        public string nftGrind;
        public string selected;
    }
    
    public struct ListHeroNftItems
    {
        public List<HeroNftItem> data;
        public int total;
    }

    [Serializable]
    public struct HeroNftItem : ICustomDefaultable<HeroNftItem>
    {
        public string id;
        public int gen;
        public NftIngameInfo ingameInfo;
        public HeroNftMetaData metaData;
        public Dictionary<EquipmentItemType, NFTEquipmentInfo> equipmentSet;
        public CommonGrindInfo grindInfo;
        public MarketInfo marketInfo;
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

    [Serializable]
    public struct MarketInfo
    {
        public string status;
    }

    [Serializable]
    public struct CommonGrindInfo
    {
        public GrindStatus status;
        public float grindPoint;
        public float currentGrindSpeed;
        public float maxGrindSpeed;
        public float grindAbility; // Range [0,1]
        public int stage;
        public int maxStage;
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

    [Serializable]
    public class GrindStatus
    {
        public string appId;
        public string grindId;
        public DateTime startGrindAt;
        public DateTime timeOut;
    }

    public struct NftIngameInfo
    {
        public GameWorldType gameId;
        public string ingameID;
        public NFTRarity rarity;
        public ItemKind kind;
        public int type;
    }

    public struct OnChainInfo
    {
        public string tokenId;
    }

    public enum ItemKind
    {
        Hero = 5,
    }
    
    public struct HeroNftMetaData
    {
        public string name;
    }

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

    public struct NFTEquipmentInfo
    {
        public string equippedId;
        public int requiredTypeId;
    }
}