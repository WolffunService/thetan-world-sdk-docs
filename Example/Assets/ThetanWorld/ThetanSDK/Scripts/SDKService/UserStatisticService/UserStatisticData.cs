using System;
using Newtonsoft.Json;
using Wolffun.RestAPI;

namespace ThetanSDK.SDKService.UserStatisticService
{
    public struct UserStatisticData
    {
        public DecimalItemNumber[] grindReward;
        public DecimalItemNumber[] unclaimGrindReward;
        
        
        public DecimalItemNumber[] victoryReward;
        public DecimalItemNumber[] unclaimVictoryReward;
        
        
        public DecimalItemNumber[] airdropReward;
        public DecimalItemNumber[] unclaimAirdropReward;
    }

    public struct LeaderboardData
    {
        [JsonProperty("game")]
        public GameLeaderboardInfo gameLeaderboard;
        
        [JsonProperty("world")]
        public GameLeaderboardInfo worldLeaderboard;
    }

    public enum LeagueLeaderboard
    {
        Wood = 1,
        Stone = 2,
        Iron = 3,
        Bronze = 4,
        Silver = 5,
        Gold = 6,
        Platinum = 7,
        Diamond = 8,
        Legend = 9,
        Champion = 10
    }
    
    public struct GameLeaderboardInfo
    {
        public LeagueLeaderboard league;
        public int rank;
        public int currentSeason;
        
        [JsonProperty("lastRank")]
        public int prevRank;
        
        [JsonProperty("totalUser")]
        public int maxRank;
        
        [JsonProperty("end")]
        public DateTime timeEnd;
        
        [JsonProperty("lpPoint")]
        public float lbPoint;
    }
    
    [Serializable]
    public struct DecimalItemNumber
    {
        [JsonProperty("value")]
        public long Value;
        
        [JsonProperty("decimals")]
        public int Decimal;

        [JsonProperty("type")]
        public ItemRewardType ItemType;
        
        public double GetRealValue()
        {
            return Value / Math.Pow(10, Decimal);
        }
    }

    public enum ItemRewardType : int
    {
        gTHG = 2,
    }
}